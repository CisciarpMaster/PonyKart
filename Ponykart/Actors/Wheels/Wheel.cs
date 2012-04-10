using System.Collections.Generic;
using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }

#region properties
#region readonly properties
		/// <summary>
		/// The radius of the wheel
		/// </summary>
		public readonly float Radius; // 0.5 (lymph)
		/// <summary>
		/// The width of the wheel
		/// </summary>
		public readonly float Width; // 0.4 (demo)
		/// <summary>
		/// The length of the suspension when it's fully extended
		/// </summary>
		protected readonly float SuspensionRestLength; // 0.6 (demo) // 0.3 (zg)

		/// <summary>
		/// The stiffness of the suspension spring. Higher values can help make the suspension return to rest length more quickly,
		/// but can also make the suspension oscillate.
		/// </summary>
		protected readonly float SpringStiffness; // 100 (zg)
		/// <summary>
		/// I have no idea what this does, since changing it doesn't seem to have any effect on the suspension whatsoever
		/// </summary>
		protected readonly float SpringCompression; // 4.2 (zg)
		/// <summary>
		/// How much force the suspension spring can absorb before returning to rest length. Higher numbers can make the kart smoother,
		/// but too high and the kart starts to shake (around 100)
		/// </summary>
		protected readonly float SpringDamping; // 20 (zg)
		/// <summary>
		/// The friction of the wheel. Higher numbers give more friction, 0 gives no friction.
		/// </summary>
		public readonly float FrictionSlip; // 1000 (demo)
		/// <summary>
		/// How much the wheel resists rolling (around its "forward" axis). Lower numbers give more resistance.
		/// </summary>
		protected readonly float RollInfluence; // 1 (zg)

		/// <summary>
		/// How much force the wheel exerts when braking.
		/// </summary>
		protected readonly float BrakeForce;
		/// <summary>
		/// How much force the wheel's motor exerts, if it's a back wheel.
		/// </summary>
		protected readonly float MotorForce;
		/// <summary>
		/// (RADIANS) The maximum amount the wheel can turn by, if it's a front wheel.
		/// </summary>
		protected readonly float MaxTurnAngle; // 0.3 rads (demo)
		/// <summary>
		/// Which way is "up"?
		/// </summary>
		protected readonly Vector3 WheelDirection = Vector3.NEGATIVE_UNIT_Y;
		/// <summary>
		/// Which axis does the wheel rotate around?
		/// </summary>
		protected readonly Vector3 WheelAxle = Vector3.NEGATIVE_UNIT_X;
		/// <summary>
		/// any slower than this and you will have the fully multiplied turn angle
		/// </summary>
		protected readonly float SlowSpeed;
		/// <summary>
		/// any faster than this and you will have the regular turn angle
		/// </summary>
		protected readonly float HighSpeed;
		/// <summary>
		/// how much should the wheel's turn angle increase by at slow speeds?
		/// </summary>
		protected readonly float SlowTurnMultiplier;
		/// <summary>
		/// how much should the wheel's turn angle increase by when drifting?
		/// </summary>
		protected readonly float DriftingTurnMultiplier;
		/// <summary>
		/// (RADIANS) how much to increment the wheel's angle by, each frame
		/// </summary>
		protected readonly float SteerIncrementTurn;
		/// <summary>
		/// (RADIANS) how much to decrement the wheel's angle by, each frame
		/// </summary>
		protected readonly float SteerDecrementTurn;
#endregion

		/// <summary>
		/// Lets us keep track of which wheel this is on the kart
		/// </summary>
		public WheelID ID { get; private set; }
		/// <summary>
		/// Since we want the ID number of this wheel in int form so much, this is used to keep track of it without casting it every time.
		/// </summary>
		public readonly int IntWheelID;
		/// <summary>
		/// Keeps track of whether we're drifting or not, and if we are, which direction we're moving in.
		/// </summary>
		public WheelDriftState DriftState { get; set; }
		/// <summary>
		/// The point on the kart where this wheel is connected.
		/// </summary>
		protected Vector3 AxlePoint;

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
		/// <summary>
		/// (RADIANS) the angle the wheel should try to be at when they aren't turning
		/// </summary>
		public float IdealSteerAngle { get; set; }
		/// <summary>
		/// The current friction of the wheel
		/// </summary>
		public float Friction { get; set; } 

		readonly Kart kart;
		readonly RaycastVehicle vehicle;
