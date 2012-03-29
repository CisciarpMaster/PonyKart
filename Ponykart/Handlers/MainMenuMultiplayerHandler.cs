using Miyagi.Common.Events;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Networking;
using Ponykart.UI;

namespace Ponykart.Handlers
{
	/// <summary>
	/// This handler responds to level and character selection events from the main menu, holds on to them, then loads the appropriate level with the right character.
	/// 
	/// 
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MainMenuMultiplayerHandler {
		string _levelSelection;
        public string levelSelection {
            set {
                _levelSelection = value;
            }
        }
		string characterSelection;
        
		public MainMenuMultiplayerHandler() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		/// <summary>
		/// unhook from the main menu events
		/// </summary>
		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			if (eventArgs.OldLevel.Type == LevelType.Menu) {
				var mmm = LKernel.GetG<MainMenuManager>();
				mmm.OnLevelSelect -= OnLevelSelect;
				mmm.OnCharacterSelect -= OnCharacterSelect;
			}
		}

		/// <summary>
		/// hook into the main menu events
		/// </summary>
		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type == LevelType.Menu) {
				var mmm = LKernel.GetG<MainMenuManager>();
				mmm.OnLevelSelect += OnLevelSelect;
				mmm.OnCharacterSelect += OnCharacterSelect;
			}
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
            }
		}

        /// <summary>
        /// Called to initiate a host network thread
        /// </summary>
        /// <param name="mmm">MainMenuManager instance that holds textbox information</param>
        void OnHostInfo_SelectNext(MainMenuManager mmm) {
            LKernel.Get<NetworkManager>().InitManager(int.Parse(mmm.NetworkHostPortTextBox.Text),
                                                      mmm.NetworkHostPasswordTextBox.Text);
            NetworkManager.StartThread(1);
        }

        /// <summary>
        /// Called to initiate a client network thread
        /// </summary>
        /// <param name="mmm">MainMenuManager instance that holds textbox information</param>
        void OnClientInfo_SelectNext(MainMenuManager mmm) {

            LKernel.Get<NetworkManager>().InitManager(int.Parse(mmm.NetworkClientPortTextBox.Text),
                                                      mmm.NetworkClientPasswordTextBox.Text,
                                                      mmm.NetworkClientIPTextBox.Text);
            LKernel.Get<NetworkManager>().SingleConnection.SendPacket((short)Commands.Connect, mmm.NetworkClientPasswordTextBox.Text);
            NetworkManager.StartThread(1);
        }
		/// <summary>
		/// Saves the chosen level for later
		/// </summary>
		void OnLevelSelect(Button button, MouseButtonEventArgs eventArgs, string levelSelection) {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedHost) {
				this.levelSelection = levelSelection;
                LKernel.Get<NetworkManager>().ForEachConnection(c => c.SendPacket((short)Commands.SelectLevel, levelSelection));
			}
		}

        public void Start_Game() {
            if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.NetworkedClient) {

                LevelChangeRequest request = new LevelChangeRequest() {
                    NewLevelName = _levelSelection,
                    CharacterNames = new string[] { characterSelection },
                };
                LKernel.GetG<LevelManager>().LoadLevel(request);
            }
        }
	}
}