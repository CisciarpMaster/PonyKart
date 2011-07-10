using System;
using System.Collections.ObjectModel;
using Ponykart.Phys;
using Ponykart.Stuff;
using Mogre;
using Mogre.PhysX;

namespace Ponykart.Actors {
	public class Kart : DynamicThing {
		
		protected override ShapeDesc ShapeDesc {
			get { return new CapsuleShapeDesc(0.3f, 0.001f); }
		}
		protected override uint DefaultCollisionGroupID {
			get { return Groups.CollidablePushableID; }
		}
		protected override string DefaultModel {
			get { return "primitives/box.mesh"; }
		}
		protected override MoveBehaviour DefaultMoveBehaviour {
			get { return MoveBehaviour.TOWARDS_PLAYER; }
		}
		protected override float DefaultMoveSpeed {
			get { return 0.08f; }
		}
		protected override string DefaultMaterial {
			get { return null; }
		}

		/// <summary>
		/// Things we will be disposing at the end
		/// </summary>
		Collection<IDisposable> toDispose = new Collection<IDisposable>();

		public Kart(ThingTemplate tt) : base(tt)
		{
			Launch.Log("Creating GenericEnemy #" + ID + " with name \"" + tt.StringTokens["Name"] + "\"");
		}

		protected override void CreateMoreMogreStuff()
		{
			// the box mesh is too big, so we have to scale it down a bit
			Node.SetScale(0.3f, 0.3f, 0.3f);

			// Outer glow
			Entity alphaGlowEntity = LKernel.Get<SceneManager>().CreateEntity("GE_OuterGlow" + ID, "primitives/ellipsoid.mesh");
			alphaGlowEntity.SetMaterialName("GE_BalloonGlow_red");
			SceneNode alphaGlowNode = Node.CreateChildSceneNode("GE_OuterGlowNode" + ID);
			alphaGlowNode.AttachObject(alphaGlowEntity);

			// add a ribbon
			CreateRibbon(4, 10, ColourValue.Red, 0.6f);

			// and then make sure we dispose this stuff when we're done
			toDispose.Add(alphaGlowEntity);
			toDispose.Add(alphaGlowNode);
		}

		public override void Dispose() {
			foreach (var v in toDispose) {
				v.Dispose();
			}
			base.Dispose();
		}
	}
}
