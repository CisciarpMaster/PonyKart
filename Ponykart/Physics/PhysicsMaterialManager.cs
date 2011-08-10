using BulletSharp;

namespace Ponykart.Physics {
	public class PhysicsMaterialManager {
		public PhysicsMaterial NoFrictionNoBounceMaterial { get; private set; }

		public PhysicsMaterialManager() {
			Launch.Log("[Loading] Creating PhysicsMaterialManager");

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
