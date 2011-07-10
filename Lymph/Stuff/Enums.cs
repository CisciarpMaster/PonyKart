
namespace Ponykart.Stuff {
	/// <summary>
	/// RenderQueueGroup... L-something.
	/// Enum to represent different render queue groups we stick things in so they're rendered correctly.
	/// 1 = rendered firs
	/// 100 = rendered last
	/// default is... 50 I think?
	/// </summary>
	public enum RQGL {
		/// <summary> 50 (The background that goes behind everything) </summary>
		BACKGROUND = 50,
		/// <summary> 50 (The level itself) </summary>
		LEVEL = 50,
		/// <summary> 60 (Ribbon emitters) </summary>
		RIBBONS = 50,
		/// <summary> 50 (Lymphy, enemies, etc) </summary>
		CELL_CHARACTERS = 50,
		/// <summary> 50 (The organelles inside the cells) </summary>
		ORGANELLES = 50,
		/// <summary> 50 (Antibody meshes) </summary>
		ANTIBODIES = 50,
		/// <summary> 70 (Faces) </summary>
		FACES = 70,
		/// <summary> 100 (The foreground of the level, i.handler. things that you want to obscure everything else) </summary>
		FOREGROUND = 100
	}

	/// <summary>
	/// Defines different types of move behaviour
	/// </summary>
	public enum MoveBehaviour {
		TOWARDS_PLAYER,
		AWAY_FROM_PLAYER,
		RANDOM,
		/// <summary>
		/// This means the Movement manager should ignore it - it does not mean that this does not move, but it usually does
		/// </summary>
		IGNORE
	}
}
