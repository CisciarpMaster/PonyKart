using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;
using Math = Mogre.Math;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }

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
		public Degree TurnAngle { get; set; } // 0.3 rads (demo)

		protected readonly Vector3 WheelDirection = Vector3.NEGATIVE_UNIT_Y;
		protected readonly Vector3 WheelAxle = Vector3.NEGATIVE_UNIT_X;
		public bool IsFrontWheel { get; protected set; }

		public WheelID ID { get; private set; }
		public readonly int IntWheelID;

		public WheelTurnState TurnState { get; set; }

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
		RaycastVehicle vehicle;

		public Wheel(Kart owner, Vector3 connectionPoint, WheelID wheelID) {
			kart = owner;
			vehicle = kart.Vehicle;
			ID = wheelID;
			IntWheelID = (int) wheelID;

			Node = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode("wheelNode" + kart.ID + ID);
			Entity = LKernel.GetG<SceneManager>().CreateEntity("wheelNode" + kart.ID + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);
			Node.InheritOrientation = false;

			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;

			LKernel.GetG<PhysicsMain>().PostSimulate += PostSimulate;
		}

		/// <summary>
		/// Makes a wheel at the given position
		/// </summary>
		public void CreateWheel(Vector3 connectionPoint, bool isFrontWheel) {
			vehicle.AddWheel(connectionPoint, WheelDirection, WheelAxle, SuspensionRestLength, Radius, kart.Tuning, isFrontWheel);

			IsFrontWheel = isFrontWheel;

			WheelInfo info = vehicle.GetWheelInfo(IntWheelID);
			info.SuspensionStiffness = SpringStiffness;
			info.WheelDampingRelaxation = SpringDamping;
			info.WheelDampingCompression = SpringCompression;
			info.FrictionSlip = FrictionSlip;
			info.RollInfluence = RollInfluence;
		}

		/// <summary>
		/// Update our node's position and orientation, and also accelerate/brake/turn if we aren't paused
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			WheelInfo info = kart.Vehicle.GetWheelInfo(IntWheelID);
			Node.Position = info.WorldTransform.GetTrans();
			Node.Orientation = info.WorldTransform.ExtractQuaternion();

			if (!Pauser.IsPaused) {
				Accelerate();
				Brake();
				Turn(evt.timeSinceLastFrame);
			}
		}

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate() {
			// if we are trying to accelerate in the opposite direction that we're moving, then brake
			if ((AccelerateMultiplier > 0 && vehicle.CurrentSpeedKmHour < -10) || (AccelerateMultiplier < 0 && vehicle.CurrentSpeedKmHour > 10)) {
				IsBrakeOn = true;
			}
			// if we're mostly stopped and we aren't trying to accelerate, then brake
			else if (AccelerateMultiplier == 0 && (vehicle.CurrentSpeedKmHour > -10 || vehicle.CurrentSpeedKmHour < 10)) {
				IsBrakeOn = true;
			}
			// if we're either mostly stopped or going in the correct direction, take off the brake and accelerate
			else if ((AccelerateMultiplier > 0 && vehicle.CurrentSpeedKmHour > -10) || (AccelerateMultiplier < 0 && vehicle.CurrentSpeedKmHour < 10)) {
				float _motorForce = 0;
				// the wheels with motor force change depending on whether the kart is drifting or not
				// rear-wheel drive, remember!
				if (TurnState == WheelTurnState.Normal) {
					if (ID == WheelID.BackLeft || ID == WheelID.BackRight)
						_motorForce = MotorForce;
				}
				else if (TurnState == WheelTurnState.DriftLeft) {
					if (ID == WheelID.FrontRight || ID == WheelID.BackRight)
						_motorForce = MotorForce;
				}
				else if (TurnState == WheelTurnState.DriftRight) {
					if (ID == WheelID.FrontLeft || ID == WheelID.BackLeft)
						_motorForce = MotorForce;
				}
				vehicle.ApplyEngineForce(_motorForce * AccelerateMultiplier, IntWheelID);
				IsBrakeOn = false;
			}
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		protected void Brake() {
			if (IsBrakeOn) {
				// handbrake
				if (AccelerateMultiplier == 0 && (vehicle.CurrentSpeedKmHour > -10 && vehicle.CurrentSpeedKmHour < 10)) {
					// the point of this is to lock the wheels in place so we don't move when we're stopped
					vehicle.SetBrake(BrakeForce * 100, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = FrictionSlip * 100;
				}
				// normal brake
				else if ((AccelerateMultiplier > 0 && vehicle.CurrentSpeedKmHour < -10) || (AccelerateMultiplier < 0 && vehicle.CurrentSpeedKmHour > 10)) {
					// brake to apply when we're changing direction
					vehicle.SetBrake(BrakeForce, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = FrictionSlip;
				}
				// normal brake
				else {
					// brake to apply when we're just slowing down
					vehicle.SetBrake(BrakeForce * 0.75f, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = FrictionSlip;
				}
			}
			else {
				vehicle.SetBrake(0, IntWheelID);
				vehicle.GetWheelInfo(IntWheelID).FrictionSlip = FrictionSlip;
			}
		}

		// any slower than this and you will have the fully multiplied turn angle
		readonly float slowSpeed = 60;
		// any faster than this and you will have the regular turn angle
		readonly float highSpeed = 160;
		// how much should the wheel's turn angle increase by at slow speeds?
		readonly float speedTurnMultiplierAtSlowSpeeds = 3f;
		// how much to increment the wheel's angle by, each frame
		static readonly Degree steerIncrementTurn = 0.4f;
		// how much to decrement the whee's angle by, each frame
		static readonly Degree steerDecrementTurn = 1f;
		// the angle wheels should try to be at when they aren't turning
		public Degree idealSteerAngle = 0;

		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn(float timeSinceLastFrame) {
			// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
			float speedTurnMultiplier;
			timeSinceLastFrame *= 100;

			// first we figure out what our maximum turn angle is depending on kart speed
			float axleSpeed = vehicle.CurrentSpeedKmHour;
			

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


			// pick whether the wheel can turn or not depending on whether it's drifting
			// only "front" wheels turn!
			float _turnAngle = 0;


				if (TurnState == WheelTurnState.Normal) {
					if (ID == WheelID.FrontLeft || ID == WheelID.FrontRight)
						_turnAngle = TurnAngle.ValueRadians;
				}
				else if (TurnState == WheelTurnState.DriftLeft) {
					if (ID == WheelID.FrontLeft || ID == WheelID.BackLeft)
						_turnAngle = TurnAngle.ValueRadians;
				}
				else if (TurnState == WheelTurnState.DriftRight) {
					if (ID == WheelID.FrontRight || ID == WheelID.BackRight)
						_turnAngle = TurnAngle.ValueRadians;
				}


			// okay so now we know what angle the wheel should try to be at
			float targetSteerAngle = (_turnAngle * TurnMultiplier * speedTurnMultiplier) + idealSteerAngle.ValueRadians;

			float currentAngle = vehicle.GetSteeringValue(IntWheelID);
			float steerChange;

			// now we have to figure out how much we have to change by
			// smooth out the turning


				if (Math.Abs(targetSteerAngle - idealSteerAngle.ValueRadians) < Math.Abs(currentAngle - idealSteerAngle.ValueRadians))
					// we are not turning any more, so the wheels are moving back to their forward positions
					steerChange = steerDecrementTurn.ValueRadians * timeSinceLastFrame;
				else
					// we are turning, so the wheels are moving to their turned positions
					steerChange = steerIncrementTurn.ValueRadians * timeSinceLastFrame;


			// turn the wheels! All of the logic here makes sure that we don't turn further than the wheel can turn,
			// and don't turn too far when we're straightening out


			if (currentAngle < targetSteerAngle) {
				if (currentAngle + steerChange <= targetSteerAngle)
					vehicle.SetSteeringValue(currentAngle + steerChange, IntWheelID);
				// close enough to the ideal angle, so we snap
				else if (currentAngle + steerChange > targetSteerAngle)
					vehicle.SetSteeringValue(targetSteerAngle, IntWheelID);
			}
			else if (currentAngle > targetSteerAngle) {
				if (currentAngle - steerChange >= targetSteerAngle)
					vehicle.SetSteeringValue(currentAngle - steerChange, IntWheelID);
				// can't decrement any more to the ideal angle, so we snap
				else if (currentAngle - steerChange < targetSteerAngle)
					vehicle.SetSteeringValue(targetSteerAngle, IntWheelID);
			}
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			LKernel.GetG<PhysicsMain>().PostSimulate -= PostSimulate;

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

	public enum WheelTurnState {
		/// <summary>
		/// Turn angle is zero
		/// </summary>
		Normal,
		/// <summary>
		/// Turn angle is positive
		/// </summary>
		DriftLeft,
		/// <summary>
		/// Turn angle is negative
		/// </summary>
		DriftRight,
	}
}
