using BulletSharp;

namespace Ponykart.Physics {
	public class PhysicsMaterials {
		public Material NoFrictionMaterial { get; private set; }

		MaterialDesc noFrictionDesc;

		public PhysicsMaterials() {
			Launch.Log("[Loading] Creating PhysXMaterials");

			noFrictionDesc = new MaterialDesc();
			noFrictionDesc.Flags |= MaterialFlags.DisableFriction;
		}

		public void SetupMaterialsForWorld(DynamicsWorld scene) {
			// Sets default material
			var defaultMaterial = scene.Materials[0];
			// restitution = bounciness
			defaultMaterial.Restitution = 0.5f;
			defaultMaterial.DynamicFriction = 0.6f;
			defaultMaterial.StaticFriction = 0.6f;

			// material with no friction
			NoFrictionMaterial = scene.CreateMaterial(noFrictionDesc);
		}
	}
}
