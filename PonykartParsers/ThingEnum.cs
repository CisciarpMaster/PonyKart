
namespace PonykartParsers {
	/// <summary>
	/// All of the different enums we can get from the .thing and .muffin files
	/// </summary>
	public enum ThingEnum {
		// for the ThingParser

		// physics type
		Dynamic,
		Kinematic,
		Static,
		None,

		// physics shape
		Box,
		/// <summary> a capsule aligned with the Y axis </summary>
		Capsule,
		Sphere,
		Cylinder,
		Cone,
		Hull,

		// collision groups
		//None,
		All,
		Default,
		Environment,
		Affectors,
		Walls,
		Triggers,
		Karts,

		// billboard types
		/// <summary> Standard point billboard (default), always faces the camera completely and is always upright </summary>
		Point,
		/// <summary> Billboards are oriented around a shared direction vector (used as Y axis) and only rotate around this to face the camera </summary>
		OrientedCommon,
		/// <summary> Billboards are oriented around their own direction vector (their own Y axis) and only rotate around this to face the camera </summary>
		OrientedSelf,
		/// <summary> Billboards are perpendicular to a shared direction vector (used as Z axis, the facing direction) and X, Y axis are determined by a shared up-vector </summary>
		PerpendicularCommon,
		/// <summary> Billboards are perpendicular to their own direction vector (their own Z axis, the facing direction) and X, Y axis are determined by a shared up-vector </summary>
		PerpendicularSelf,

		// billboard origins
		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		Center,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight,

		// for the MuffinParser
		Menu,
		Race,
		EmptyLevel,
	}
}
