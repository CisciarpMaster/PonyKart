
namespace Ponykart.Stuff {
	/// <summary>
	/// Defines different types of move behaviour
	/// </summary>
	public enum MoveBehaviour {
		/// <summary>
		/// Towards the cart in front of the one that spawned this thing
		/// </summary>
		TOWARDS_NEXT_KART,
		/// <summary>
		/// In a straight line, either stopping when it hits something or bouncing off it
		/// </summary>
		STRAIGHT_LINE,
		/// <summary>
		/// Towards whoever's in 1st place (a la blue shells)
		/// </summary>
		TOWARDS_FIRST_KART,
		/// <summary>
		/// This means the Movement manager should ignore it - it does not mean that this does not move, but it usually does
		/// </summary>
		IGNORE
	}
}
