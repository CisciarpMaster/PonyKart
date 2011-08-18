using BulletSharp;
using LuaNetInterface;
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
	}
}
