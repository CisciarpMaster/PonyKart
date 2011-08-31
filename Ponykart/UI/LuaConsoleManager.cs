using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Events;
using Miyagi.UI;
using Miyagi.UI.Controls;
using MOIS;
using Ponykart.Lua;
using KeyEvent = MOIS.KeyEvent;

namespace Ponykart.UI {
	/// <summary>
	/// This class manages our lua console, since now we can type things into lua and make it do stuff, all without changing windows! :D
	/// Note that this console only outputs stuff from lua - it doesn't output anything else from the program.
	/// Check the log files if you want to look at *all* output.
	/// </summary>
	public class LuaConsoleManager {
		private int labelY;
		private Panel panel;
		private TextBox textBox;
		public bool IsVisible { get; private set; }
		private string lastInput = "";

		public LuaConsoleManager() {
			Launch.Log("[Loading] Creating LuaConsoleManager");
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress;
			IsVisible = false;
			Create();

			// swallow the input if our text box has focus
			LKernel.GetG<InputSwallowerManager>().AddSwallower(() => textBox.Focused, this);
		}

		/// <summary>
		/// Runs whenever we press the key to toggle whether to show or hide the console.
		/// </summary>
		void OnKeyboardPress(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_RETURN && !IsVisible) {
				Show();
			} else if (eventArgs.key == KeyCode.KC_UP && IsVisible) {
				textBox.Text = lastInput;
			}
		}

		/// <summary>
		/// Hide the console!
		/// </summary>
		public void Hide() {
			IsVisible = false;
			panel.Visible = IsVisible;
			textBox.Visible = IsVisible;
			textBox.Text = "";
		}

		/// <summary>
		/// Show the console! Also this should automatically focus on the console too, but for some reason it's not doing that.
		/// </summary>
		public void Show() {
			IsVisible = true;
			panel.Visible = IsVisible;
			textBox.Visible = IsVisible;
			textBox.Text = "";

			textBox.Focused = true;
		}

		/// <summary>
		/// I don't really need to have this in a separate method, but eh it doesn't matter
		/// </summary>
		void Create() {
			GUI gui = LKernel.GetG<UIMain>().Gui;

			// make the panel
			panel = new Panel("ConsolePanel") {
				TabStop = false,
				TabIndex = 0,
				Throwable = true,
				Size = new Size(768, 300),
				Location = new Point(0, 0),
				MinSize = new Size(0, 0),
				ResizeThreshold = new Thickness(0),
				BorderStyle = {
					Thickness = new Thickness(2, 2, 2, 2)
				},
				HScrollBarStyle = {
					Extent = 16,
					ThumbStyle = {
						BorderStyle = {
							Thickness = new Thickness(2, 2, 2, 2)
						}
					}
				},
				VScrollBarStyle = {
					Extent = 16,
					ThumbStyle = {
						BorderStyle = {
							Thickness = new Thickness(2, 2, 2, 2)
						}
					}
				},
				Skin = UIResources.Skins["PanelSkin"],
				UserData = new UIUserData {
					ObstructsViewport = true,
				},
				Visible = false,
				AlwaysOnTop = true,
				ResizeMode = ResizeModes.None,
			};

			// make the text box
			textBox = new TextBox("ConsoleTextBox") {
				Size = new Size(768, 32),
				Location = new Point(0, 300),
				Padding = new Thickness(9, 0, 8, 0),
				TextStyle = {
					Alignment = Alignment.MiddleLeft,
				},
				TextBoxStyle = {
					CaretStyle = {
						Size = new Size(2, 16),
						Colour = Colours.Black
					}
				},
				Skin = UIResources.Skins["TextBoxSkin"],
				UserData = new UIUserData {
					ObstructsViewport = true,
				},
				Visible = false,
				AlwaysOnTop = true,
			};

			// eeeeeeeeveeeeeeeeeents
			textBox.Submit += TextBoxSubmit;

			gui.Controls.Add(panel);
			gui.Controls.Add(textBox);
		}

		/// <summary>
		/// Adds a line of text to the console
		/// </summary>
		/// <param name="text">The text to add.</param>
		public void AddLabel(string text) {
			var label = new Label {
				Location = new Point(0, labelY),
				Text = text,
				AutoSize = true,
				MaxSize = new Size(panel.Size.Width, 300),
				TextStyle = {
					ForegroundColour = Colours.White,
					WordWrap = true,
					Multiline = true,
				}
			};
			panel.Controls.Add(label);
			labelY += label.Size.Height;
			// apparently this doesn't exist any more?
			// maybe this is what's causing it to crash when we add a scrollbar
			panel.ScrollToBottom();

			// only focus the text box if it's visible, otherwise it swallows input for no reason
			if (IsVisible)
				textBox.Focused = true;
		}

		/// <summary>
		/// Runs whenever we press enter after typing something into the text box.
		/// A &gt; is put in front of any lines here, to distinquish between user input and lua output.
		/// Something similar is also done for the ogre log.
		/// </summary>
		private void TextBoxSubmit(object sender, ValueEventArgs<string> vea) {
			// ignore if nothing is entered
			if (vea.Data == "")
				return;

			lastInput = vea.Data;
			AddLabel("> " + vea.Data);
			Launch.Log("[Lua] <Input> " + vea.Data);
			LKernel.GetG<LuaMain>().DoString("print(" + vea.Data + ")");

			textBox.Text = "";

			// yet it still loses focus?
			textBox.Focused = true;
		}
	}
}
