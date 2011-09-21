using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class RaceCountdownWrapper {

		//static ICollection<KeyValuePair<RaceCountState, RaceCountEvent>> toDispose;

		public RaceCountdownWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);

			//toDispose = new Collection<KeyValuePair<RaceCountState, RaceCountEvent>>();

			//LKernel.GetG<LevelManager>().OnLevelUnload += OnLevelUnload;
		}

		/// <summary>
		/// Go through all of the functions we've hooked up to trigger regions and unhook them. We need to do this otherwise the
		/// events won't be disposed properly.
		/// </summary>
		/*void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			var countdown = LKernel.GetG<RaceCountdown>();

			foreach (KeyValuePair<RaceCountState, RaceCountEvent> pair in toDispose) {
				switch (pair.Key) {
					case RaceCountState.Three:
						countdown.OnThree -= pair.Value; break;
					case RaceCountState.Two:
						countdown.OnTwo -= pair.Value; break;
					case RaceCountState.One:
						countdown.OnOne -= pair.Value; break;
					case RaceCountState.Go:
						countdown.OnGo -= pair.Value; break;
				}
			}
		}*/

		[LuaFunction("hookFunctionToCountdownEvent", "Hooks a function to one of the countdown events.", "Function(RaceCountdownState)")]
		public static void HookFunctionToCountdownEvent(RaceCountdownEvent rce) {
			LKernel.GetG<RaceCountdown>().OnCountdown += rce;
		}
	}
}
