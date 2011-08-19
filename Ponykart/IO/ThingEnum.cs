
namespace Ponykart.IO {
	public enum ThingEnum {
		Unassigned = 0,
		Dynamic,
		Kinematic,
		Static,
		None,
		Box,
		/// <summary> a capsule aligned with the Y axis </summary>
		Capsule,
		/// <summary> a capsule aligned with the X axis </summary>
		CapsuleX,
		/// <summary> a capsule aligned with the Z axis </summary>
		CapsuleZ,
		Sphere,
		Cylinder,
		CylinderX,
		CylinderZ,
		Cone,
		ConeX,
		ConeZ
		// ...
	}
}
