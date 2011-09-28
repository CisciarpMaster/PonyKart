using System;
using BulletSharp;
using Mogre;
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
		/// <summary>
		/// Whether the player has indicated that they want to drift or not. Used to cancel drifting if you've already started bouncing.
		/// </summary>
		public bool WantsDrifting { get; set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		protected readonly Degree FrontDriftAngle;
		protected readonly Degree BackDriftAngle;

		public RaycastVehicle Vehicle { get; protected set; }
		public RaycastVehicle.VehicleTuning Tuning { get; protected set; }
		protected VehicleRaycaster Raycaster;


		public Kart(ThingBlock block, ThingDefinition def) : base(block, def) {
			MaxSpeed = def.GetFloatProperty("maxspeed", 60f);
			MaxReverseSpeed = def.GetFloatProperty("maxreversespeed", 20f);
			MaxSpeedSquared = MaxSpeed * MaxSpeed;
			MaxReverseSpeedSquared = MaxReverseSpeed * MaxReverseSpeed;
			IsInAir = false;

			FrontDriftAngle = new Degree(def.GetFloatProperty("FrontDriftAngle", 86));
			BackDriftAngle = new Degree(def.GetFloatProperty("BackDriftAngle", 89));
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
			WheelFL = wheelFac.CreateWheel(frontWheelName, WheelID.FrontLeft, this, def.GetVectorProperty("frontleftwheelposition", null));
			WheelFR = wheelFac.CreateWheel(frontWheelName, WheelID.FrontRight, this, def.GetVectorProperty("frontrightwheelposition", null));
			WheelBL = wheelFac.CreateWheel(backWheelName, WheelID.BackLeft, this, def.GetVectorProperty("backleftwheelposition", null));
			WheelBR = wheelFac.CreateWheel(backWheelName, WheelID.BackRight, this, def.GetVectorProperty("backrightwheelposition", null));
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

				ForEachWheel(w => {
					w.AccelerateMultiplier = value;
					w.IsBrakeOn = false;
				});
			}
		}

		/// <summary>
		/// Sets the motor torque of all wheels to 0 and applies a brake torque.
		/// </summary>
		[Obsolete]
		public void Brake() {
			ForEachWheel(w => {
				w.IsBrakeOn = true;
				w.AccelerateMultiplier = 0;
			});
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

				ForEachWheel(w => {
					w.TurnMultiplier = value;
				});
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
				if (TurnMultiplier < 0) {
					Body.AngularVelocity += RootNode.GetLocalYAxis() * -1f;
					WantDriftState = DriftState.DriftLeft;
				}
				else if (TurnMultiplier > 0) {
					Body.AngularVelocity += RootNode.GetLocalYAxis() * 1f;
					WantDriftState = DriftState.DriftRight;
				}
				else {
					WantDriftState = DriftState.Normal;
				}

				Body.LinearVelocity += RootNode.GetLocalYAxis() * 10;
			}
		}

		/// <summary>
		/// Start drifting! This is run right after we land from bouncing, if the drift button is still pressed
		/// </summary>
		public void StartDrifting() {
			IsDrifting = true;

			ForEachWheel(w => {
				// left
				if (WantDriftState == DriftState.DriftLeft) {
					w.DriftState = DriftState.DriftLeft;

					// change the back wheels' angles
					if (w.ID == WheelID.FrontRight || w.ID == WheelID.BackRight) {
						w.IdealSteerAngle = BackDriftAngle;
						Vehicle.SetSteeringValue(BackDriftAngle.ValueRadians, w.IntWheelID);
					}
					// change the front wheels' angles
					else {
						w.IdealSteerAngle = FrontDriftAngle;
						Vehicle.SetSteeringValue(FrontDriftAngle.ValueRadians, w.IntWheelID);
					}
				}
				// right
				else if (WantDriftState == DriftState.DriftRight) {
					w.DriftState = DriftState.DriftRight;

					// change the back wheels' angles
					if (w.ID == WheelID.FrontLeft || w.ID == WheelID.BackLeft) {
						w.IdealSteerAngle = -BackDriftAngle;
						Vehicle.SetSteeringValue(-BackDriftAngle.ValueRadians, w.IntWheelID);
					}
					// change the front wheels' angles
					else {
						w.IdealSteerAngle = -FrontDriftAngle;
						Vehicle.SetSteeringValue(-FrontDriftAngle.ValueRadians, w.IntWheelID);
					}
				}
			});
		}

		/// <summary>
		/// Stop drifting. This is run when we let go of the drift button.
		/// </summary>
		public void StopDrifting() {
			IsDrifting = false;
			WantsDrifting = false;
			WantDriftState = DriftState.Normal;

			ForEachWheel(w => {
				w.DriftState = DriftState.Normal;
				w.IdealSteerAngle = 0;
			});
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

				ForEachWheel(w => {
					w.FrictionSlip = value;
				});
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

		public DriftState WantDriftState { get; set; }

		/// <summary>
		/// A little helper method since we do stuff to all four wheels so often
		/// </summary>
		/// <param name="action"></param>
		public void ForEachWheel(Action<Wheel> action) {
			action(WheelFL);
			action(WheelFR);
			action(WheelBL);
			action(WheelBR);
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
