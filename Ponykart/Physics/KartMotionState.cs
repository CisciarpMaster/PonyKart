using BulletSharp;
using Mogre;
using Ponykart.Actors;

namespace Ponykart.Physics {
	/// <summary>
	/// MotionState specifically for karts. Mostly just updates the wheels as well.
	/// </summary>
	public class KartMotionState : MogreMotionState {
		protected Kart Kart;

		public KartMotionState(Matrix4 transform, SceneNode node, Kart kart)
			: base(transform, node)
		{
			Kart = kart;
		}

		public KartMotionState(Vector3 position, Quaternion orientation, SceneNode node, Kart kart)
			: base(position, orientation, node)
		{
			Kart = kart;
		}

		public KartMotionState(Vector3 position, Vector3 rotation, SceneNode node, Kart kart)
			: this(position, rotation.DegreeVectorToGlobalQuaternion(), node, kart)
		{ }

		public KartMotionState(SceneNode node, Kart kart)
			: this(node.Position, node.Orientation, node, kart)
		{ }

		public override Matrix4 WorldTransform {
			get {
				return base.WorldTransform;
			}
			set {
				base.WorldTransform = value;

				if (Kart.Vehicle == null)
					return;

				UpdateWheel(Kart.WheelFL);
				UpdateWheel(Kart.WheelFR);
				UpdateWheel(Kart.WheelBL);
				UpdateWheel(Kart.WheelBR);
			}
		}

		private void UpdateWheel(Wheel wheel) {
			WheelInfo info = Kart.Vehicle.GetWheelInfo((int) wheel.WheelID);
			wheel.Node.Position = new Vector3(wheel.Node.Position.x,
											  wheel.SuspensionRestLength - info.RaycastInfo_.SuspensionLength,
											  wheel.Node.Position.z);
			wheel.Node.Orientation = info.WorldTransform.ExtractQuaternion();
		}
	}
}
