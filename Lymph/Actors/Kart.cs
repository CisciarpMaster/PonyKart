using Mogre;
using Mogre.PhysX;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Phys;
using Math = Mogre.Math;

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
			get { return 10f; }
		}

		public virtual float ForwardSpeed {
			get { return 3000f; }
		}
		public virtual float ReverseSpeed {
			get { return -3000f; }
		}
		public virtual Radian TurnAngle {
			get { return Math.PI / 12f; }
		}
		public float BrakeForce = 10000;

		// lat = sideways grip, long = forwards grip
		//										1.0f				 0.02f						2.0f					   0.01f						1000000f
		public static float LatExtremumSlip = 1.0f, LatExtremumValue = 0.05f, LatAsymptoteSlip = 5.0f, LatAsymptoteValue = 0.005f, LatStiffnessFactor = 1000000f,
							LongExtremumSlip = 1.0f, LongExtremumValue = 0.05f, LongAsymptoteSlip = 2.0f, LongAsymptoteValue = 0.01f, LongStiffnessFactor = 1000000f;

		// our wheelshapes
		// TODO: maybe stick all of this wheel stuff in a separate class? it's making this kart class pretty big
		public WheelShape WheelFR { get; protected set; }
		public WheelShape WheelFL { get; protected set; }
		public WheelShape WheelBR { get; protected set; }
		public WheelShape WheelBL { get; protected set; }
		private SceneNode WheelNodeFR, WheelNodeFL, WheelNodeBR, WheelNodeBL;
		private Entity WheelEntFR, WheelEntFL, WheelEntBR, WheelEntBL;


		public Kart(ThingTemplate tt) : base(tt) {
			Launch.Log("Creating Kart #" + ID + " with name \"" + tt.StringTokens["Name"] + "\"");

			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		// degrees
		float currentRoll;
		/// <summary>
		/// Update the wheel's orientation to match its steering angle and the kart's speed
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!LKernel.Get<LevelManager>().IsValidLevel || WheelFR == null || WheelFR.IsDisposed || Actor.IsSleeping || Pauser.IsPaused)
				return true;

			currentRoll += WheelFR.AxleSpeed / 3f;
			currentRoll %= 360f;

			Quaternion frontOrient = new Quaternion().FromGlobalEuler(new Degree(currentRoll), WheelFR.SteerAngle, 0);
			Quaternion backOrient = new Quaternion().FromGlobalEuler(new Degree(currentRoll), 0, 0);
			WheelNodeFR.Orientation = frontOrient;
			WheelNodeFL.Orientation = frontOrient;
			WheelNodeBR.Orientation = backOrient;
			WheelNodeBL.Orientation = backOrient;

			return true;
		}

		/// <summary>
		/// Adds a ribbon and creates the wheel nodes and entities
		/// </summary>
		protected override void CreateMoreMogreStuff() {
			// add a ribbon
			Node.SetScale(new Vector3(1, 1, 1));
			CreateRibbon(10, 20, ColourValue.Blue, 2f);

			// wheels
			var sceneMgr = LKernel.Get<SceneManager>();

			WheelNodeFR = Node.CreateChildSceneNode("wheelNodeFR" + ID, new Vector3(-1.7f, 0f, 0.75f));
			WheelEntFR = sceneMgr.CreateEntity("wheelNodeFR" + ID, "kart/KartWheel.mesh");
			WheelNodeFR.AttachObject(WheelEntFR);

			WheelNodeFL = Node.CreateChildSceneNode("wheelNodeFL" + ID, new Vector3(1.7f, 0f, 0.75f));
			WheelEntFL = sceneMgr.CreateEntity("wheelNodeFL" + ID, "kart/KartWheel.mesh");
			WheelNodeFL.AttachObject(WheelEntFL);

			WheelNodeBR = Node.CreateChildSceneNode("wheelNodeBR" + ID, new Vector3(-1.7f, 0f, -1.33f));
			WheelEntBR = sceneMgr.CreateEntity("wheelNodeBR" + ID, "kart/KartWheel.mesh");
			WheelNodeBR.AttachObject(WheelEntBR);

			WheelNodeBL = Node.CreateChildSceneNode("wheelNodeBL" + ID, new Vector3(1.7f, 0f, -1.33f));
			WheelEntBL = sceneMgr.CreateEntity("wheelNodeBL" + ID, "kart/KartWheel.mesh");
			WheelNodeBL.AttachObject(WheelEntBL);
		}

		/// <summary>
		/// Same as base class + that funny angled bit and the wheels
		/// </summary>
		protected override void CreateActor() {
			base.CreateActor(); // the main box
			CreateFunnyAngledBitInTheFront();
			// TODO: make these properties or something
			WheelFR = CreateWheel(new Vector3(-1.7f, 0f, 0.75f));
			WheelFL = CreateWheel(new Vector3(1.7f, 0f, 0.75f));
			WheelBR = CreateWheel(new Vector3(-1.7f, 0f, -1.33f));
			WheelBL = CreateWheel(new Vector3(1.7f, 0f, -1.33f));
		}

		protected void CreateFunnyAngledBitInTheFront() {
			var frontAngledShape = Actor.CreateShape(new BoxShapeDesc(new Vector3(1, 0.2f, 1), new Vector3(0, 0.3f, 1.33f)));
			frontAngledShape.GlobalOrientation = new Quaternion().FromLocalEuler(new Vector3(0, 45, 0).DegreeVectorToRadianVector()).ToRotationMatrix();
		}

		/// <summary>
		/// Makes a wheel at the given position
		/// </summary>
		protected WheelShape CreateWheel(Vector3 position) {
			WheelShapeDesc wsd = new WheelShapeDesc();
			// default properties
			float radius = 0.5f,
				suspension = 0.5f, // how "long" is the suspension spring?
				springRestitution = 7000, // how much force it'll absorb
				springDamping = 800f, // bounciness: bigger = less bouncy
				springBias = 0f;

			// kinda a hack for now
			//wsd.MaterialIndex = PhysXMain.noFrictionMaterial.Index;

			// position and orientation
			wsd.LocalPosition = position;
			/*Quaternion q = new Quaternion();
			q.FromAngleAxis(new Degree(90), Vector3.UNIT_Y);
			wsd.LocalOrientation = q.ToRotationMatrix();*/

			// suspension
			float heightModifier = (suspension + radius) / suspension;
			wsd.Suspension.Spring = springRestitution * heightModifier;
			wsd.Suspension.Damper = springDamping * heightModifier;
			wsd.Suspension.TargetValue = springBias * heightModifier;

			// other stuff
			wsd.Radius = radius;
			wsd.SuspensionTravel = suspension;
			wsd.InverseWheelMass = 0.1f; // physx docs say "not given!? TODO", whatever that means

			// tyre... things. Something to do with gripping surfaces at different velocities
			// this one has to do with sideways grip for things like handling
			wsd.LateralTireForceFunction.ExtremumSlip = LatExtremumSlip;
			wsd.LateralTireForceFunction.ExtremumValue = LatExtremumValue;
			wsd.LateralTireForceFunction.AsymptoteSlip = LatAsymptoteSlip;
			wsd.LateralTireForceFunction.AsymptoteValue = LatAsymptoteValue;
			wsd.LateralTireForceFunction.StiffnessFactor = LatStiffnessFactor;
			// this one has to do with forwards grip for things like acceleration
			wsd.LongitudalTireForceFunction.ExtremumSlip = LongExtremumSlip;
			wsd.LongitudalTireForceFunction.ExtremumValue = LongExtremumValue;
			wsd.LongitudalTireForceFunction.AsymptoteSlip = LongAsymptoteSlip;
			wsd.LongitudalTireForceFunction.AsymptoteValue = LongAsymptoteValue;
			wsd.LongitudalTireForceFunction.StiffnessFactor = LongStiffnessFactor;

			return Actor.CreateShape(wsd) as WheelShape;
		}

		protected override void SetDefaultActorProperties() {
			// lower the center of mass to make it not flip over very easily
			Actor.CMassOffsetLocalPosition = new Vector3(0, -1f, 0);
			Actor.AngularDamping = 0.5f;
			Actor.LinearDamping = 0.1f;
		}

		/// <summary>
		/// Sets the torque of both rear wheels to <paramref name="speed"/> and sets their brake torque to 0.
		/// TODO: Limit the maximum speed by not applying torque when we're going faster than the maximum speed
		/// </summary>
		public void Accelerate(float speed) {
			if (Actor.IsSleeping)
				Actor.WakeUp();
			WheelBR.MotorTorque = speed;
			WheelBR.BrakeTorque = 0;
			WheelBL.MotorTorque = speed;
			WheelBL.BrakeTorque = 0;
			WheelFR.MotorTorque = speed;
			WheelFR.BrakeTorque = 0;
			WheelFL.MotorTorque = speed;
			WheelFL.BrakeTorque = 0;
		}

		/// <summary>
		/// Sets the motor torque of both rear wheels to 0 and applies a brake torque.
		/// </summary>
		public void Brake() {
			WheelBR.MotorTorque = 0;
			WheelBR.BrakeTorque = BrakeForce;
			WheelBL.MotorTorque = 0;
			WheelBL.BrakeTorque = BrakeForce;
			WheelFR.MotorTorque = 0;
			WheelFR.BrakeTorque = BrakeForce;
			WheelFL.MotorTorque = 0;
			WheelFL.BrakeTorque = BrakeForce;
		}

		/// <summary>
		/// Turns the front wheels to <paramref name="angle"/>
		/// </summary>
		public void Turn(Radian angle) {
			if (Actor.IsSleeping)
				Actor.WakeUp();
			WheelFR.SteerAngle = angle.ValueRadians;
			WheelFL.SteerAngle = angle.ValueRadians;
		}

		#region IDisposable stuff
		public override void Dispose() {
			// unhook from the event
			LKernel.Get<Root>().FrameStarted -= FrameStarted;

			var sceneMgr = LKernel.Get<SceneManager>();
			bool valid = LKernel.Get<LevelManager>().IsValidLevel;

			// then we have to dispose of all of the wheels. Maybe stick these wheels in a separate class? who knows
			if (WheelNodeFR != null) {
				if (valid) {
					sceneMgr.DestroyEntity(WheelEntFR);
					sceneMgr.DestroySceneNode(WheelNodeFR);
				}
				WheelEntFR.Dispose();
				WheelNodeFR.Dispose();
				WheelEntFR = null;
				WheelNodeFR = null;
			}
			if (WheelNodeFL != null) {
				if (valid) {
					sceneMgr.DestroyEntity(WheelEntFL);
					sceneMgr.DestroySceneNode(WheelNodeFL);
				}
				WheelEntFL.Dispose();
				WheelNodeFL.Dispose();
				WheelEntFL = null;
				WheelNodeFL = null;
			}
			if (WheelNodeBR != null) {
				if (valid) {
					sceneMgr.DestroyEntity(WheelEntBR);
					sceneMgr.DestroySceneNode(WheelNodeBR);
				}
				WheelEntBR.Dispose();
				WheelNodeBR.Dispose();
				WheelEntBR = null;
				WheelNodeBR = null;
			}
			if (WheelNodeBL != null) {
				if (valid) {
					sceneMgr.DestroyEntity(WheelEntBL);
					sceneMgr.DestroySceneNode(WheelNodeBL);
				}
				WheelEntBL.Dispose();
				WheelNodeBL.Dispose();
				WheelEntBL = null;
				WheelNodeBL = null;
			}

			base.Dispose();
		}
		#endregion
	}
}
