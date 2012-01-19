using System;
using BulletSharp;
using Mogre;
using Ponykart.Handlers;
using Ponykart.Physics;
using PonykartParsers;

namespace Ponykart.Actors {

	public delegate void KartEvent(Kart kart);

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
		
		public KartDriftState DriftState { get; set; }

		public Driver Driver { get; set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		public SceneNode LeftParticleNode { get; private set; }
		public SceneNode RightParticleNode { get; private set; }

		public readonly Degree FrontDriftAngle;
		public readonly Degree BackDriftAngle;

		protected RaycastVehicle _vehicle;
		public RaycastVehicle Vehicle { get { return _vehicle; } }
		public RaycastVehicle.VehicleTuning Tuning { get; protected set; }
		protected VehicleRaycaster Raycaster;


		public static event KartEvent OnStartDrifting, OnDrifting, OnStopDrifting, OnFinishDrifting;


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
		/// Make some nodes for us to attach wheel particles to
		/// </summary>
		protected override void PostInitialiseComponents(ThingBlock template, ThingDefinition def) {
			Vector3 frontleft = def.GetVectorProperty("frontleftwheelposition", null);
			Vector3 frontright = def.GetVectorProperty("frontrightwheelposition", null);
			Vector3 backleft = def.GetVectorProperty("backleftwheelposition", null);
			Vector3 backright = def.GetVectorProperty("backrightwheelposition", null);
			LeftParticleNode = RootNode.CreateChildSceneNode(frontleft.MidPoint(backleft));
			RightParticleNode = RootNode.CreateChildSceneNode(frontright.MidPoint(backright));
		}

		/// <summary>
		/// After we create our RigidBody, we turn it into a vehicle
		/// </summary>
		protected override void PostCreateBody(ThingDefinition def) {
			Body.CcdMotionThreshold = 0.001f;
			Body.CcdSweptSphereRadius = 0.2f;

			Raycaster = new DefaultVehicleRaycaster(LKernel.GetG<PhysicsMain>().World);
			Tuning = new RaycastVehicle.VehicleTuning();
			_vehicle = new RaycastVehicle(Tuning, Body, Raycaster);
			_vehicle.SetCoordinateSystem(0, 1, 2); // I have no idea what this does... I'm assuming something to do with a rotation matrix?

			LKernel.GetG<PhysicsMain>().World.AddAction(_vehicle);

			var wheelFac = LKernel.GetG<WheelFactory>();
			string frontWheelName = def.GetStringProperty("frontwheel", null);
			string backWheelName = def.GetStringProperty("backwheel", null);
			WheelFL = wheelFac.CreateWheel(frontWheelName, WheelID.FrontLeft, this, def.GetVectorProperty("frontleftwheelposition", null), def.GetStringProperty("FrontWheelMesh", null));
			WheelFR = wheelFac.CreateWheel(frontWheelName, WheelID.FrontRight, this, def.GetVectorProperty("frontrightwheelposition", null), def.GetStringProperty("FrontWheelMesh", null));
			WheelBL = wheelFac.CreateWheel(backWheelName, WheelID.BackLeft, this, def.GetVectorProperty("backleftwheelposition", null), def.GetStringProperty("BackWheelMesh", null));
			WheelBR = wheelFac.CreateWheel(backWheelName, WheelID.BackRight, this, def.GetVectorProperty("backrightwheelposition", null), def.GetStringProperty("BackWheelMesh", null));

			LeftParticleNode.Position -= new Vector3(0, WheelBL.Radius * 0.7f, 0);
			RightParticleNode.Position -= new Vector3(0, WheelBR.Radius * 0.7f, 0);
			
			Body.LinearVelocity = new Vector3(0, 1, 0);
		}

		/// <summary>
		/// Start drifting in a certain direction
		/// </summary>
		/// <param name="state">This must be either StartDriftLeft or StartDriftRight</param>
		public void StartDrifting(KartDriftState state) {
			// first check to make sure we weren't passed an incorrect argument
			if (!(KartDriftState.StartLeft | KartDriftState.StartRight).HasFlag(state))
				throw new ArgumentException("You must pass either StartDriftLeft or StartDriftRight!", "state");

			if (_vehicle.CurrentSpeedKmHour < 100 || IsDriftingAtAll)
				return;

			// update our state
			DriftState = state;

			if (OnStartDrifting != null)
				OnStartDrifting(this);
		}

		public void StartActuallyDrifting() {
			// "upgrade" our drift states
			if (DriftState == KartDriftState.StartLeft)
				DriftState = KartDriftState.FullLeft;
			else if (DriftState == KartDriftState.StartRight)
				DriftState = KartDriftState.FullRight;

			if (OnDrifting != null)
				OnDrifting(this);

			if (DriftState == KartDriftState.FullLeft) {
				new Rotater<Kart>(this, 0.3f, new Degree(-FrontDriftAngle / 2f), RotaterAxisMode.RelativeY);
			}
			else if (DriftState == KartDriftState.FullRight) {
				new Rotater<Kart>(this, 0.3f, new Degree(FrontDriftAngle / 2f), RotaterAxisMode.RelativeY);
			}

			ForEachWheel(w => {
				// left
				if (this.DriftState == KartDriftState.FullLeft) {
					w.DriftState = WheelDriftState.Left;

					// change the back wheels' angles
					if (w.ID == WheelID.FrontRight || w.ID == WheelID.BackRight) {
						w.IdealSteerAngle = BackDriftAngle;
						_vehicle.SetSteeringValue(BackDriftAngle.ValueRadians, w.IntWheelID);
						_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
					}
					// change the front wheels' angles
					else {
						w.IdealSteerAngle = FrontDriftAngle;
						_vehicle.SetSteeringValue(FrontDriftAngle.ValueRadians, w.IntWheelID);
						_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
					}
				}
				// right
				else if (this.DriftState == KartDriftState.FullRight) {
					w.DriftState = WheelDriftState.Right;

					// change the back wheels' angles
					if (w.ID == WheelID.FrontLeft || w.ID == WheelID.BackLeft) {
						w.IdealSteerAngle = -BackDriftAngle;
						_vehicle.SetSteeringValue(-BackDriftAngle.ValueRadians, w.IntWheelID);
						_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
					}
					// change the front wheels' angles
					else {
						w.IdealSteerAngle = -FrontDriftAngle;
						_vehicle.SetSteeringValue(-FrontDriftAngle.ValueRadians, w.IntWheelID);
						_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
					}
				}
			});
		}

		/// <summary>
		/// Stop drifting. This is run when we let go of the drift button.
		/// </summary>
		public void StopDrifting() {
			// "upgrade" our events
			if (DriftState == KartDriftState.FullLeft || DriftState == KartDriftState.StartLeft)
				DriftState = KartDriftState.StopLeft;
			else if (DriftState == KartDriftState.FullRight || DriftState == KartDriftState.StartRight)
				DriftState = KartDriftState.StopRight;

			// eeeeeveeeeeent
			if (OnStopDrifting != null)
				OnStopDrifting(this);

			// make the wheels back to normal
			ForEachWheel(w => {
				w.DriftState = WheelDriftState.None;
				w.IdealSteerAngle = 0;

				if (w.ID == WheelID.FrontRight || w.ID == WheelID.FrontLeft) {
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = true;
				}
				else {
					_vehicle.GetWheelInfo(w.IntWheelID).IsFrontWheel = false;
					_vehicle.ApplyEngineForce(0, w.IntWheelID);
				}
			});

			_vehicle.GetWheelInfo((int) WheelID.FrontLeft).IsFrontWheel = true;
			_vehicle.GetWheelInfo((int) WheelID.FrontRight).IsFrontWheel = true;
			_vehicle.GetWheelInfo((int) WheelID.BackLeft).IsFrontWheel = false;
			_vehicle.GetWheelInfo((int) WheelID.BackRight).IsFrontWheel = false;
		}

		public void FinishDrifting() {
			DriftState = KartDriftState.None;

			if (OnFinishDrifting != null)
				OnFinishDrifting(this);
		}



		#region Properties
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
					w.Friction = value;
				});
			}
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
				if (value != 0)
					Body.Activate();
				this._accelerate = value;

				ForEachWheel(w => {
					w.AccelerateMultiplier = value;
					w.IsBrakeOn = false;
				});
			}
		}

		private float _turnMultiplier;
		/// <summary>
		/// Turns the wheels
		/// 
		/// Turn left is positive, turn right is negative
		/// </summary>
		public float TurnMultiplier {
			get {
				return _turnMultiplier;
			}
			set {
				if (IsCompletelyDrifting) {
					if (this._turnMultiplier - value < 0) {
						new Rotater<Kart>(this, 0.3f, new Degree(10), RotaterAxisMode.RelativeY);
					}
					else if (this._turnMultiplier - value > 0) {
						new Rotater<Kart>(this, 0.3f, new Degree(-10), RotaterAxisMode.RelativeY);
					}
				}

				this._turnMultiplier = value;

				ForEachWheel(w => {
					w.TurnMultiplier = value;
				});
			}
		}

		/// <summary>
		/// shortcut
		/// </summary>
		public float VehicleSpeed {
			get {
				return _vehicle.CurrentSpeedKmHour;
			}
		}

		/// <summary>
		/// Returns true if we're completely drifting - not starting, not stopping, but in between.
		/// </summary>
		public bool IsCompletelyDrifting {
			get {
				return DriftState == KartDriftState.FullLeft || DriftState == KartDriftState.FullRight;
			}
		}

		/// <summary>
		/// Returns true if we're starting to drift
		/// </summary>
		public bool IsStartingDrifting {
			get {
				return DriftState == KartDriftState.StartLeft || DriftState == KartDriftState.StartRight;
			}
		}

		/// <summary>
		/// Returns true if we're stopping drifting
		/// </summary>
		public bool IsStoppingDrifting {
			get {
				return DriftState == KartDriftState.StopLeft || DriftState == KartDriftState.StopRight;
			}
		}

		/// <summary>
		/// Returns true if we're drifting at all - starting, stopping, or in between.
		/// </summary>
		public bool IsDriftingAtAll {
			get {
				return DriftState == KartDriftState.FullLeft || DriftState == KartDriftState.FullRight
					|| DriftState == KartDriftState.StartLeft || DriftState == KartDriftState.StartRight
					|| DriftState == KartDriftState.StopLeft || DriftState == KartDriftState.StopRight;
			}
		}
		#endregion

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

			_vehicle.Dispose();

			base.Dispose(disposing);
		}
	}
}
