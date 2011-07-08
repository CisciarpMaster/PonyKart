using System.Linq;
using Lymph.Actors;
using Lymph.Core;
using Lymph.Phys;
using Lymph.Stuff;
using Mogre;
using Mogre.PhysX;
using System;

namespace Lymph.Handlers {
	/// <summary>
	/// This handles attaching an antibody to an enemy
	/// </summary>
	public class AntibodyAttachingHandler : IDisposable {

		/// <summary>
		/// hook up to the projectile-enemy event
		/// </summary>
		public AntibodyAttachingHandler() {
			Launch.Log("[Loading] Creating AntibodyAttachingHandler");
			LKernel.Get<ContactReporter>().AddEvent(Groups.ProjectileID, Groups.EnemyID, HandleAntibodyCollision);
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing of AntibodyAttachingHandler");
			LKernel.Get<ContactReporter>().RemoveEvent(Groups.ProjectileID, Groups.EnemyID, HandleAntibodyCollision);
		}

		/// <summary>
		/// Responds to an event when a projectile collides with an enemy. Responds to the Projectile_EnemyOnBodyContact event.
		/// </summary>
		void HandleAntibodyCollision(ContactPair pair, ContactPairFlags flags) {
			Actor actor1 = pair.ActorFirst;
			Actor actor2 = pair.ActorSecond;
			System.Console.WriteLine("Handling antibody/enemy collision...");

			// first we need the antibody that contacted it
			Antibody antibody = actor1.UserData as Antibody;
			Enemy enemy = actor1.UserData as Enemy;

			// find out which is the antibody and which is the enemy
			if (antibody == null) {
				// actor1 is not an antibody
				antibody = actor2.UserData as Antibody;
				if (antibody == null) {
					// neither of them are antibodies so this must've been some other projectile
					return;
				}
			}
			if (enemy == null) {
				// actor1 is not an enemy
				enemy = actor2.UserData as Enemy;
				if (enemy == null) {
					// neither of them are an enemy so this must've been something else
					// not supposed to happen
					throw new System.ArgumentException("[AntibodyAttachingHandler] Neither of the actors in the AntibodyCollision "
						+ "callback were enemies, even though it was a Projectile_EnemyCallback!");
				}
			}

			// so far so good - get the color
			AntigenColour ac = antibody.Colour;
			// check to see if the colors match
			if (enemy.Colours.Contains(ac)
				&& enemy.Antibodies.Count < Constants.MAX_ANTIBODY_ATTACHMENTS
				&& antibody.Actor != null
				&& !antibody.HasBeenAttached) {

					antibody.HasBeenAttached = true;

				// first we have to convert the antibody's position and orientation so they're relative to this cell
				Vector3 pos = enemy.Node.ConvertWorldToLocalPosition(antibody.Node.Position);
				Quaternion orientation = enemy.Node.ConvertWorldToLocalOrientation(antibody.Node.Orientation);

				AntibodyAttachment aa = new AntibodyAttachment("AntibodyAttachment", ac, enemy.Node, antibody.Node, antibody.TimeCreated, pos /** 0.9f*/);

				SphereShapeDesc sphereDesc = new SphereShapeDesc(0.1f, pos * enemy.Node.GetScale() /** 0.45f*/);
				enemy.Actor.CreateShape(sphereDesc); // adds the new sphere collision to the cell's collision
				//enemy.Actor.UpdateMassFromShapes(1, 0.113097f);

				// then we get rid of the original antibody
				if (!antibody.Actor.IsDisposed)
					antibody.QueueDispose();
				// register some stuff
				aa.AttachedEnemy = enemy;
				enemy.Antibodies.Add(aa);

				// for funsies, make black enemies divide on contact
				if (enemy.Colours.Contains(AntigenColour.black)) {
					GenericEnemy splitted = LKernel.Get<Spawner>().Spawn(
						ThingEnum.GenericEnemy, "GenericEnemy", enemy.Node.Position) as GenericEnemy;

					System.Random r = new System.Random();
					Vector3 vec = new Vector3((float) r.NextDouble(), 0, (float) r.NextDouble());
					vec.Normalise();
					System.Console.WriteLine(vec.ToString());
					vec *= enemy.MoveSpeed * 2f;

					splitted.Actor.AddForce(vec);
					enemy.Actor.AddForce(vec * -1);
				}
			}
		}
	}
}
