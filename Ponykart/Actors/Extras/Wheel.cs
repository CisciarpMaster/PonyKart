using BulletSharp;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Stuff;
using Math = Mogre.Math;

namespace Ponykart.Actors {
	// might want to make this abstract and make two more classes for front and back wheels
	public class Wheel : System.IDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public int ID { get; protected set; }

		public float Radius { get; set; } // 0.5 (lymph)
		public float Width { get; set; } // 0.4 (demo)
		public float ConnectionHeight { get; set; } // 1.2 (demo)
		public float HalfExtents { get; set; } // 0.7 (lymph)
		public float SuspensionRestLength { get; set; } // 0.6 (demo) // 0.3 (zg)

		public float SpringStiffness { get; set; } // 100 (zg)
		public float SpringCompression { get; set; } // 4.2 (zg)
		public float SpringDamping { get; set; } // 20 (zg)
		public float FrictionSlip { get; set; } // 1000 (demo)
		public float RollInfluence { get; set; } // 1 (zg)

		public float BrakeForce { get; set; }
		public float MotorForce { get; set; }
		public Radian TurnAngle { get; set; } // 0.3 (demo)
		public float MaxSpeed { get; set; }

		protected Vector3 WheelDirection = Vector3.NEGATIVE_UNIT_Y;
		protected Vector3 WheelAxle = Vector3.NEGATIVE_UNIT_X;
		public float IsFrontWheel { get; set; }
		public WheelID WheelID { get; private set; }

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

		Kart Kart;

		public Wheel(Kart owner, Vector3 connectionPoint, WheelID wheelID) {
			Kart = owner;
			WheelID = wheelID;

			ID = IDs.New;

			Node = Kart.Node.CreateChildSceneNode("wheelNode" + ID, connectionPoint - new Vector3(0, 0.5f, 0));
			Entity = LKernel.Get<SceneManager>().CreateEntity("wheelNode" + ID, "kart/KartWheel.mesh");
			Node.AttachObject(Entity);

			AccelerateMultiplier = 0;
			TurnMultiplier = 0;
			IsBrakeOn = false;

			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		/// <summary>
		/// Makes a wheel at the given position
		/// </summary>
		public void CreateWheel(Vector3 connectionPoint, bool isFrontWheel) {
			Kart.Vehicle.AddWheel(connectionPoint, WheelDirection, WheelAxle, SuspensionRestLength, Radius, Kart.Tuning, isFrontWheel);

			WheelInfo info = Kart.Vehicle.GetWheelInfo((int) WheelID);
			info.SuspensionStiffness = SpringStiffness;
			info.WheelDampingRelaxation = SpringDamping;
			info.WheelDampingCompression = SpringCompression;
			info.FrictionSlip = FrictionSlip;
			info.RollInfluence = RollInfluence;
		}

		float spin = 0; // rads
		Matrix4 previousTransform;
		/// <summary>
		/// Update our node's orientation. I'd still like a way to figure out how to update its position based on the suspension, but oh well.
		/// </summary>
		bool FrameEnded(FrameEvent evt) {
			if (!LKernel.Get<LevelManager>().IsValidLevel || Pauser.IsPaused)
				return true;

			// only update the node if it changed
			if (previousTransform != Kart.Vehicle.GetWheelTransformWS((int) WheelID))
				previousTransform = Kart.Vehicle.GetWheelTransformWS((int) WheelID);
			else
				return true;

			Node.Position = previousTransform.GetTrans();
			Node.Orientation = previousTransform.ExtractQuaternion();

			/////////////////
#if DEBUG
			//if (drawLines && Kart == LKernel.Get<Players.PlayerManager>().MainPlayer.Kart) {
				//DebugDrawer.Singleton.BuildLine(Shape.GlobalPosition, lastPos, ColourValue.Blue, 1);
				//lastPos = Shape.GlobalPosition;
			//}
#endif

			return true;
		}
		//Vector3 lastPos = Vector3.ZERO;
		public static bool drawLines = false;

		/// <summary>
		/// Apply some torque to the engine.
		/// </summary>
		protected void Accelerate() {
			Kart.Vehicle.ApplyEngineForce(MotorForce * AccelerateMultiplier, (int) WheelID);
		}

		/// <summary>
		/// Apply some brake torque.
		/// </summary>
		protected void Brake() {
			if (IsBrakeOn)
				Kart.Vehicle.SetBrake(BrakeForce, (int) WheelID);
			else
				Kart.Vehicle.SetBrake(0, (int) WheelID);
		}

		float slowSpeed = 0;
		float highSpeed = 40;
		float idealSteerAngle = 0;
		static readonly float one_degree = Math.PI / 180;
		/// <summary>
		/// Rotates our wheels.
		/// </summary>
		protected void Turn() {
			// this bit lets us do sharper turns when we move slowly, but less sharp turns when we're going fast. Works better!
			float speedTurnMultiplier = 1;
			/*if (Shape.AxleSpeed < slowSpeed)
				speedTurnMultiplier = 1.5f;
			else if (Shape.AxleSpeed > highSpeed)
				speedTurnMultiplier = 1;
			else {
				float relativeSpeed = Shape.AxleSpeed - slowSpeed;
				float maxRelativeSpeed = highSpeed - slowSpeed;
				speedTurnMultiplier = 1 + (Math.Cos((relativeSpeed * Math.PI) / (maxRelativeSpeed * 2)) / 2);
			}*/
			idealSteerAngle = TurnAngle.ValueRadians * TurnMultiplier * speedTurnMultiplier;

			float currentAngle = Kart.Vehicle.GetSteeringValue((int) WheelID);
			
			// smooth out the turning
			if (currentAngle < idealSteerAngle) {
				if (currentAngle + one_degree <= idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(currentAngle + one_degree, (int) WheelID);
				else if (currentAngle + one_degree > idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(idealSteerAngle, (int) WheelID);
			}
			else if (currentAngle > idealSteerAngle) {
				if (currentAngle - one_degree >= idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(currentAngle - one_degree, (int) WheelID);
				else if (currentAngle - one_degree < idealSteerAngle)
					Kart.Vehicle.SetSteeringValue(idealSteerAngle, (int) WheelID);
			}
		}

		/// <summary>
		/// clean up stuff
		/// </summary>
		public void Dispose() {
			LKernel.Get<Root>().FrameEnded -= FrameEnded;
			// dispose of mogre stuff? I suppose we don't need to since we aren't going to be disposing karts in the middle of a level
		}
	}

	public enum WheelID {
		/// <summary>
		/// 0
		/// </summary>
		FrontLeft = 0,
		/// <summary>
		/// 1
		/// </summary>
		FrontRight = 1,
		/// <summary>
		/// 2
		/// </summary>
		BackLeft = 2,
		/// <summary>
		/// 3
		/// </summary>
		BackRight = 3,
	}
}
