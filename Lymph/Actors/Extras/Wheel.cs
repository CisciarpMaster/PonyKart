using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Levels;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : IDisposable {
		// lat = sideways grip, long = forwards grip
		//										1.0f				 0.02f						2.0f					   0.01f						1000000f
		public static float LatExtremumSlip = 1.0f, LatExtremumValue = 0.05f, LatAsymptoteSlip = 5.0f, LatAsymptoteValue = 0.002f, LatStiffnessFactor = 1000000f,
							LongExtremumSlip = 1.0f, LongExtremumValue = 0.05f, LongAsymptoteSlip = 2.0f, LongAsymptoteValue = 0.01f, LongStiffnessFactor = 1000000f;
		
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public WheelShape Shape { get; protected set; }
		public int ID { get; protected set; }

		Kart kart;

		public Wheel(Kart owner, Vector3 position) {
			kart = owner;

			ID = IDs.New;

			var sceneMgr = LKernel.Get<SceneManager>();

			Node = kart.Node.CreateChildSceneNode("wheelNode" + ID, position - new Vector3(0, 0.5f, 0));
			Entity = sceneMgr.CreateEntity("wheelNode" + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);

			CreateWheelShape(position);

		}

		/// <summary>
		/// Makes a wheel at the given position
		/// </summary>
		protected void CreateWheelShape(Vector3 position) {
			WheelShapeDesc wsd = new WheelShapeDesc();
			// default properties
			float radius = 0.5f,
				suspension = 0.5f, // how "long" is the suspension spring?
				springRestitution = 7000, // how much force it'll absorb
				springDamping = 800f, // bounciness: bigger = less bouncy
				springBias = 0f;

			// kinda a hack for now
			//wsd.MaterialIndex = PhysXMain.noFrictionMaterial.Index;

			// position and orientation
			wsd.LocalPosition = position;
			/*Quaternion q = new Quaternion();
			q.FromAngleAxis(new Degree(90), Vector3.UNIT_Y);
			wsd.LocalOrientation = q.ToRotationMatrix();*/

			// suspension
			float heightModifier = (suspension + radius) / suspension;
			wsd.Suspension.Spring = springRestitution * heightModifier;
			wsd.Suspension.Damper = springDamping * heightModifier;
			wsd.Suspension.TargetValue = springBias * heightModifier;

			// other stuff
			wsd.Radius = radius;
			wsd.SuspensionTravel = suspension;
			wsd.InverseWheelMass = 0.1f; // physx docs say "not given!? TODO", whatever that means

			// tyre... things. Something to do with gripping surfaces at different velocities
			// this one has to do with sideways grip for things like handling
			wsd.LateralTireForceFunction.ExtremumSlip = LatExtremumSlip;
			wsd.LateralTireForceFunction.ExtremumValue = LatExtremumValue;
			wsd.LateralTireForceFunction.AsymptoteSlip = LatAsymptoteSlip;
			wsd.LateralTireForceFunction.AsymptoteValue = LatAsymptoteValue;
			wsd.LateralTireForceFunction.StiffnessFactor = LatStiffnessFactor;
			// this one has to do with forwards grip for things like acceleration
			wsd.LongitudalTireForceFunction.ExtremumSlip = LongExtremumSlip;
			wsd.LongitudalTireForceFunction.ExtremumValue = LongExtremumValue;
			wsd.LongitudalTireForceFunction.AsymptoteSlip = LongAsymptoteSlip;
			wsd.LongitudalTireForceFunction.AsymptoteValue = LongAsymptoteValue;
			wsd.LongitudalTireForceFunction.StiffnessFactor = LongStiffnessFactor;

			Shape = kart.Actor.CreateShape(wsd) as WheelShape;
		}

		public void Accelerate(float speed) {
			Shape.MotorTorque = speed;
			Shape.BrakeTorque = 0;
		}

		public void Brake(float force) {
			Shape.MotorTorque = 0;
			Shape.BrakeTorque = force;
		}

		public void Turn(Radian angle) {
			Shape.SteerAngle = angle.ValueRadians;
		}

		public void UpdateAngle(Quaternion orientation) {
			Node.Orientation = orientation;
		}

		public void Dispose() {
			var sceneMgr = LKernel.Get<SceneManager>();

			if (Node != null) {
				if (LKernel.Get<LevelManager>().IsValidLevel) {
					sceneMgr.DestroyEntity(Entity);
					sceneMgr.DestroySceneNode(Node);
				}
				Entity.Dispose();
				Node.Dispose();
				Entity = null;
				Node = null;
			}
		}
	}
}
