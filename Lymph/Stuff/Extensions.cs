using Mogre;
using Math = Mogre.Math;

namespace Ponykart {
	/// <summary>
	/// Some extension methods 
	/// </summary>
	public static class Extensions {
		/// <summary>
		/// Extension method for Vector3. Turns this vector into a quaternion. This must be in radians!
		/// </summary>
		/// <returns>A quaternion</returns>
		public static Quaternion ToQuaternion(this Vector3 vec) {
			return EulerToQuat(vec.x, vec.y, vec.z);
		}

		/// <summary>
		/// If you have a vector to be used for rotation but it's in degrees and you want radians, use this!
		/// </summary>
		public static Vector3 DegreeVectorToRadianVector(this Vector3 vec) {
			return new Vector3(Math.DegreesToRadians(vec.x), Math.DegreesToRadians(vec.y), Math.DegreesToRadians(vec.z));
		}

		/// <summary>
		/// If you have a vector to be used for rotation but it's in radians and you want degrees, use this!
		/// </summary>
		public static Vector3 RadianVectorToDegreeVector(this Vector3 vec) {
			return new Vector3(Math.RadiansToDegrees(vec.x), Math.RadiansToDegrees(vec.y), Math.RadiansToDegrees(vec.z));
		}

		/// <summary>
		/// Extension method for Quaternion. Given three euler radian angles, we make a new quaternion from those angles and return it.
		/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
		/// do "Quaternion newQuat = new Quaternion().FromEuler(x, y, z);"
		/// </summary>
		/// <param name="rotX">Rotation (in radians) on the X axis</param>
		/// <param name="rotY">Rotation (in radians) on the Y axis</param>
		/// <param name="rotZ">Rotation (in radians) on the Z axis</param>
		/// <returns>A new quaternion</returns>
		public static Quaternion FromEuler(this Quaternion quat, Radian rotX, Radian rotY, Radian rotZ) {
			return EulerToQuat(rotX, rotY, rotZ);
		}

		/// <summary>
		/// Extension method for Quaternion. Given three euler angles, we make a new quaternion from those angles and return it.
		/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
		/// do "Quaternion newQuat = new Quaternion().FromEuler(x, y, z);".
		/// Alternatively, you could do "Quaternion newQuat = myVector.ToQuaternion();".
		/// </summary>
		/// <param name="rotations">
		/// A vector3 representing the rotations on the appropriate axes, so that rotations.x represents
		/// the rotation on the X axis, etc. This must be in radians! Use Vector3.DegreeVectorToRadianVector() to convert it!
		/// </param>
		/// <returns>A new quaternion</returns>
		public static Quaternion FromEuler(this Quaternion quat, Vector3 rotations) {
			return EulerToQuat(rotations.x, rotations.y, rotations.z);
		}

		/// <summary>
		/// Converts three euler angles to an equivalent quaternion. This assumes that the given angles are in radians and not degrees.
		/// </summary>
		private static Quaternion EulerToQuat(Radian rotX, Radian rotY, Radian rotZ) {
			// Assuming the angles are in radians.
			float c1 = (float)Math.Cos(rotY / 2);
			float s1 = (float)Math.Sin(rotY / 2);
			float c2 = (float)Math.Cos(rotZ / 2);
			float s2 = (float)Math.Sin(rotZ / 2);
			float c3 = (float)Math.Cos(rotX / 2);
			float s3 = (float)Math.Sin(rotX / 2);
			float c1c2 = c1 * c2;
			float s1s2 = s1 * s2;
			float w = c1c2 * c3 - s1s2 * s3;
			float x = c1c2 * s3 + s1s2 * c3;
			float y = s1 * c2 * c3 + c1 * s2 * s3;
			float z = c1 * s2 * c3 - s1 * c2 * s3;

			return new Quaternion(w, x, y, z);
		}
	}
}
