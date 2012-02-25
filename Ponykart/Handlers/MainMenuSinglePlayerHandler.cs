﻿using Miyagi.Common.Events;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This handler responds to level and character selection events from the main menu, holds on to them, then loads the appropriate level with the right character.
	/// 
	/// At the moment it's only for single player and it's pretty rough. It'll probably need to be rewritten a bunch when we come to adding multiplayer.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MainMenuSinglePlayerHandler {
		string levelSelection;
		string characterSelection;

		public MainMenuSinglePlayerHandler() {
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
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.SinglePlayer) {
				this.characterSelection = characterSelection;

				LevelChangeRequest request = new LevelChangeRequest() {
					NewLevelName = levelSelection,
					CharacterName = characterSelection
				};
				LKernel.GetG<LevelManager>().LoadLevel(request);
			}
		}

		/// <summary>
		/// Saves the chosen level for later
		/// </summary>
		void OnLevelSelect(Button button, MouseButtonEventArgs eventArgs, string levelSelection) {
			if (LKernel.Get<MainMenuUIHandler>().GameType == GameTypeEnum.SinglePlayer) {
				this.levelSelection = levelSelection;
			}
		}
	}
}
