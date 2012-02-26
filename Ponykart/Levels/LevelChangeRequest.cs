
namespace Ponykart.Levels {
	/// <summary>
	/// A little class to hold data that needs to be passed from one level to another to create specific stuff
	/// </summary>
	public class LevelChangeRequest {
		public string NewLevelName { get; set; }
		public string CharacterName { get; set; }

		// add more stuff as needed here
	}
}
