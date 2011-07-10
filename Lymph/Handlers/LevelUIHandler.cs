using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lymph.Levels;
using Lymph.UI;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;

namespace Lymph.Handlers {
	/// <summary>
	/// This class handles making the UI for both the levels and the main menu (because the main menu is considered to be a level
	/// </summary>
	public class LevelUIHandler {
		private Button commandsButton, level1Button, level2Button, level3Button, level4Button, level5Button, level6Button, quitButton;
		private Label commandsLabel;
		private ICollection<Control> levelControls, mainMenuControls;

		public LevelUIHandler() {
			LKernel.Get<LevelManager>().OnLevelLoad += OnLevelLoad;
			LKernel.Get<LevelManager>().OnLevelUnload += OnLevelUnload;

			levelControls = new Collection<Control>();
			mainMenuControls = new Collection<Control>();
		}

		/// <summary>
		/// Make the level UI
		/// </summary>
		void MakeLevelUI() {
			GUI Gui = LKernel.Get<UIMain>().Gui;
			levelControls.Clear();

			commandsButton = new Button("show/hide commands button") {
				Location = new Point(10, 10),
				Size = new Size(120, 25),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "Show Commands",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
				UserData = new UIUserData {
					ObstructsViewport = true,
				}
			};
			// subscribe to the events that change the current texture
			commandsButton.MouseDown += CommandsButton_MouseDown;
			levelControls.Add(commandsButton);

			commandsLabel = new Label("commands label") {
				Location = new Point(10, 60),
				Size = new Size(400, 700),
				TextStyle = {
					Alignment = Alignment.TopLeft,
					ForegroundColour = Colours.White,
					WordWrap = true,
					Multiline = true
				},
				UserData = new UIUserData {
					ObstructsViewport = false,
				},
				Visible = false,
				Text =
					"[W A S D] Move\r\n" +
					"[0 1 2 3 4 5 6] Change level\r\n" +
					"[Z X] Spawn generic enemy\r\n" +
					"[Esc] Exit\r\n" +
					"[-] Toggle OGRE debug box\r\n" +
					"[M] Turn music on or off (requires level reload)\r\n" +
					"[P] Turn sounds on or off\r\n" +
					"[N] Play music now\r\n" +
					"[L] Run a test lua script\r\n" +
					"[C] Syncs the media folder and restarts Lua.\r\n" + 
					"[backtick] Pause\r\n" +
					"[enter] Toggle Lua console",
			};
			levelControls.Add(commandsLabel);

			foreach (Control c in levelControls) 
				Gui.Controls.Add(c);
		}

		/// <summary>
		/// Make the UI for the main menu
		/// </summary>
		void MakeMainMenuUI() {
			GUI Gui = LKernel.Get<UIMain>().Gui;
			mainMenuControls.Clear();

			level1Button = new Button("Level 1") {
				Location = new Point((int)(Constants.WINDOW_WIDTH / 2) - 100, 50), // the 100 is half of 200, which makes sure the button is centered
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "Level 1",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(level1Button);
			level1Button.MouseClick += (o, e) => LKernel.Get<LevelManager>().LoadLevel("shittyterrain");

			quitButton = new Button("Quit") {
				Location = new Point((int)(Constants.WINDOW_WIDTH / 2) - 100, 100), // the 100 is half of 200, which makes sure the button is centered
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "Quit",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(quitButton);
			quitButton.MouseClick += (o, e) => LKernel.Get<Mogre.Root>().Dispose(); // apparently this is one way to quit

			foreach (Control c in mainMenuControls)
				Gui.Controls.Add(c);
		}

		/// <summary>
		/// Decides which UI to make when a level is loaded
		/// </summary>
		private void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			//Launch.Log("LevelUIHandler level load: old level: " + eventArgs.OldLevelId.Id + " new level: " + eventArgs.NewLevelId.Id);
			if (eventArgs.NewLevelId.Name == Settings.Default.FirstLevelName)
			{
				// when going to the menu
				MakeMainMenuUI();
			}
			else if (eventArgs.OldLevelId.Name == Settings.Default.FirstLevelName)
			{
				// when going from the menu to something else
				MakeLevelUI();
			}
			// don't need to remake it if we're going between levels
		}

		/// <summary>
		/// Dispose of the current UI when unloading a level
		/// </summary>
		/// <param name="eventArgs"></param>
		private void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			//Launch.Log("LevelUIHandler level unload: old level: " + eventArgs.OldLevelId.Id + " new level: " + eventArgs.NewLevelId.Id);
			if (eventArgs.OldLevelId.Name == Settings.Default.FirstLevelName)
			{
				foreach (Control c in mainMenuControls) {
					c.Dispose();
				}
				mainMenuControls.Clear();
			}
			else if (eventArgs.NewLevelId.Name == Settings.Default.FirstLevelName)
			{
				foreach (Control c in levelControls) {
					c.Dispose();
				}
				levelControls.Clear();
			}
		}

		// ======================================================

		private void CommandsButton_MouseDown(object sender, MouseButtonEventArgs e) {
			if (((Button)sender).Text == "Show Commands") {
				((Button)sender).Text = "Hide Commands";
				commandsLabel.Visible = true;
			} else {
				((Button)sender).Text = "Show Commands";
				commandsLabel.Visible = false;
			}
		}
	}
}