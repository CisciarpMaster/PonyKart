using System;
using BulletSharp;
using Ponykart.Physics;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for karts. Eventually this'll be abstract.
	/// Z is forwards!
	/// </summary>
	public class Kart : LThing {
		public int OwnerID { get; set; }
		protected override MotionState InitializationMotionState {
			get { return new MogreMotionState(this, SpawnPosition, SpawnOrientation, RootNode); }
		}
		public float MaxSpeed { get; set; }
		public float MaxReverseSpeed { get; set; }
		public float MaxSpeedSquared { get; private set; }
		public float MaxReverseSpeedSquared { get; private set; }
		/// <summary>
		/// Should only be set by KartHandler
		/// </summary>
		public bool IsInAir { get; set; }
		/// <summary>
		/// For the bounce you do before a drift. Used to tell whether we should start drifting when we land.
		/// </summary>
		public bool IsBouncing { get; set; }
		/// <summary>
		/// When you're actually drifting
		/// </summary>
		public bool IsDrifting { get; set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		public RaycastVehicle Vehicle { get; protected set; }
		public RaycastVehicle.VehicleTuning Tuning { get; protected set; }
		protected VehicleRaycaster Raycaster;


		public Kart(ThingBlock block, ThingDefinition def) : base(block, def) {
			MaxSpeed = def.GetFloatProperty("maxspeed", 60f);
			MaxReverseSpeed = def.GetFloatProperty("maxreversespeed", 20f);
			MaxSpeedSquared = MaxSpeed * MaxSpeed;
			MaxReverseSpeedSquared = MaxReverseSpeed * MaxReverseSpeed;
			IsInAir = false;
		}

		/// <summary>
		/// After we create our RigidBody, we turn it into a vehicle
		/// </summary>
		protected override void PostCreateBody(ThingDefinition def) {
			Body.CcdMotionThreshold = 0.001f;
			Body.CcdSweptSphereRadius = 0.2f;
			//Body.ActivationState = ActivationState.DisableDeactivation;

			Raycaster = new DefaultVehicleRaycaster(LKernel.GetG<PhysicsMain>().World);
			Tuning = new RaycastVehicle.VehicleTuning();
			Vehicle = new RaycastVehicle(Tuning, Body, Raycaster);
			Vehicle.SetCoordinateSystem(0, 1, 2); // I have no idea what this does... I'm assuming something to do with a matrix?

			LKernel.GetG<PhysicsMain>().World.AddAction(Vehicle);

			var wheelFac = LKernel.GetG<WheelFactory>();
			string frontWheelName = def.GetStringProperty("frontwheel", null);
			string backWheelName = def.GetStringProperty("backwheel", null);
			WheelFL = wheelFac.CreateWheel(frontWheelName, WheelID.FrontLeft, this, def.GetVectorProperty("frontleftwheelposition", null), true);
			WheelFR = wheelFac.CreateWheel(frontWheelName, WheelID.FrontRight, this, def.GetVectorProperty("frontrightwheelposition", null), true);
			WheelBL = wheelFac.CreateWheel(backWheelName, WheelID.BackLeft, this, def.GetVectorProperty("backleftwheelposition", null), false);
			WheelBR = wheelFac.CreateWheel(backWheelName, WheelID.BackRight, this, def.GetVectorProperty("backrightwheelposition", null), false);
		}

		private float _accelerate;
		/// <summary>
		/// Sets the motor torque of all wheels and sets their brake torque to 0.
		/// </summary>
		public float Acceleration {
			get {
				return _accelerate;
			}
			set {
				Body.Activate();
				this._accelerate = value;

				WheelBR.AccelerateMultiplier =
					WheelBL.AccelerateMultiplier =
					WheelFR.AccelerateMultiplier =
					WheelFL.AccelerateMultiplier
						= value;

				WheelBR.IsBrakeOn =
					WheelBL.IsBrakeOn =
					WheelFR.IsBrakeOn =
					WheelFL.IsBrakeOn
						= false;
			}
		}

		/// <summary>
		/// Sets the motor torque of all wheels to 0 and applies a brake torque.
		/// </summary>
		[Obsolete]
		public void Brake() {
			WheelBR.IsBrakeOn =
				WheelBL.IsBrakeOn =
				WheelFR.IsBrakeOn =
				WheelFL.IsBrakeOn
					= true;

			WheelBR.AccelerateMultiplier =
				WheelBL.AccelerateMultiplier =
				WheelFR.AccelerateMultiplier =
				WheelFL.AccelerateMultiplier
					= 0;
		}

		private float _multiplier;
		/// <summary>
		/// Turns the wheels
		/// </summary>
		public float TurnMultiplier {
			get {
				return _multiplier;
			}
			set {
				this._multiplier = value;

				WheelFR.TurnMultiplier =
					WheelFL.TurnMultiplier =
					WheelBR.TurnMultiplier =
					WheelBL.TurnMultiplier
						= value;
			}
		}

		/// <summary>
		/// Make the kart bounce up in preparation for drifting, but only if it isn't in the air already
		/// </summary>
		public void Bounce() {
			Body.Activate();

			IsBouncing = true;
			IsDrifting = false;

			if (!IsInAir) {
				Body.LinearVelocity += RootNode.GetLocalYAxis() * 10;
			}
		}

		public void StartDrifting() {
			IsDrifting = true;
			WheelFL.idealSteerAngle = WheelFR.idealSteerAngle = WheelBL.idealSteerAngle = WheelBR.idealSteerAngle = 90;
		}

		public void StopDrifting() {
			IsDrifting = false;
			WheelFL.idealSteerAngle = WheelFR.idealSteerAngle = WheelBL.idealSteerAngle = WheelBR.idealSteerAngle = 0;
		}

		private float _friction;
		/// <summary>
		/// Sets the friction of the wheels
		/// </summary>
		public float WheelFriction {
			get {
				return _friction;
			}
			set {
				this._friction = value;

				for (int a = 0; a < 4; a++) {
					Vehicle.GetWheelInfo(a).FrictionSlip = value;
				}
			}
		}

		/// <summary>
		/// shortcut
		/// </summary>
		public float WheelSpeed {
			get {
				return Vehicle.CurrentSpeedKmHour;
			}
		}

		/// <summary>
		/// Gets a wheel
		/// </summary>
		/// <param name="id">Must be between 0 and 3!</param>
		public Wheel GetWheel(int id) {
			if (id < 0 || id > 3)
				throw new ArgumentOutOfRangeException("id", "The ID number must be between 0 and 3 inclusive!");
			return GetWheel((WheelID) id);
		}

		/// <summary>
		/// Gets a wheel
		/// </summary>
		public Wheel GetWheel(WheelID wid) {
			switch (wid) {
				case WheelID.FrontLeft:
					return WheelFL;
				case WheelID.FrontRight:
					return WheelFR;
				case WheelID.BackLeft:
					return WheelBL;
				case WheelID.BackRight:
					return WheelBR;
				default:
					return null;
			}
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				// then we have to dispose of all of the wheels
				WheelFL.Dispose();
				WheelFR.Dispose();
				WheelBL.Dispose();
				WheelBR.Dispose();
			}

			Vehicle.Dispose();

			base.Dispose(disposing);
		}
	}
}
