
namespace Ponykart.Levels {
	/// <summary>
	/// A little struct to hold data that needs to be passed from one level to another to create specific stuff
	/// </summary>
	public struct LevelChangeRequest {
		public string NewLevelName { get; set; }
		public string CharacterName { get; set; }

		// add more stuff as needed here
	}
}
