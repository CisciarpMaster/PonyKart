using LuaNetInterface;
using Mogre;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class MogreWrapper {

		public MogreWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("vector", "Creates a Mogre.Vector3 object", "x", "y", "z")]
		public static Vector3 vec(float x, float y, float z) {
			return new Vector3(x, y, z);
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

		// ------------------------------------

		[LuaFunction("quaternion", "Creates a Mogre.Quaternion object", "x", "y", "z", "w")]
		public static Quaternion quat(float x, float y, float z, float w) {
			return new Quaternion(w, x, y, z);
		}
	}
}
