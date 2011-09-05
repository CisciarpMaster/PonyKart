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
			LKernel.GetG<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
			LKernel.GetG<LevelManager>().OnLevelUnload += new LevelEventHandler(OnLevelUnload);
		}

		/// <summary>
		/// When a level loads, we create the players. For now, we just have one human player and 7 computer-controlled ones
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			Players = new Player[Settings.Default.NumberOfPlayers];

			MainPlayer = new HumanPlayer(0);
			Players[0] = MainPlayer;

			for (int a = 1; a < Settings.Default.NumberOfPlayers; a++) {
				Players[a] = new ComputerPlayer(a);
			}
		}

		/// <summary>
		/// Dispose of all of the players
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			for (int a = 0; a < Players.Length; a++) {
				if (Players[a] != null) {
					Players[a].Dispose();
					Players[a] = null;
				}
			}
			MainPlayer = null;
		}
	}


}
