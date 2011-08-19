using BulletSharp;
using Mogre;
using Ponykart.IO;
using Ponykart.Physics;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for karts. Eventually this'll be abstract.
	/// Z is forwards!
	/// </summary>
	public class Kart : LThing {
		protected PhysicsMaterial PhysicsMaterial {
			get { return LKernel.Get<PhysicsMaterialManager>().KartMaterial; }
		}
		protected override MotionState DefaultMotionState {
			get { return new KartMotionState(SpawnPosition, SpawnOrientation, RootNode, this); }
		}
		public float MaxSpeed { get; set; }
		public float MaxSpeedSquared { get; private set; }

		// our wheelshapes
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelBL { get; protected set; }
		public Wheel WheelBR { get; protected set; }

		public RaycastVehicle Vehicle;
		public RaycastVehicle.VehicleTuning Tuning;
		protected VehicleRaycaster Raycaster;
		// do not dispose of the damn shapes
		protected static CompoundShape Compound;


		public Kart(ThingInstanceTemplate tt, ThingDefinition td) : base(tt, td) {
			Launch.Log("Creating Kart #" + ID + " with name \"" + Name + "\"");

			MaxSpeed = 40f;
			MaxSpeedSquared = MaxSpeed * MaxSpeed;
		}

		protected override void PostCreateBody() {
			Body.ActivationState = ActivationState.DisableDeactivation;

			Raycaster = new DefaultVehicleRaycaster(LKernel.Get<PhysicsMain>().World);
			Tuning = new RaycastVehicle.VehicleTuning();
			Tuning.MaxSuspensionTravelCm = 40f;
			Vehicle = new RaycastVehicle(Tuning, Body, Raycaster);
			Vehicle.SetCoordinateSystem(0, 1, 2); // I have no idea what this does... I'm assuming something to do with a matrix?

			LKernel.Get<PhysicsMain>().World.AddAction(Vehicle);

			var wheelFac = LKernel.Get<WheelFactory>();
			WheelFL = wheelFac.CreateWheel("FrontWheel", WheelID.FrontLeft, this, new Vector3(1.7f, 0.4f, 1.33f), true);
			WheelFR = wheelFac.CreateWheel("FrontWheel", WheelID.FrontRight, this, new Vector3(-1.7f, 0.4f, 1.33f), true);
			WheelBL = wheelFac.CreateWheel("BackWheel", WheelID.BackLeft, this, new Vector3(1.7f, 0.4f, -1.33f), false);
			WheelBR = wheelFac.CreateWheel("BackWheel", WheelID.BackRight, this, new Vector3(-1.7f, 0.4f, -1.33f), false);
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

		#region IDisposable stuff
		public override void Dispose() {

			// then we have to dispose of all of the wheels
			WheelFL.Dispose();
			WheelFR.Dispose();
			WheelBL.Dispose();
			WheelBR.Dispose();

			base.Dispose();
		}
		#endregion
	}
}
