using BulletSharp;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for karts. Eventually this'll be abstract.
	/// Z is forwards!
	/// </summary>
	public class Kart : DynamicThing {
		
		/// <summary>
		/// Cast this to a CompoundShape when you use it here
		/// </summary>
		protected override CollisionShape CollisionShape {
			get { return CreateCompoundShape(); }
		}
		protected override PonykartCollisionGroups CollisionGroup {
			get { return PonykartCollisionGroups.Karts; }
		}
		protected override PonykartCollidesWithGroups CollidesWith {
			get { return PonykartCollidesWithGroups.Karts; }
		}
		protected override PhysicsMaterial PhysicsMaterial {
			get { return LKernel.Get<PhysicsMaterialManager>().KartMaterial; }
		}
		protected override string DefaultModel {
			get { return "kart/KartChassis.mesh"; }
		}
		protected override sealed string DefaultMaterial {
			get { return "redbrick"; }
		}
		protected override float Mass {
			get { return 500f; }
		}
		protected override MotionState DefaultMotionState {
			get { return new KartMotionState(SpawnPosition, SpawnRotation, RootNode, this); }
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


		public Kart(ThingTemplate tt) : base(tt) {
			Launch.Log("Creating Kart #" + ID + " with name \"" + tt.StringTokens["Name"] + "\"");

			MaxSpeed = 40f;
			MaxSpeedSquared = MaxSpeed * MaxSpeed;
		}

		/// <summary>
		/// Adds a ribbon and creates the wheel nodes and entities
		/// </summary>
		protected override void CreateMoreMogreStuff() {
			// add a ribbon
			CreateRibbon(15, 30, ColourValue.Blue, 2f);
			Node.Position += new Vector3(0, 0.5f, 0);
		}

		/// <summary>
		/// Creates our compound shape. This only runs once since we can re-use shapes as much as we want.
		/// </summary>
		/// <returns></returns>
		protected static CompoundShape CreateCompoundShape() {
			// only want to run this once
			if (Compound != null)
				return Compound;

			Compound = new CompoundShape();

			Matrix4 trans = new Matrix4();
			trans.MakeTrans(0, 0.7f, 0);

			BoxShape chassisShape = new BoxShape(new Vector3(1.5f, 0.5f, 1.4f));
			Compound.AddChildShape(trans, chassisShape);

			/*BoxShape frontAngledShape = new BoxShape(new Vector3(1, 0.2f, 1));
			trans = new Matrix4(new Vector3(0, 45, 0).DegreeVectorToLocalQuaternion());
			trans.SetTrans(new Vector3(0, 0.15f, 1.33f));
			Compound.AddChildShape(trans, frontAngledShape);*/

			return Compound;
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
			WheelFL = wheelFac.CreateWheel("FrontWheel", WheelID.FrontLeft, this, new Vector3(1.7f, 0.4f, 1.33f/*0.75f*/), true);
			WheelFR = wheelFac.CreateWheel("FrontWheel", WheelID.FrontRight, this, new Vector3(-1.7f, 0.4f, 1.33f), true);
			WheelBL = wheelFac.CreateWheel("BackWheel", WheelID.BackLeft, this, new Vector3(1.7f, 0.4f, -1.33f), false);
			WheelBR = wheelFac.CreateWheel("BackWheel", WheelID.BackRight, this, new Vector3(-1.7f, 0.4f, -1.33f), false);
		}

		/// <summary>
		/// Sets the motor torque of all wheels and sets their brake torque to 0.
		/// TODO: Limit the maximum speed by not applying torque when we're going faster than the maximum speed
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
