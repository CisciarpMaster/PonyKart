using System;

namespace Ponykart {
	/// <summary>
	/// Attributes to go on handlers. Make sure LevelHandlers implement ILevelHandler!
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class HandlerAttribute : Attribute {
		public readonly HandlerScope Scope;
		public readonly LevelType LevelType;

		public HandlerAttribute(HandlerScope scope, LevelType levelType = LevelType.All) {
			Scope = scope;
			LevelType = levelType;
		}
	}
}
