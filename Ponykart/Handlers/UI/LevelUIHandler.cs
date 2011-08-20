using System.Collections.Generic;
using System.Collections.ObjectModel;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.UI;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles making the UI for both the levels and the main menu (because the main menu is considered to be a level
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LevelUIHandler {
		private Button commandsButton, level1Button, level2Button, level3Button, level4Button, quitButton;
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
			Launch.Log("[Loading] Creating LevelUIHandler");

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
					"[W A S D] Move and turn\r\n" +
					"[Space] Brake\r\n" + 
					"[0 1 2 3...] Change level\r\n" +
					"[K] Spawn another kart\r\n" +
					"[Esc] Exit / Close lua console\r\n" +
					"[-] Toggle OGRE debug info\r\n" +
					"[M] Turn music on or off (requires level reload)\r\n" +
					"[P] Turn sounds on or off\r\n" +
					"[N] Play music now\r\n" +
					"[L] Run a test lua script\r\n" +
					"[C] Syncs the media folder and restarts Lua\r\n" + 
					"[U] Apply an upwards force\r\n" + 
					"[B] Spawn a primitive\r\n" + 
					"[F] Multiply your speed by 2\r\n" +
#if DEBUG
					"[I] Toggles debug lines\r\n" +
#endif
					"[backtick] Pause\r\n" +
					"[enter] Open Lua console",
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

			level1Button = new Button("shittyterrain") {
				Location = new Point((int)(Constants.WINDOW_WIDTH / 2) - 100, 50), // the 100 is half of 200, which makes sure the button is centered
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "shittyterrain",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(level1Button);
			level1Button.MouseClick += (o, e) => LKernel.Get<LevelManager>().LoadLevel("shittyterrain");

			level2Button = new Button("flat") {
				Location = new Point((int) (Constants.WINDOW_WIDTH / 2) - 100, 100),
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "flat",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(level2Button);
			level2Button.MouseClick += (o, e) => LKernel.Get<LevelManager>().LoadLevel("flat");

			level3Button = new Button("testlevel") {
				Location = new Point((int) (Constants.WINDOW_WIDTH / 2) - 100, 150),
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "testlevel",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(level3Button);
			level3Button.MouseClick += (o, e) => LKernel.Get<LevelManager>().LoadLevel("testlevel");

			level4Button = new Button("sweet apple acres") {
				Location = new Point((int) (Constants.WINDOW_WIDTH / 2) - 100, 200),
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "sweet apple acres",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(level4Button);
			level4Button.MouseClick += (o, e) => LKernel.Get<LevelManager>().LoadLevel("saa_0.7");

			quitButton = new Button("Quit") {
				Location = new Point((int)(Constants.WINDOW_WIDTH / 2) - 100, 250),
				Size = new Size(200, 40),
				Skin = UIResources.Skins["ButtonSkin"],
				Text = "Quit",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White
				},
			};
			mainMenuControls.Add(quitButton);
			quitButton.MouseClick += (o, e) => Main.quit = true; // apparently this is one way to quit

			foreach (Control c in mainMenuControls)
				Gui.Controls.Add(c);
		}

		/// <summary>
		/// Decides which UI to make when a level is loaded
		/// </summary>
		private void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			//Launch.Log("LevelUIHandler level load: old level: " + eventArgs.OldLevelId.Id + " new level: " + eventArgs.NewLevelId.Id);
			if (eventArgs.NewLevel.Name == Settings.Default.MainMenuName)
			{
				// when going to the menu
				MakeMainMenuUI();
			}
			else if (eventArgs.OldLevel.Name == Settings.Default.MainMenuName)
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
			if (eventArgs.OldLevel.Name == Settings.Default.MainMenuName)
			{
				foreach (Control c in mainMenuControls) {
					c.Dispose();
				}
				mainMenuControls.Clear();
			}
			else if (eventArgs.NewLevel.Name == Settings.Default.MainMenuName)
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