using Ponykart.Levels;
using Ponykart.Properties;

namespace Ponykart.Players {
	/// <summary>
	/// This class manages the players
	/// </summary>
	public class PlayerManager {
		public Player MainPlayer { get; private set; }
		public Player[] Players { get; private set; }

		/// <summary>
		/// Hook up to the level load/unload events
		/// </summary>
		public PlayerManager() {
			Launch.Log("[Loading] Creating PlayerManager...");
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		/// <summary>
		/// When a level loads, we create the players. For now, we just have one human player and 7 computer-controlled ones
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Race) {
				Players = new Player[Settings.Default.NumberOfPlayers];

				MainPlayer = new HumanPlayer(eventArgs.NewLevel.Definition, 0);
				Players[0] = MainPlayer;

				for (int a = 1; a < Settings.Default.NumberOfPlayers; a++) {
					Players[a] = new ComputerPlayer(eventArgs.NewLevel.Definition, a);
				}
			}
		}

		/// <summary>
		/// Dispose of all of the players
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			if (eventArgs.OldLevel.Type == LevelType.Race) {
				for (int a = 0; a < Players.Length; a++) {
					if (Players[a] != null) {
						Players[a].Detach();
						Players[a] = null;
					}
				}
				MainPlayer = null;
			}
		}
	}


}
