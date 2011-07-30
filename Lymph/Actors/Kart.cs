using Mogre;
using Mogre.PhysX;
using Ponykart.Phys;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for karts. Eventually this'll be abstract.
	/// Z is forwards!
	/// </summary>
	public class Kart : DynamicThing {
		
		protected override ShapeDesc ShapeDesc {
			get { return new BoxShapeDesc(new Vector3(1.5f, 0.5f, 1.4f)); }
		}
		protected override sealed uint DefaultCollisionGroupID {
			get { return Groups.CollidablePushableID; }
		}
		protected override string DefaultModel {
			get { return "kart/KartChassis.mesh"; }
		}
		protected override sealed string DefaultMaterial {
			get { return "redbrick"; }
		}
		protected override float Density {
			get { return 40f; }
		}

		// our wheelshapes
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelBR { get; protected set; }
		public Wheel WheelBL { get; protected set; }


		public Kart(ThingTemplate tt) : base(tt) {
			Launch.Log("Creating Kart #" + ID + " with name \"" + tt.StringTokens["Name"] + "\"");
		}

		/// <summary>
		/// Adds a ribbon and creates the wheel nodes and entities
		/// </summary>
		protected override void CreateMoreMogreStuff() {
			// add a ribbon
			CreateRibbon(15, 30, ColourValue.Blue, 2f);
		}

		/// <summary>
		/// Same as base class + that funny angled bit and the wheels
		/// </summary>
		protected override void CreateActor() {
			base.CreateActor(); // the main box
			CreateFunnyAngledBitInTheFront();

			WheelFR = WheelFactory.CreateAltFrontWheel(this, new Vector3(-1.7f, 0f, 0.75f));
			WheelFL = WheelFactory.CreateAltFrontWheel(this, new Vector3(1.7f, 0f, 0.75f));
			WheelBR = WheelFactory.CreateAltBackWheel(this, new Vector3(-1.7f, 0f, -1.33f));
			WheelBL = WheelFactory.CreateAltBackWheel(this, new Vector3(1.7f, 0f, -1.33f));
		}

		protected void CreateFunnyAngledBitInTheFront() {
			var frontAngledShape = Actor.CreateShape(new BoxShapeDesc(new Vector3(1, 0.2f, 1), new Vector3(0, 0.3f, 1.33f)));
			frontAngledShape.MaterialIndex = LKernel.Get<PhysXMaterials>().NoFrictionMaterial.Index;
			frontAngledShape.GlobalOrientation = new Vector3(0, 45, 0).DegreeVectorToLocalQuaternion().ToRotationMatrix();
		}

		

		protected override void SetDefaultActorProperties() {
			// lower the center of mass to make it not flip over very easily
			Actor.CMassOffsetLocalPosition = new Vector3(0, -1f, 0);
			Actor.AngularDamping = 0.5f;
			Actor.LinearDamping = 0.3f;
		}

		/// <summary>
		/// Sets the motor torque of all wheels and sets their brake torque to 0.
		/// TODO: Limit the maximum speed by not applying torque when we're going faster than the maximum speed
		/// </summary>
		public void Accelerate(float multiplier) {
			if (Actor.IsSleeping)
				Actor.WakeUp();
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
			if (Actor.IsSleeping)
				Actor.WakeUp();
			WheelFR.TurnMultiplier = WheelFL.TurnMultiplier = WheelBR.TurnMultiplier = WheelBL.TurnMultiplier = multiplier;
		}

		#region IDisposable stuff
		public override void Dispose() {

			// then we have to dispose of all of the wheels
			WheelFR.Dispose();
			WheelFL.Dispose();
			WheelBR.Dispose();
			WheelBL.Dispose();

			base.Dispose();
		}
		#endregion
	}
}
