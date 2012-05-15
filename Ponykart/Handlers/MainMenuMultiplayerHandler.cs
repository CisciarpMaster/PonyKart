using Miyagi.Common.Events;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This handler responds to level and character selection events from the main menu, holds on to them, then loads the appropriate level with the right character.
	/// 
	/// 
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MainMenuMultiplayerHandler {
		// just keeping this as a field since I'll be using it so much
		MainMenuManager mmm;
		NetworkManager netMgr;
		string _levelSelection;
		public string LevelSelection {
			set {
				_levelSelection = value;
			}
		}
		string characterSelection;

		public MainMenuMultiplayerHandler() {
			mmm = LKernel.GetG<MainMenuManager>();
			netMgr = LKernel.GetG<NetworkManager>();

			mmm.OnLevelSelect += new MainMenuLevelSelectEvent(OnLevelSelect);
			mmm.OnCharacterSelect += new MainMenuCharacterSelectEvent(OnCharacterSelect);
			mmm.OnHostInfo_SelectNext += new MainMenuButtonPressEvent(OnHostInfo_SelectNext);
			mmm.OnClientInfo_SelectNext += new MainMenuButtonPressEvent(OnClientInfo_SelectNext);
		}

		/// <summary>
		/// Since character selection at the moment is the final stage in the menus, this loads the new level based on the previous
		/// level selection and current character selection
		/// </summary>
		void OnCharacterSelect(Button button, MouseButtonEventArgs eventArgs, string characterSelection) {
			//This will need to do other things at some point.
			this.characterSelection = characterSelection;
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {

				LevelChangeRequest request = new LevelChangeRequest() {
					NewLevelName = _levelSelection,
					CharacterNames = new string[] { characterSelection },
				};
				LKernel.GetG<LevelManager>().LoadLevel(request);
                netMgr.ForEachConnection(c => c.SendPacket(Commands.StartGame, "", false));
                
			}
		}

		/// <summary>
		/// Called to initiate a host network thread
		/// </summary>
		void OnHostInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs) {
			netMgr.InitManager(int.Parse(mmm.NetworkHostPortTextBox.Text),
							   mmm.NetworkHostPasswordTextBox.Text);
			netMgr.StartThread(1);
		}

		/// <summary>
		/// Called to initiate a client network thread
		/// </summary>
		void OnClientInfo_SelectNext(Button button, MouseButtonEventArgs eventArgs) {

			netMgr.InitManager(int.Parse(mmm.NetworkClientPortTextBox.Text),
							   mmm.NetworkClientPasswordTextBox.Text,
                               mmm.NetworkClientIPTextBox.Text);
            netMgr.StartThread(1);
			netMgr.SingleConnection.SendPacket(Commands.Connect, mmm.NetworkClientPasswordTextBox.Text);
		}
		/// <summary>
		/// Saves the chosen level for later
		/// </summary>
		void OnLevelSelect(Button button, MouseButtonEventArgs eventArgs, string levelSelection) {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {
				this.LevelSelection = levelSelection;
				netMgr.ForEachConnection(c => c.SendPacket(Commands.SelectLevel, levelSelection));
			}
		}

		// why is this in here? :U 
		public void Start_Game() {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {

				LevelChangeRequest request = new LevelChangeRequest() {
					NewLevelName = _levelSelection,
					CharacterNames = new string[] { characterSelection ?? "Twilight Sparkle" },
				};
				LKernel.GetG<LevelManager>().LoadLevel(request);
			}
		}
	}
}