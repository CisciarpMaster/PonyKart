using BulletSharp;
using LuaNetInterface;
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class PhysicsWrapper {

		public PhysicsWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
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
			LKernel.Get<PhysicsMain>().World.AddConstraint(constraint, disableCollisionsBetweenBodies);
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
	}
}
