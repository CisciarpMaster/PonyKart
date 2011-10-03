
namespace Ponykart.Levels {
	public struct LevelChangedEventArgs {

		/// <summary>
		/// If you're reading this from OnLevelUnload or OnLevelPreUnload, keep in mind that the new level has not been
		/// fully initialised yet, so properties such as Type and stuff will probably not be correct!.
		/// </summary>
		public Level NewLevel { get; set; }

		public Level OldLevel { get; set; }
	}
}
