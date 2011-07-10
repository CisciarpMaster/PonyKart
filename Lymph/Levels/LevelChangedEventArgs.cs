using System;

namespace Ponykart.Levels
{
	public class LevelChangedEventArgs : EventArgs
	{
		public LevelChangedEventArgs(Level newLevelId, Level oldLevelId)
		{
			NewLevelId = newLevelId;
			OldLevelId = oldLevelId;
		}

		public Level NewLevelId { get; private set; }
		public Level OldLevelId { get; private set; }
	}
}
