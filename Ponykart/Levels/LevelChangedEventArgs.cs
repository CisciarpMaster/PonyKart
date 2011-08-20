
namespace Ponykart.Levels {
	public class LevelChangedEventArgs {
		public LevelChangedEventArgs(Level newLevel, Level oldLevel) {
			NewLevel = newLevel;
			OldLevel = oldLevel;
		}

		public Level NewLevel { get; private set; }
		public Level OldLevel { get; private set; }
	}
}
