using System;

namespace Ponykart {

	/// <summary>
	/// Represents a "type" of level
	/// </summary>
	[Flags]
	public enum LevelType {
		All = -1,
		EmptyLevel = 0,
		Menu = 1,
		Race = 2
	}
}
