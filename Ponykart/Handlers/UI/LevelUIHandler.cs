using System.Collections.Generic;
using System.Drawing;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Ponykart.Levels;
using Ponykart.Properties;
using Ponykart.UI;
using Point = Miyagi.Common.Data.Point;
using Size = Miyagi.Common.Data.Size;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class handles making the UI for both the levels and the main menu (because the main menu is considered to be a level
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LevelUIHandler {
		private Button commandsButton, level1Button, level2Button, level3Button, level4Button, quitButton;
		private Panel playMenuPanel;
		private PictureBox checkersPicture;
		private Label commandsLabel;
		private IList<Control> levelControls, mainMenuControls;

		public LevelUIHandler() {
			Launch.Log("[Loading] Creating LevelUIHandler");

			LKernel.GetG<LevelManager>().OnLevelLoad += OnLevelLoad;
			LKernel.GetG<LevelManager>().OnLevelUnload += OnLevelUnload;

			levelControls = new List<Control>();
			mainMenuControls = new List<Control>();

			MakeLevelUI();
			MakeMainMenuUI();
		}

		/// <summary>
		/// Make the level UI
		/// </summary>
		void MakeLevelUI() {
			GUI Gui = LKernel.GetG<UIMain>().Gui;

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
				},
				Visible = false,
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
				Visible = false,
				Text =
@"[W A S D] Move and turn
[Space] Brake
[0 1 2 3...] Change level
[K] Spawn another kart
[Esc] Exit / Close lua console
[-] Toggle OGRE debug info
[M] Turn music on or off (requires level reload)
[P] Turn sounds on or off
[N] Play music now
[L] Run a test lua script
[C] Syncs the media folder and restarts Lua
[U] Apply an upwards force
[B] Spawn a primitive
[F] Multiply your speed by 2
" + 
#if DEBUG
@"[I] Toggles debug lines
" +
#endif
@"[backtick] Pause
[enter] Open Lua console",
			};
			Gui.Controls.Add(commandsLabel);

			foreach (Control c in levelControls)
				Gui.Controls.Add(c);
		}

		/// <summary>
		/// Make the UI for the main menu
		/// </summary>
		void MakeMainMenuUI() {
			GUI Gui = LKernel.GetG<UIMain>().Gui;

			// the checkers bit in the corner
			var checkerBmp = new Bitmap("media/gui/checkers.png");
			checkersPicture = new PictureBox("checkers picture") {
				Location = new Point(0, 0),
				Size = new Size(checkerBmp.Size.Width, checkerBmp.Size.Height),
				Bitmap = checkerBmp,
				TextureFiltering = TextureFiltering.Anisotropic,
				Enabled = false,
				Movable = false,
			};
			mainMenuControls.Add(checkersPicture);

			// the menu panel bit
			playMenuPanel = new Panel("play menu panel") {
				Location = new Point((int) (Settings.Default.WindowWidth / 2) - (725 / 2), (int) (Settings.Default.WindowHeight / 2) - (600 / 2)),
				Size = new Size(725, 600),
				Skin = UIResources.Skins["PKPlayMenu"],
				TextureFiltering = TextureFiltering.Anisotropic,
				Movable = false,
				ResizeMode = ResizeModes.None,
				Enabled = false,
				Anchor = AnchorStyles.HorizontalCenter,
			};
			mainMenuControls.Add(playMenuPanel);

			// some buttons
			level1Button = new Button("shittyterrain") {
				Parent = playMenuPanel,
				Location = new Point((playMenuPanel.Width / 2) + 150, 250),
				Size = new Size(300, 40),
				Skin = UIResources.Skins["PKButtonSkin"],
				Text = "shittyterrain",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["Zil"],
				},
				Enabled = true,
			};
			mainMenuControls.Add(level1Button);
			level1Button.MouseClick += (o, e) => LKernel.GetG<LevelManager>().LoadLevel("shittyterrain");

			level2Button = new Button("flat") {
				Parent = playMenuPanel,
				Location = new Point((playMenuPanel.Width / 2) + 150, 300),
				Size = new Size(300, 40),
				Skin = UIResources.Skins["PKButtonSkin"],
				Text = "flat",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["Zil"],
				},
				Enabled = true,
			};
			mainMenuControls.Add(level2Button);
			level2Button.MouseClick += (o, e) => LKernel.GetG<LevelManager>().LoadLevel("flat");

			level3Button = new Button("testlevel") {
				Parent = playMenuPanel,
				Location = new Point((playMenuPanel.Width / 2) + 150, 350),
				Size = new Size(300, 40),
				Skin = UIResources.Skins["PKButtonSkin"],
				Text = "testlevel",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["Zil"],
				},
				Enabled = true,
			};
			mainMenuControls.Add(level3Button);
			level3Button.MouseClick += (o, e) => LKernel.GetG<LevelManager>().LoadLevel("testlevel");

			level4Button = new Button("Sweet Apple Acres") {
				Parent = playMenuPanel,
				Location = new Point((playMenuPanel.Width / 2) + 150, 400),
				Size = new Size(300, 40),
				Skin = UIResources.Skins["PKButtonSkin"],
				Text = "sweet apple acres",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["Zil"],
				},
				Enabled = true,
			};
			mainMenuControls.Add(level4Button);
			level4Button.MouseClick += (o, e) => LKernel.GetG<LevelManager>().LoadLevel("saa");

			quitButton = new Button("Quit") {
				Parent = playMenuPanel,
				Location = new Point((playMenuPanel.Width / 2) + 150, 450),
				Size = new Size(300, 40),
				Skin = UIResources.Skins["PKButtonSkin"],
				Text = "Quit",
				TextStyle = {
					Alignment = Alignment.MiddleCenter,
					ForegroundColour = Colours.White,
					Font = UIResources.Fonts["Zil"],
				},
			};
			mainMenuControls.Add(quitButton);
			quitButton.MouseClick += (o, e) => Main.quit = true;

			foreach (Control c in mainMenuControls)
				Gui.Controls.Add(c);
		}

		/// <summary>
		/// Decides which UI to make when a level is loaded
		/// </summary>
		private void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type != LevelType.Race) {
				// when going to the menu
				foreach (Control c in mainMenuControls)
					c.Visible = true;
			}
			else if (eventArgs.OldLevel.Type != LevelType.Race) {
				// when going from the menu to something else
				foreach (Control c in levelControls)
					c.Visible = true;
			}
			else {
				foreach (Control c in levelControls)
					c.Visible = true;
			}
		}

		/// <summary>
		/// Dispose of the current UI when unloading a level
		/// </summary>
		/// <param name="eventArgs"></param>
		private void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			if (eventArgs.OldLevel.Type != LevelType.Race) {
				foreach (Control c in mainMenuControls)
					c.Visible = false;
			}
			else if (eventArgs.NewLevel.Type != LevelType.Race) {
				foreach (Control c in levelControls)
					c.Visible = false;
			}
		}

		// ======================================================

		private void CommandsButton_MouseDown(object sender, MouseButtonEventArgs e) {
			if (((Button) sender).Text == "Show Commands") {
				((Button) sender).Text = "Hide Commands";
				commandsLabel.Visible = true;
			}
			else {
				((Button) sender).Text = "Show Commands";
				commandsLabel.Visible = false;
			}
		}
	}
}