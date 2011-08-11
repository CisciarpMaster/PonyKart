using BulletSharp;

namespace Ponykart.Physics {
	/// <summary>
	/// Little class to hold materials and assign them to RigidBodies and RigidBodyConstructionInfos
	/// </summary>
	public class PhysicsMaterialManager {
		public PhysicsMaterial NoFrictionNoBounceMaterial { get; private set; }
		public PhysicsMaterial DefaultMaterial { get; private set; }

		public PhysicsMaterialManager() {
			Launch.Log("[Loading] Creating PhysicsMaterialManager");

			DefaultMaterial = new PhysicsMaterial();
			NoFrictionNoBounceMaterial = new PhysicsMaterial(0, 0);
		}

		/// <summary>
		/// Only applies friction and bounciness. Use a RigidBodyConstructionInfo if you want to set the damping.
		/// </summary>
		public void ApplyMaterial(RigidBody body, PhysicsMaterial material) {
			body.Friction = material.Friction;
			body.Restitution = material.Bounciness;
		}

		/// <summary>
		/// Applies friction, bounciness, angular damping, and linear damping
		/// </summary>
		public void ApplyMaterial(RigidBodyConstructionInfo info, PhysicsMaterial material) {
			info.Friction = material.Friction;
			info.Restitution = material.Bounciness;
			info.AngularDamping = material.AngularDamping;
			info.LinearDamping = material.LinearDamping;
		}
	}
}
