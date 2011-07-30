using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Phys;
using Ponykart.Stuff;
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

		// we use these three things to control the wheels
		/// <summary>
		/// 1 for forwards, -1 for backwards, 0 for no torque
		/// </summary>
		public float AccelerateMultiplier { get; set; }
		/// <summary>
		/// 1 for left, -1 for right
		/// </summary>
		public float TurnMultiplier { get; set; }
		/// <summary>
		/// true for brake on, false for brake off
		/// </summary>
		public bool IsBrakeOn { get; set; }

		Kart kart;

		public Wheel(Kart owner, Vector3 position) {
			kart = owner;

			ID = IDs.New;

			Node = kart.Node.CreateChildSceneNode("wheelNode" + ID, position - new Vector3(0, 0.5f, 0));
			Entity = LKernel.Get<SceneManager>().CreateEntity("wheelNode" + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);

			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;

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

			// temporary hack
			if ((Shape.AxleSpeed > MaxSpeed && LKernel.Get<KeyBindingManager>().IsKeyPressed(LKey.Accelerate))
			   || Shape.AxleSpeed < -MaxSpeed && LKernel.Get<KeyBindingManager>().IsKeyPressed(LKey.Reverse))
			{
				Shape.MotorTorque = 0;
			}
			else
				Accelerate();

			Brake();
			Turn();

			/////////////////

			spin += (Shape.AxleSpeed * evt.timeSinceLastFrame) / Math.PI;
			spin %= Math.TWO_PI;

			Node.Orientation = new Quaternion().FromGlobalEuler(spin, Shape.SteerAngle, 0);

			/////////////////
#if DEBUG
			if (drawLines && kart == LKernel.Get<Players.PlayerManager>().MainPlayer.Kart) {
				DebugDrawer.Singleton.BuildLine(Shape.GlobalPosition, lastPos, ColourValue.Blue, 1);
				lastPos = Shape.GlobalPosition;
			}
#endif

			return true;
		}
		Vector3 lastPos = Vector3.ZERO;
		public static bool drawLines = true;

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate() {
			Shape.MotorTorque = MotorForce * AccelerateMultiplier;
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		protected void Brake() {
			if (IsBrakeOn)
				Shape.BrakeTorque = BrakeForce;
			else
				Shape.BrakeTorque = 0;
		}

		float slowSpeed = 0;
		float highSpeed = 40;
		float idealSteerAngle = 0;
		static readonly float one_degree = Math.PI / 180;
		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn() {
			// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
			float speedTurnMultiplier;
			if (Shape.AxleSpeed < slowSpeed)
				speedTurnMultiplier = 2;
			else if (Shape.AxleSpeed > highSpeed)
				speedTurnMultiplier = 1;
			else {
				float relativeSpeed = Shape.AxleSpeed - slowSpeed;
				float maxRelativeSpeed = highSpeed - slowSpeed;
				speedTurnMultiplier = 1 + Math.Cos((relativeSpeed * Math.PI) / (maxRelativeSpeed * 2));
			}
			idealSteerAngle = TurnAngle.ValueRadians * TurnMultiplier * speedTurnMultiplier;
			
			// smooth out the turning
			if (Shape.SteerAngle < idealSteerAngle) {
				if (Shape.SteerAngle + one_degree <= idealSteerAngle)
					Shape.SteerAngle += one_degree;
				else if (Shape.SteerAngle + one_degree > idealSteerAngle)
					Shape.SteerAngle = idealSteerAngle;
			}
			else if (Shape.SteerAngle > idealSteerAngle) {
				if (Shape.SteerAngle - one_degree >= idealSteerAngle)
					Shape.SteerAngle -= one_degree;
				else if (Shape.SteerAngle - one_degree < idealSteerAngle)
					Shape.SteerAngle = idealSteerAngle;
			}
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}
