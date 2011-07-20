using LuaNetInterface;
using Ponykart.Core;

namespace Ponykart.Lua {
	
	//[LuaPackage("Pauser", "A wrapper for the pauser class")]
	[LuaPackage(null, null)]
	public class PauserWrapper {

		public PauserWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("pause", "Pauses the game but does not fire any events.")]
		public static void Pause() {
			Pauser.Paused = true;
		}

		[LuaFunction("unpause", "Unpauses the game but does not fire any events.")]
		public static void Unpause() {
			Pauser.Paused = false;
		}

		[LuaFunction("pauseWithEvent", "Pauses the game and fires an event.")]
		public static void PauseWithEvent() {
			if (Pauser.Paused) // don't call this if it's already paused
				return;

			Pauser p = LKernel.Get<Pauser>();
			if (p != null)
				p.InvokePauseEvent();
		}

		[LuaFunction("unpauseWithEvent", "Unauses the game and fires an event.")]
		public static void UnpauseWithEvent() {
			if (!Pauser.Paused) // don't call this if it's already unpaused
				return;

			Pauser p = LKernel.Get<Pauser>();
			if (p != null)
				p.InvokePauseEvent();
		}

		[LuaFunction("isPaused", "Returns whether the game is currently paused or not.")]
		public static bool IsPaused() {
			return Pauser.Paused;
		}

		[LuaFunction("hookScriptToPauseEvent", "Hook up a lua script so it will run whenever the pause event fires.",
			"string pathToLuaFile - the file path to the lua file you want to execute. Ex: media/scripts/example.lua")]
		public static void HookScriptToPauseEvent(string pathToLuaFile) {
			Pauser p = LKernel.Get<Pauser>();
			if (p != null)
				p.PauseEvent += (b) => LKernel.Get<LuaMain>().DoFile(pathToLuaFile);
		}

		[LuaFunction("hookFunctionToPauseEvent", "Hook up a lua function so it will run whenever the pause event fires.",
			"string nameOfLuaFunction - the name of the lua function you want to run whenever the event fires.")]
		public static void HookFunctionToPauseEvent(string nameOfLuaFunction) {
			Pauser p = LKernel.Get<Pauser>();
			if (p != null) {
				var eventInfo = p.GetType().GetEvent("PauseEvent");
				eventInfo.AddEventHandler(p, LKernel.Get<LuaMain>().LuaVM.Lua.GetFunction(eventInfo.EventHandlerType, nameOfLuaFunction));
			}
		}
	}
}
