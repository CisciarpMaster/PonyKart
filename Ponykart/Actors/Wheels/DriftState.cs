
namespace Ponykart.Actors {
	/// <summary>
	/// Lets us keep track of whether we're drifting or not, and if we are, which direction we're drifting in
	/// </summary>
	public enum DriftState {
		/// <summary>
		/// Turn angle is zero
		/// </summary>
		Normal,
		/// <summary>
		/// Turn angle is negative
		/// </summary>
		DriftLeft,
		/// <summary>
		/// Turn angle is positive
		/// </summary>
		DriftRight,
	}
}
