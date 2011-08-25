using MOIS;
using Ponykart.Levels;
using Ponykart.Properties;

namespace Ponykart.Handlers {
	/// <summary>
	/// this is hooked up to the keyboard events and tells the level manager to change levels when certain keys are pressed
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class LevelChangerHandler {

		public LevelChangerHandler() {
			LKernel.Get<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			// if the input is swallowed, don't do anything
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed())
				return;

			string s = "";
			switch (ke.key) {
				case KeyCode.KC_0:
					s = Settings.Default.MainMenuName; break;
				case KeyCode.KC_1:
					s = "shittyterrain"; break;
				case KeyCode.KC_2:
					s = "flat"; break;
				case KeyCode.KC_3:
					s = "testlevel"; break;
				case KeyCode.KC_4:
					s = "saa08"; break;
			}
			if (s != "")
				LKernel.Get<LevelManager>().LoadLevel(s);
		}
	}
}
