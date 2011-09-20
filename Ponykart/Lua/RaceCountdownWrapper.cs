using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class RaceCountdownWrapper {

		//static ICollection<KeyValuePair<RaceCountState, RaceCountEvent>> toDispose;

		/*enum RaceCountState {
			Three,
			Two,
			One,
			Go
		}*/

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

		[LuaFunction("hookFunctionToCountdownEvent", "Hooks a function to one of the countdown events.",
			"int count - The countdown number to hook this event to. Use 3 for Three, 2 for Two, 1 for One, and 0 for Go. Anything else will be ignored.",
			"Function() rce - The function to run. Must take 0 parameters.")]
		public static void HookFunctionToCountdownEvent(int count, RaceCountEvent rce) {
			var countdown = LKernel.GetG<RaceCountdown>();

			switch (count) {
				case 3:
					countdown.OnThree += rce; break;
				case 2:
					countdown.OnTwo += rce; break;
				case 1:
					countdown.OnOne += rce; break;
				case 0:
					countdown.OnGo += rce; break;
				default:
					LKernel.GetG<LuaMain>().Print("Invalid countdown number! It must be between 0 and 3 inclusive!");
					break;
			}
		}
	}
}
