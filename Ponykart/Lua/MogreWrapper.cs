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

		[LuaFunction("addVectors", "one + two", "Vector3 one", "Vector3 two")]
		public static Vector3 addVec(Vector3 vec1, Vector3 vec2) {
			return vec1 + vec2;
		}

		[LuaFunction("subtractVectors", "one - two", "Vector3 one", "Vector3 two")]
		public static Vector3 subtractVec(Vector3 vec1, Vector3 vec2) {
			return vec1 - vec2;
		}

		[LuaFunction("multiplyVectors", "one * two", "Vector3 one", "Vector3 two")]
		public static Vector3 multiplyVec(Vector3 vec1, Vector3 vec2) {
			return vec1 * vec2;
		}

		[LuaFunction("scaleVector", "one * two", "Vector3 one", "float two")]
		public static Vector3 multiplyVec(Vector3 vec1, float scalar) {
			return vec1 * scalar;
		}
	}
}
