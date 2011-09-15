using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Physics;
using Math = Mogre.Math;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public int ID { get; protected set; }

		public float Radius { get; set; } // 0.5 (lymph)
		public float Width { get; set; } // 0.4 (demo)
		public float SuspensionRestLength { get; set; } // 0.6 (demo) // 0.3 (zg)

		public float SpringStiffness { get; set; } // 100 (zg)
		public float SpringCompression { get; set; } // 4.2 (zg)
		public float SpringDamping { get; set; } // 20 (zg)
		public float FrictionSlip { get; set; } // 1000 (demo)
		public float RollInfluence { get; set; } // 1 (zg)

		public float BrakeForce { get; set; }
		public float MotorForce { get; set; }
		public Radian TurnAngle { get; set; } // 0.3 (demo)

		protected Vector3 WheelDirection = Vector3.NEGATIVE_UNIT_Y;
		protected Vector3 WheelAxle = Vector3.NEGATIVE_UNIT_X;
		public float IsFrontWheel { get; set; }
		public WheelID WheelID { get; private set; }

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

		Kart Kart;

		public Wheel(Kart owner, Vector3 connectionPoint, WheelID wheelID) {
			Kart = owner;
			WheelID = wheelID;

			ID = IDs.New;

			Node = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode("wheelNode" + ID);
			Entity = LKernel.GetG<SceneManager>().CreateEntity("wheelNode" + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);
			Node.InheritOrientation = false;

			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;

			LKernel.GetG<PhysicsMain>().PreSimulate += PreSimulate;
		}

		/// <summary>
		/// Makes a wheel at the given position
		/// </summary>
		public void CreateWheel(Vector3 connectionPoint, bool isFrontWheel) {
			Kart.Vehicle.AddWheel(connectionPoint, WheelDirection, WheelAxle, SuspensionRestLength, Radius, Kart.Tuning, isFrontWheel);

			WheelInfo info = Kart.Vehicle.GetWheelInfo((int) WheelID);
			info.SuspensionStiffness = SpringStiffness;
			info.WheelDampingRelaxation = SpringDamping;
			info.WheelDampingCompression = SpringCompression;
			info.FrictionSlip = FrictionSlip;
			info.RollInfluence = RollInfluence;
		}


		/// <summary>
		/// Update our node's orientation. I'd still like a way to figure out how to update its position based on the suspension, but oh well.
		/// </summary>
		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!LKernel.GetG<LevelManager>().IsValidLevel || Pauser.IsPaused)
				return;

			Accelerate();
			Brake();
			Turn(evt.timeSinceLastFrame);
		}

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate() {
			Kart.Vehicle.ApplyEngineForce(MotorForce * AccelerateMultiplier, (int) WheelID);
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		protected void Brake() {
			if (IsBrakeOn)
				Kart.Vehicle.SetBrake(BrakeForce, (int) WheelID);
			else
				Kart.Vehicle.SetBrake(0, (int) WheelID);
		}

		readonly float slowSpeed = 40;
		readonly float highSpeed = 110;
		readonly float speedTurnMultiplierAtSlowSpeeds = 2.5f;
		float idealSteerAngle = 0;
		static readonly float steerIncrement = Math.PI / 90;
		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn(float timeSinceLastFrame) {
			// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
			float speedTurnMultiplier = 1;
			timeSinceLastFrame *= 100;

			float axleSpeed = Kart.Vehicle.CurrentSpeedKmHour;
			// less than the slow speed = extra turn multiplier
			if (axleSpeed < slowSpeed)
				speedTurnMultiplier = speedTurnMultiplierAtSlowSpeeds;
			// more than the high speed = no extra multiplier
			else if (axleSpeed > highSpeed)
				speedTurnMultiplier = 1;
			// somewhere in between = time for a cosine curve!
			else {
				float relativeSpeed = axleSpeed - slowSpeed;
				float maxRelativeSpeed = highSpeed - slowSpeed;
				speedTurnMultiplier = 1 + (Math.Cos((relativeSpeed * Math.PI) / (maxRelativeSpeed * 2f)) * (speedTurnMultiplierAtSlowSpeeds - 1f));
			}
			idealSteerAngle = TurnAngle.ValueRadians * TurnMultiplier * speedTurnMultiplier;

			float currentAngle = Kart.Vehicle.GetSteeringValue((int) WheelID);
			float thisSteerIncr = steerIncrement * timeSinceLastFrame;
			
			// smooth out the turning
			if (currentAngle < idealSteerAngle) {
				if (currentAngle + thisSteerIncr <= idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(currentAngle + thisSteerIncr, (int) WheelID);
				else if (currentAngle + thisSteerIncr > idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(idealSteerAngle, (int) WheelID);
			}
			else if (currentAngle > idealSteerAngle) {
				if (currentAngle - thisSteerIncr >= idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(currentAngle - thisSteerIncr, (int) WheelID);
				else if (currentAngle - thisSteerIncr < idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(idealSteerAngle, (int) WheelID);
			}
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			LKernel.GetG<PhysicsMain>().PreSimulate -= PreSimulate;

			Node.Dispose();
			Node = null;
			Entity.Dispose();
			Entity = null;

			base.Dispose(disposing);
		}
	}

	public enum WheelID {
		/// <summary>
		/// 0
		/// </summary>
		FrontLeft = 0,
		/// <summary>
		/// 1
		/// </summary>
		FrontRight = 1,
		/// <summary>
		/// 2
		/// </summary>
		BackLeft = 2,
		/// <summary>
		/// 3
		/// </summary>
		BackRight = 3,
	}
}
