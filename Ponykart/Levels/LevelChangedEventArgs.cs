
namespace Ponykart.Levels {
	public struct LevelChangedEventArgs {

		/// <summary>
		/// If you're reading this from OnLevelUnload or OnLevelPreUnload, keep in mind that the new level has not been
		/// fully initialised yet, so properties such as Type and stuff will probably not be correct!.
		/// </summary>
		public Level NewLevel;

		public Level OldLevel;

		public LevelChangedEventArgs(Level newLevel, Level oldLevel) {
			NewLevel = newLevel;
			OldLevel = oldLevel;
		}
	}
}
