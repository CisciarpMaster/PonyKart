using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class RaceCountdownWrapper {

		public RaceCountdownWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("hookFunctionToCountdownEvent", "Hooks a function to one of the countdown events.", "Function(RaceCountdownState)")]
		public static void HookFunctionToCountdownEvent(RaceCountdownEvent rce) {
			RaceCountdown.OnCountdown += rce;
		}
	}
}
