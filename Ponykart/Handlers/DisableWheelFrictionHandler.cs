using MOIS;
using Ponykart.Players;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DisableWheelFrictionHandler : ILevelHandler {
		public DisableWheelFrictionHandler() {
			LKernel.Get<InputMain>().OnKeyboardPress_Anything += Press;
			LKernel.Get<InputMain>().OnKeyboardRelease_Anything += Release;
		}

		void Release(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_G)
				LKernel.Get<PlayerManager>().MainPlayer.Kart.SetWheelFriction(5);
		}

		void Press(KeyEvent eventArgs) {
			if (eventArgs.key == KeyCode.KC_G)
				LKernel.Get<PlayerManager>().MainPlayer.Kart.SetWheelFriction(0);
		}

		public void Dispose() {
			LKernel.Get<InputMain>().OnKeyboardPress_Anything -= Press;
			LKernel.Get<InputMain>().OnKeyboardRelease_Anything -= Release;
		}
	}
}
