using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Phys;
using Math = Mogre.Math;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : IDisposable {
		
		
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public WheelShape Shape { get; protected set; }
		public int ID { get; protected set; }

		public float Radius { get; set; }
		public float Suspension { get; set; }
		public float SpringRestitution { get; set; }
		public float SpringDamping { get; set; }
		public float SpringBias { get; set; }

		public float BrakeForce { get; set; }
		public float MotorForce { get; set; }
		public Radian TurnAngle { get; set; }
		public float MaxSpeed { get; set; }

		public float LatExtremumSlip { get; set; }
		public float LatExtremumValue { get; set; }
		public float LatAsymptoteSlip { get; set; }
		public float LatAsymptoteValue { get; set; }
		public float LatStiffnessFactor { get; set; }

		public float LongExtremumSlip { get; set; }
		public float LongExtremumValue { get; set; }
		public float LongAsymptoteSlip { get; set; }
		public float LongAsymptoteValue { get; set; }
		public float LongStiffnessFactor { get; set; }

		Kart kart;

		public Wheel(Kart owner, Vector3 position) {
			kart = owner;

			ID = IDs.New;

			Node = kart.Node.CreateChildSceneNode("wheelNode" + ID, position - new Vector3(0, 0.5f, 0));
			Entity = LKernel.Get<SceneManager>().CreateEntity("wheelNode" + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);

			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		/// <summary>
		/// Makes a wheel shape at the given position
		/// </summary>
		public void CreateWheelShape(Vector3 position) {
			WheelShapeDesc wsd = new WheelShapeDesc();

			// wheel friction is managed by its own functions, so this just stops other things from getting friction over the wheels.
			// Also it speeds it up since physx doesn't have to calculate friction twice.
			wsd.MaterialIndex = LKernel.Get<PhysXMaterials>().NoFrictionMaterial.Index;

			// position and orientation
			wsd.LocalPosition = position;

			// suspension
			float heightModifier = (Suspension + Radius) / Suspension;
			wsd.Suspension.Spring = SpringRestitution * heightModifier;
			wsd.Suspension.Damper = SpringDamping * heightModifier;
			wsd.Suspension.TargetValue = SpringBias * heightModifier;

			// other stuff
			wsd.Radius = Radius;
			wsd.SuspensionTravel = Suspension;
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

		float spin = 0; // rads
		/// <summary>
		/// Update our node's orientation. I'd still like a way to figure out how to update its position based on the suspension, but oh well.
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!LKernel.Get<LevelManager>().IsValidLevel || Shape.IsDisposed || kart.Actor.IsSleeping || Pauser.IsPaused)
				return true;

			spin += (Shape.AxleSpeed * evt.timeSinceLastFrame) / Math.PI;
			spin %= Math.TWO_PI;

			Node.Orientation = new Quaternion().FromGlobalEuler(spin, Shape.SteerAngle, 0);

			return true;
		}

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		/// <param name="multiplier">
		/// Use this to control its speed: 1 is normal forwards, -1 is reverse, 0 is stop. Can set it to 1.2f if you were using a powerup, for example.
		/// </param>
		public void Accelerate(float multiplier) {
			if (Shape.AxleSpeed > MaxSpeed)
				Shape.MotorTorque = 0;
			else
				Shape.MotorTorque = MotorForce * multiplier;
			Shape.BrakeTorque = 0;
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		public void Brake() {
			Shape.MotorTorque = 0;
			Shape.BrakeTorque = BrakeForce;
		}

		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		/// <param name="multiplier">
		/// Looking downwards, 1 rotates counter-clockwise (left), -1 rotates clockwise (right).
		/// </param>
		public void Turn(float multiplier) {
			Shape.SteerAngle = TurnAngle.ValueRadians * multiplier;
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;

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
