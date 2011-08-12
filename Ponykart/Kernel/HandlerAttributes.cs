using System;

namespace Ponykart {
	/// <summary>
	/// Attributes to go on handlers. Make sure LevelHandlers implement ILevelHandler!
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class HandlerAttribute : Attribute {
		public readonly HandlerScope Scope;

		public HandlerAttribute(HandlerScope scope) {
			Scope = scope;
		}
	}

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
