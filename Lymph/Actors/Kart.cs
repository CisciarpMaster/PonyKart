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
			get { return 20f; }
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

		

		// our wheelshapes
		// TODO: maybe stick all of this wheel stuff in a separate class? it's making this kart class pretty big
		public Wheel WheelFR { get; protected set; }
		public Wheel WheelFL { get; protected set; }
		public Wheel WheelBR { get; protected set; }
		public Wheel WheelBL { get; protected set; }


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
			if (!LKernel.Get<LevelManager>().IsValidLevel || WheelFR == null || Actor.IsDisposed || Actor.IsSleeping || Pauser.IsPaused)
				return true;

			currentRoll += WheelFR.Shape.AxleSpeed / 3f;
			currentRoll %= 360f;

			Quaternion frontOrient = new Quaternion().FromGlobalEuler(new Degree(currentRoll), WheelFR.Shape.SteerAngle, 0);
			Quaternion backOrient = new Quaternion().FromGlobalEuler(new Degree(currentRoll), 0, 0);
			WheelFR.UpdateAngle(frontOrient);
			WheelFL.UpdateAngle(frontOrient);
			WheelBR.UpdateAngle(backOrient);
			WheelBL.UpdateAngle(backOrient);

			return true;
		}

		/// <summary>
		/// Adds a ribbon and creates the wheel nodes and entities
		/// </summary>
		protected override void CreateMoreMogreStuff() {
			// add a ribbon
			Node.SetScale(1, 1, 1);
			CreateRibbon(15, 30, ColourValue.Blue, 2f);
		}

		/// <summary>
		/// Same as base class + that funny angled bit and the wheels
		/// </summary>
		protected override void CreateActor() {
			base.CreateActor(); // the main box
			CreateFunnyAngledBitInTheFront();

			WheelFR = new Wheel(this, new Vector3(-1.7f, 0f, 0.75f));
			WheelFL = new Wheel(this, new Vector3(1.7f, 0f, 0.75f));
			WheelBR = new Wheel(this, new Vector3(-1.7f, 0f, -1.33f));
			WheelBL = new Wheel(this, new Vector3(1.7f, 0f, -1.33f));
		}

		protected void CreateFunnyAngledBitInTheFront() {
			var frontAngledShape = Actor.CreateShape(new BoxShapeDesc(new Vector3(1, 0.2f, 1), new Vector3(0, 0.3f, 1.33f)));
			frontAngledShape.GlobalOrientation = new Quaternion().FromLocalEuler(new Vector3(0, 45, 0).DegreeVectorToRadianVector()).ToRotationMatrix();
		}

		

		protected override void SetDefaultActorProperties() {
			// lower the center of mass to make it not flip over very easily
			Actor.CMassOffsetLocalPosition = new Vector3(0, -1f, 0);
			Actor.AngularDamping = 0.5f;
			Actor.LinearDamping = 0.3f;
		}

		/// <summary>
		/// Sets the torque of both rear wheels to <paramref name="speed"/> and sets their brake torque to 0.
		/// TODO: Limit the maximum speed by not applying torque when we're going faster than the maximum speed
		/// </summary>
		public void Accelerate(float speed) {
			if (Actor.IsSleeping)
				Actor.WakeUp();
			WheelBR.Accelerate(speed);
			WheelBL.Accelerate(speed);
			WheelFR.Accelerate(speed);
			WheelFL.Accelerate(speed);
		}

		/// <summary>
		/// Sets the motor torque of both rear wheels to 0 and applies a brake torque.
		/// </summary>
		public void Brake() {
			WheelBR.Brake(BrakeForce);
			WheelBL.Brake(BrakeForce);
			WheelFR.Brake(BrakeForce);
			WheelFL.Brake(BrakeForce);
		}

		/// <summary>
		/// Turns the front wheels to <paramref name="angle"/>
		/// </summary>
		public void Turn(Radian angle) {
			if (Actor.IsSleeping)
				Actor.WakeUp();
			WheelFR.Turn(angle);
			WheelFL.Turn(angle);
		}

		#region IDisposable stuff
		public override void Dispose() {
			// unhook from the event
			LKernel.Get<Root>().FrameStarted -= FrameStarted;

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
