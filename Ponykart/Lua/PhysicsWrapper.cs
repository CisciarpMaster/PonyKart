using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Physics;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class PhysicsWrapper {

		public PhysicsWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("getBodyName", "Gets the name of this body.", "The body you want to get the name of.")]
		public static string GetBodyName(CollisionObject obj) {
			return obj.GetName();
		}

		[LuaFunction("getCollisionGroup", "Gets the collision group of this body.", "The body you want to get the collision group of.")]
		public static PonykartCollisionGroups GetCollisionGroup(CollisionObject obj) {
			return obj.GetCollisionGroup();
		}

		[LuaFunction("addConstraint", "Adds a constraint to the physics world", "TypedConstraint - The constraint to add", "bool - disable collisions between affected bodies?")]
		public static void AddConstraint(TypedConstraint constraint, bool disableCollisionsBetweenBodies) {
			LKernel.GetG<PhysicsMain>().World.AddConstraint(constraint, disableCollisionsBetweenBodies);
		}

		[LuaFunction("setBodyOrientation", "Sets the orientation of a RigidBody", "RigidBody", "Quaternion")]
		public static void SetBodyOrientation(RigidBody body, Quaternion quat) {
			Matrix4 mat = new Matrix4(quat);
			mat[0, 3] = body.WorldTransform[0, 3];
			mat[1, 3] = body.WorldTransform[1, 3];
			mat[2, 3] = body.WorldTransform[2, 3];
			mat[3, 3] = body.WorldTransform[3, 3];
			body.WorldTransform = mat;
		}

		[LuaFunction("setBodyPosition", "Sets the position of a RigidBody", "RigidBody", "Vector3")]
		public static void SetBodyPosition(RigidBody body, Vector3 vec) {
			body.WorldTransform.SetTrans(vec);
		}

		/*
		 * ActiveTag = 1,
		 * IslandSleeping = 2,
		 * WantsDeactivation = 3,
		 * DisableDeactivation = 4,
		 * DisableSimulation = 5,
		 */
		[LuaFunction("forceActivationState", "Forces the activation state of a RigidBody to be something.", "", "")]
		public static void ForceActivationState(LThing thing, int activationState) {
			if (thing != null && thing.Body != null)
				thing.Body.ForceActivationState((ActivationState) activationState);
		}

		[LuaFunction("deactivateThing", "Deactivates a LThing's physics body. This means it won't move until something else (that's moving) comes near it.",
			"LThing - the thing to deactivate")]
		public static void DeactivateThing(LThing thing) {
			if (thing != null && thing.Body != null)
				thing.Body.ForceActivationState(ActivationState.WantsDeactivation);
		}
	}
}
