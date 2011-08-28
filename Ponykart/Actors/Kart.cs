using BulletSharp;
using PonykartParsers;
using Ponykart.Physics;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for karts. Eventually this'll be abstract.
	/// Z is forwards!
	/// </summary>
	public class Kart : LThing {
		protected override MotionState DefaultMotionState {
			get { return new KartMotionState(SpawnPosition, SpawnOrientation, RootNode, this); }
		}
		public float MaxSpeed { get; set; }
		public float MaxReverseSpeed { get; set; }
		public float MaxSpeedSquared { get; private set; }
		public float MaxReverseSpeedSquared { get; private set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		public RaycastVehicle Vehicle { get; protected set; }
		public RaycastVehicle.VehicleTuning Tuning { get; protected set; }
		protected VehicleRaycaster Raycaster;
		// do not dispose of the damn shapes
		protected static CompoundShape Compound;


		public Kart(ThingBlock block, ThingDefinition def) : base(block, def) {
			MaxSpeed = def.GetFloatProperty("maxspeed", 40f);
			MaxReverseSpeed = def.GetFloatProperty("maxreversespeed", 20f);
			MaxSpeedSquared = MaxSpeed * MaxSpeed;
			MaxReverseSpeedSquared = MaxReverseSpeed * MaxReverseSpeed;
		}

		protected override void PostCreateBody(ThingDefinition def) {
			Body.ActivationState = ActivationState.DisableDeactivation;

			Raycaster = new DefaultVehicleRaycaster(LKernel.Get<PhysicsMain>().World);
			Tuning = new RaycastVehicle.VehicleTuning();
			//Tuning.MaxSuspensionTravelCm = 40f;
			Vehicle = new RaycastVehicle(Tuning, Body, Raycaster);
			Vehicle.SetCoordinateSystem(0, 1, 2); // I have no idea what this does... I'm assuming something to do with a matrix?

			LKernel.Get<PhysicsMain>().World.AddAction(Vehicle);

			var wheelFac = LKernel.Get<WheelFactory>();
			string frontWheelName = def.GetStringProperty("frontwheel", null);
			string backWheelName = def.GetStringProperty("backwheel", null);
			WheelFL = wheelFac.CreateWheel(frontWheelName, WheelID.FrontLeft, this, def.GetVectorProperty("frontleftwheelposition", null), true);
			WheelFR = wheelFac.CreateWheel(frontWheelName, WheelID.FrontRight, this, def.GetVectorProperty("frontrightwheelposition", null), true);
			WheelBL = wheelFac.CreateWheel(backWheelName, WheelID.BackLeft, this, def.GetVectorProperty("backleftwheelposition", null), false);
			WheelBR = wheelFac.CreateWheel(backWheelName, WheelID.BackRight, this, def.GetVectorProperty("backrightwheelposition", null), false);
		}

		/// <summary>
		/// Sets the motor torque of all wheels and sets their brake torque to 0.
		/// </summary>
		public void Accelerate(float multiplier) {
			WheelBR.AccelerateMultiplier = WheelBL.AccelerateMultiplier = WheelFR.AccelerateMultiplier = WheelFL.AccelerateMultiplier = multiplier;
			WheelBR.IsBrakeOn = WheelBL.IsBrakeOn = WheelFR.IsBrakeOn = WheelFL.IsBrakeOn = false;
		}

		/// <summary>
		/// Sets the motor torque of all wheels to 0 and applies a brake torque.
		/// </summary>
		public void Brake() {
			WheelBR.IsBrakeOn = WheelBL.IsBrakeOn = WheelFR.IsBrakeOn = WheelFL.IsBrakeOn = true;
			WheelBR.AccelerateMultiplier = WheelBL.AccelerateMultiplier = WheelFR.AccelerateMultiplier = WheelFL.AccelerateMultiplier = 0;
		}

		/// <summary>
		/// Turns the wheels
		/// </summary>
		public void Turn(float multiplier) {
			WheelFR.TurnMultiplier = WheelFL.TurnMultiplier = WheelBR.TurnMultiplier = WheelBL.TurnMultiplier = multiplier;
		}

		public void SetWheelFriction(float friction) {
			for (int a = 0; a < 4; a++) {
				Vehicle.GetWheelInfo(a).FrictionSlip = friction;
			}
		}

		public override void Dispose() {

			// then we have to dispose of all of the wheels
			WheelFL.Dispose();
			WheelFR.Dispose();
			WheelBL.Dispose();
			WheelBR.Dispose();

			base.Dispose();
		}
	}
}
