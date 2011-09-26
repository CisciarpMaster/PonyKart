using System;

namespace Ponykart {
	/// <summary>
	/// Attributes to go on handlers. Make sure LevelHandlers implement ILevelHandler!
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class HandlerAttribute : Attribute {
		public readonly HandlerScope Scope;
		public readonly LevelType LevelType;
		public readonly string LevelNames;

		/// <param name="scope">What kind of handler is it?</param>
		/// <param name="levelType">What level types should it be on? Default = All</param>
		/// <param name="levelName">Which levels does it specifically run on? Default = All</param>
		public HandlerAttribute(HandlerScope scope, LevelType levelType = LevelType.All, string levelNames = "") {
			Scope = scope;
			LevelType = levelType;
			LevelNames = levelNames;
		}
	}

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
