
namespace Ponykart {
	/// <summary>
	/// Specifies whether the handler is a global or level handler
	/// </summary>
	public enum HandlerScope {
		/// <summary>
		/// Disposed on level unload, recreated on level load
		/// </summary>
		Level,
		/// <summary>
		/// Only created once in the whole lifetime of the game
		/// </summary>
		Global
	}
}
