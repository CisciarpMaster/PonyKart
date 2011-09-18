using BulletSharp;

namespace Ponykart.Physics {
	/// <summary>
	/// Little class to hold materials and assign them to RigidBodies and RigidBodyConstructionInfos
	/// </summary>
	public class PhysicsMaterialManager {
		public PhysicsMaterialManager() {
			Launch.Log("[Loading] Creating PhysicsMaterialManager");
		}

		/// <summary>
		/// Only applies friction and bounciness. Use a RigidBodyConstructionInfo if you want to set the damping.
		/// </summary>
		public void ApplyMaterial(RigidBody body, string material) {
			PhysicsMaterial mat = LKernel.GetG<PhysicsMaterialFactory>().GetMaterial(material);

			body.Friction = mat.Friction;
			body.Restitution = mat.Bounciness;
		}

		/// <summary>
		/// Applies friction, bounciness, angular damping, and linear damping
		/// </summary>
		public void ApplyMaterial(RigidBodyConstructionInfo info, string material) {
			PhysicsMaterial mat = LKernel.GetG<PhysicsMaterialFactory>().GetMaterial(material);

			info.Friction = mat.Friction;
			info.Restitution = mat.Bounciness;
			info.AngularDamping = mat.AngularDamping;
			info.LinearDamping = mat.LinearDamping;
		}
	}
}
