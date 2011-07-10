using System;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.TwoD.Layers;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Texture = Miyagi.Common.Resources.Texture;

namespace Lymph.UI {
	/// <summary>
	/// This class manages the speech dialogue thingies. If you were looking for the popup windows with questions, use Miyagi's DialogBox.Show.
	/// </summary>
	public class DialogueManager {
		TextureOverlay portrait;
		Label dialogue, speaker;
		Panel panel;
		Layer layer;
		public bool IsVisible { get; private set; }

		public DialogueManager() {
			Launch.Log("[Loading] Creating DialogueManager");
			IsVisible = false;
		}

		/// <summary>
		/// Creates a dialogue. If there is already a dialogue visible, this destroys that one and then creates the new one.
		/// </summary>
		/// <param name="speakerImage">The "portrait" image to use for the character who is speaking</param>
		/// <param name="speakerName">The name of the speaker</param>
		/// <param name="text">What they are saying (you can't use any markup here (yet))</param>
		public void CreateDialogue(string speakerImage, string speakerName, string text) {
			Console.WriteLine("[DialogueManager] Creating dialogue...");
			// if there already is some gui up, destroy what we've got
			if (IsVisible)
				DestroyDialogue();

			GUI gui = LKernel.Get<UIMain>().Gui;
			IsVisible = true;

			// make a panel
			panel = new Panel("DialoguePanel") {
				Padding = new Thickness(0),
				ResizeMode = ResizeModes.None,
				Size = new Size((int)Constants.WINDOW_WIDTH, 150),
				Location = new Point(0, (int)Constants.WINDOW_HEIGHT - 160),
				Skin = UIResources.Skins["PanelSkin"],
				UserData = new UIUserData {
					ObstructsViewport = true,
				},
			};
			gui.Controls.Add(panel);

			// make the portrait
			layer = new Layer();

			portrait = new TextureOverlay("DialoguePortrait") {
				Location = new Point(5, (int)Constants.WINDOW_HEIGHT - 150),
				Size = new Size(100, 100),
				Texture = new Texture(speakerImage),
			};

			layer.Elements.Add(portrait);
			LKernel.Get<UIMain>().MiyagiSys.TwoDManager.Layers.Add(layer);

			// make the name
			speaker = new Label("DialogueSpeaker") {
				Location = new Point(125, 0),
				Size = new Size(800, 50),
				TextStyle = {
					Alignment = Alignment.MiddleLeft,
					ForegroundColour = Colours.Goldenrod,
				},
				Text = speakerName,
			};
			panel.Controls.Add(speaker);

			// make the text
			dialogue = new Label("DialogueText") {
				Location = new Point(125, 40),
				Size = new Size(800, 100),
				TextStyle = {
					Alignment = Alignment.TopLeft,
					ForegroundColour = Colours.PapayaWhip,
				},
				Text = text,
			};
			panel.Controls.Add(dialogue);
		}

		/// <summary>
		/// Destroys the current dialogue. If there is no current dialogue showing, this does nothing.
		/// </summary>
		public void DestroyDialogue() {
			// only destroy the stuff if it's visible
			if (IsVisible) {
				layer.Dispose();
				panel.Dispose();
				portrait.Dispose();
				speaker.Dispose();
				dialogue.Dispose();
			}
		}
	}
}