#endregion

		/// <summary>
		/// This should only be used by the WheelFactory
		/// </summary>
		/// <param name="owner">Which kart is the wheel attached to?</param>
		/// <param name="connectionPoint">Where is the wheel attached?</param>
		/// <param name="wheelID">ID number of the wheel</param>
		/// <param name="dict">The properties and values from the .wheel file this wheel was built from</param>
		/// <param name="meshName">The filename of the mesh we should use for this wheel</param>
		public Wheel(Kart owner, Vector3 connectionPoint, WheelID wheelID, IDictionary<string, float> dict, string meshName) {
			// set up these
			kart = owner;
			ID = wheelID;
			vehicle = kart.Vehicle;

			// set up our readonlies
			Radius = dict["Radius"];
			Width = dict["Width"];
			SuspensionRestLength = dict["SuspensionRestLength"];
			SpringStiffness = dict["SpringStiffness"];
			SpringCompression = dict["SpringCompression"];
			SpringDamping = dict["SpringDamping"];
			Friction = dict["FrictionSlip"];
			RollInfluence = dict["RollInfluence"];
			BrakeForce = dict["BrakeForce"];
			MotorForce = dict["MotorForce"];
			MaxTurnAngle = new Degree(dict["TurnAngle"]).ValueRadians;
			SlowSpeed = dict["SlowSpeed"];
			HighSpeed = dict["HighSpeed"];
			SlowTurnMultiplier = dict["SlowTurnMultiplier"];
			DriftingTurnMultiplier = dict["DriftingTurnMultiplier"];
			SteerIncrementTurn = new Degree(dict["SteerIncrementTurn"]).ValueRadians;
			SteerDecrementTurn = new Degree(dict["SteerDecrementTurn"]).ValueRadians;

			// give our fields some default values
			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;
			DriftState = WheelDriftState.None;
			IntWheelID = (int) wheelID;
			FrictionSlip = Friction;
			IdealSteerAngle = 0f;

			// need to tell bullet whether it's a front wheel or not
			bool isFrontWheel;
			if (ID == WheelID.FrontLeft || ID == WheelID.FrontRight)
				isFrontWheel = true;
			else
				isFrontWheel = false;

			vehicle.AddWheel(connectionPoint, WheelDirection, WheelAxle, SuspensionRestLength, Radius, kart.Tuning, isFrontWheel);

			WheelInfo info = vehicle.GetWheelInfo(IntWheelID);
			info.SuspensionStiffness = SpringStiffness;
			info.WheelDampingRelaxation = SpringDamping;
			info.WheelDampingCompression = SpringCompression;
			info.FrictionSlip = Friction;
			info.RollInfluence = RollInfluence;

			AxlePoint = connectionPoint + new Vector3(0, -SuspensionRestLength, 0);

			// create our node and entity
			Node = owner.RootNode.CreateChildSceneNode("wheelNode" + kart.ID + ID, AxlePoint);
			Entity = LKernel.GetG<SceneManager>().CreateEntity("wheelNode" + kart.ID + ID, meshName);
			Node.AttachObject(Entity);
			Node.InheritOrientation = false;

			Node.Orientation = kart.ActualOrientation;//kart.Vehicle.GetWheelInfo(IntWheelID).WorldTransform.ExtractQuaternion();

			// and then hook up to the event
			PhysicsMain.PostSimulate += PostSimulate;
		}

		/// <summary>
		/// Update our node's position and orientation and accelerate/brake/turn if we aren't paused
		/// </summary>
		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt) {
			if (!Pauser.IsPaused) {
				WheelInfo info = kart.Vehicle.GetWheelInfo(IntWheelID);
				if (kart.Body.IsActive && (kart.VehicleSpeed > 1 || kart.VehicleSpeed < -1)) {
					// don't change the kart's orientation when we're drifting
					if (kart.IsDriftingAtAll || System.Math.Abs(info.Steering) > System.Math.Abs(MaxTurnAngle * speedTurnMultiplier)) {
						Node.Orientation = kart.ActualOrientation;
					}
					else {
						Node.Orientation = info.WorldTransform.ExtractQuaternion();
					}
				}
				else {
					// TODO: fix it so wheels aren't spinning when we're stopped, but they can still move left and right
					Node.Orientation = info.WorldTransform.ExtractQuaternion();
				}

				// the wheel sorta "comes off" when it's moving quickly in the air, so we only need to update the translation then
				if (!kart.IsInAir) {
					Vector3 trans = info.WorldTransform.GetTrans();
					Node.SetPosition(AxlePoint.x, kart.RootNode.ConvertWorldToLocalPosition(trans).y, AxlePoint.z);
				}
				else {
					Node.Position = AxlePoint;
				}
			
				Accelerate();
				Brake();
				Turn(evt.timeSinceLastFrame);
			}
		}

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate() {
			float speed = vehicle.CurrentSpeedKmHour;
			// if we are trying to accelerate in the opposite direction that we're moving, then brake
			if ((AccelerateMultiplier > 0 && speed < -2) || (AccelerateMultiplier < 0 && speed > 2))
			{
				IsBrakeOn = true;
			}
			// if we're mostly stopped and we aren't trying to accelerate, then brake
			else if (AccelerateMultiplier == 0 && (speed > -2 || speed < 2))
			{
				IsBrakeOn = true;
			}
			// if we're either mostly stopped or going in the correct direction, take off the brake and accelerate
			else if ((AccelerateMultiplier > 0 && speed > -2) || (AccelerateMultiplier < 0 && speed < 2)) {
				float _motorForce = 0;
				// the wheels with motor force change depending on whether the kart is drifting or not
				// rear-wheel drive, remember!
				if (DriftState == WheelDriftState.None) {
					if (ID == WheelID.BackLeft || ID == WheelID.BackRight)
						_motorForce = MotorForce;
				}
				else if (DriftState == WheelDriftState.Left) {
					if (ID == WheelID.FrontRight || ID == WheelID.BackRight)
						_motorForce = MotorForce;
				}
				else if (DriftState == WheelDriftState.Right) {
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
				float speed = vehicle.CurrentSpeedKmHour;
				// handbrake
				if (AccelerateMultiplier == 0 && (speed > -2 && speed < 2)) {
					// the point of this is to lock the wheels in place so we don't move when we're stopped
					vehicle.SetBrake(10000, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = 10000;
				}
				// normal brake
				else if ((AccelerateMultiplier > 0 && speed < -2) || (AccelerateMultiplier < 0 && speed > 2)) {
					// brake to apply when we're changing direction
					vehicle.SetBrake(BrakeForce, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = Friction;
				}
				// normal brake
				else {
					// brake to apply when we're just slowing down
					vehicle.SetBrake(BrakeForce * 0.75f, IntWheelID);
					vehicle.GetWheelInfo(IntWheelID).FrictionSlip = Friction;
				}
			}
			else {
				vehicle.SetBrake(0, IntWheelID);
				vehicle.GetWheelInfo(IntWheelID).FrictionSlip = Friction;
			}
		}

		// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
		float speedTurnMultiplier;

		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn(float timeSinceLastFrame) {
			
			timeSinceLastFrame *= 100;

			// first we figure out what our maximum turn angle is depending on kart speed
			float axleSpeed = vehicle.CurrentSpeedKmHour;


			if (DriftState == WheelDriftState.None) {
				// less than the slow speed = extra turn multiplier
				if (axleSpeed < SlowSpeed)
					speedTurnMultiplier = SlowTurnMultiplier;
				// more than the high speed = no extra multiplier
				else if (axleSpeed > HighSpeed)
					speedTurnMultiplier = 1;
				// somewhere in between = time for a cosine curve!
				else {
					float relativeSpeed = axleSpeed - SlowSpeed;
					float maxRelativeSpeed = HighSpeed - SlowSpeed;
					speedTurnMultiplier = 1 + (Mogre.Math.Cos((relativeSpeed * Mogre.Math.PI) / (maxRelativeSpeed * 2f)) * (SlowTurnMultiplier - 1f));
				}
			}
			// no multiplier when we're drifting
			else {
				speedTurnMultiplier = DriftingTurnMultiplier;
			}


			// pick whether the wheel can turn or not depending on whether it's drifting
			// only "front" wheels turn!
			float _turnAngle = 0;

			if ((DriftState == WheelDriftState.None && (ID == WheelID.FrontLeft || ID == WheelID.FrontRight))
				|| (DriftState == WheelDriftState.Left && (ID == WheelID.FrontLeft || ID == WheelID.BackLeft))
				|| (DriftState == WheelDriftState.Right && (ID == WheelID.FrontRight || ID == WheelID.BackRight)))
			{
				_turnAngle = MaxTurnAngle;
			}


			// okay so now we know what angle the wheel should try to be at
			float targetSteerAngle = (_turnAngle * TurnMultiplier * speedTurnMultiplier) + IdealSteerAngle;

			float currentAngle = vehicle.GetSteeringValue(IntWheelID);
			float steerChange;

			// now we have to figure out how much we have to change by
			// smooth out the turning

			if (DriftState == WheelDriftState.None) {
				if (System.Math.Abs(targetSteerAngle - IdealSteerAngle) < System.Math.Abs(currentAngle - IdealSteerAngle))
					// we are not turning any more, so the wheels are moving back to their forward positions
					steerChange = SteerDecrementTurn * timeSinceLastFrame;
				else
					// we are turning, so the wheels are moving to their turned positions
					steerChange = SteerIncrementTurn * timeSinceLastFrame;
			}
			else {
				steerChange = SteerIncrementTurn * timeSinceLastFrame;
			}


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

			PhysicsMain.PostSimulate -= PostSimulate;

			Node.Dispose();
			Node = null;
			Entity.Dispose();
			Entity = null;

			base.Dispose(disposing);
		}
	}
}
