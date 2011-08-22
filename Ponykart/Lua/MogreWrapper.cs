using LuaNetInterface;
using Mogre;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class MogreWrapper {

		public MogreWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		

		[LuaFunction("vectorToQuaternion", "Turns a degree vector into a global quaternion", "Vector3 vec")]
		public static Quaternion vec2quat(Vector3 vec) {
			return vec.DegreeVectorToGlobalQuaternion();
		}
	}
}
