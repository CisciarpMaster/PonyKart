using BulletSharp;
using Mogre;

namespace Ponykart.Physics {
	/// <summary>
	/// Handy thing that bullet has to keep graphics stuff synched up with physics stuff!
	/// Best thing is, it only updates the node when it's actually changed instead of every frame!
	/// 
	/// TODO: update sound components
	/// TODO: #if DEBUG, draw our objects using the DebugDrawer
	/// </summary>
	public class MogreMotionState : MotionState {

		protected SceneNode node;
		protected Matrix4 transform;


		public MogreMotionState(Matrix4 transform, SceneNode node) {
			this.node = node;
			this.transform = transform;
		}
		public MogreMotionState(Vector3 position, Quaternion orientation, SceneNode node) {
			transform = new Matrix4(orientation);
			transform.SetTrans(position);
			this.node = node;
		}
		/// <param name="rotation">In degrees</param>
		public MogreMotionState(Vector3 position, Vector3 rotation, SceneNode node)
			: this(position, rotation.DegreeVectorToGlobalQuaternion(), node)
		{ }

		public MogreMotionState(SceneNode node)
			: this(node.Position, node.Orientation, node)
		{ }



		public override Matrix4 WorldTransform {
			get {
				return transform;
			}
			set {
				if (node == null)
					return;

				node.Orientation = value.ExtractQuaternion();
				node.Position = value.GetTrans();
			}
		}
	}
}
