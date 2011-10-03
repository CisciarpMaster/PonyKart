
namespace Ponykart.Actors {
	/// <summary>
	/// Lets us keep track of whether we're drifting or not, and if we are, which direction we're drifting in
	/// </summary>
	public enum KartDriftState {
		/// <summary>
		/// Turn angle is zero
		/// </summary>
		None,

		/// <summary>
		/// Turn angle is negative
		/// </summary>
		FullLeft,
		/// <summary>
		/// Turn angle is positive
		/// </summary>
		FullRight,

		/// <summary>
		/// Starting to drift left
		/// </summary>
		StartLeft,
		/// <summary>
		/// Starting to drift right
		/// </summary>
		StartRight,

		/// <summary>
		/// Stopping drifting left
		/// </summary>
		StopLeft,
		/// <summary>
		/// Stopping drifting right
		/// </summary>
		StopRight,

		/// <summary>
		/// Used if we're pressing the drift button but we haven't specified which way we want to drift yet.
		/// You should unset this flag once you know the desired direction.
		/// </summary>
		WantsDriftingButNotTurning,
	}

	/// <summary>
	/// The wheels don't really care if we're starting or stopping drifting, so they use a much simpler enum.
	/// </summary>
	public enum WheelDriftState {
		None,
		Left,
		Right
	}
}
