using LuaNetInterface;
using Ponykart.UI;

namespace Ponykart.Lua {
	//[LuaPackage("Dialogue", "This manages the dialogue for speech and stuff. If you were looking for popup windows, you'll want Miyagi's DialogBox.Show.")]
	[LuaPackage(null, null)]
	public class DialogueWrapper {

		public DialogueWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("createDialogue", "Creates a dialogue using the specified information",
			"string speakerImage - a file path to a \"portrait\" image used to show who is talking. Example: media/images/lymph.png",
			"string speakerName - the name of the speaker", "string text - what are they saying?")]
		public static void CreateDialogue(string speakerImage, string speakerName, string text) {
			var dm = LKernel.Get<DialogueManager>();
			if (dm != null) {
				dm.CreateDialogue(speakerImage, speakerName, text);
			}
		}

		[LuaFunction("destroyDialogue", "Gets rid of the current dialogue on the screen. If there isn't a dialogue up, this does nothing.")]
		public static void DestroyDialogue() {
			var dm = LKernel.Get<DialogueManager>();
			if (dm != null) {
				dm.DestroyDialogue();
			}
		}
	}
}
