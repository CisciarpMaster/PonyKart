using LuaNetInterface;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Lua {
	[LuaPackage(null, null)]
	public class MiscWrapper {

		public MiscWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("loadLevel", "Unloads the current level and loads a new one. If you know the level's ID, use loadLevel instead. The name is case insensitive.",
			"string newLevelName - The name of the new level. Case insensitive.")]
		public static void LoadLevel(string newLevelName) {
			LKernel.GetG<LevelManager>().LoadLevel(newLevelName);
		}

		[LuaFunction("getLevelName", "Gets the name of the current level. Returns \"\" if the current level is not valid.")]
		public static string GetCurrentLevelName() {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm.IsValidLevel)
				return lm.CurrentLevel.Name;
			else
				return string.Empty;
		}

		/// <summary>
		/// yeah okay this uses OnLevelPreUnload instead of OnLevelUnload, but the latter wouldn't even work since lua scripts aren't ran when IsValidLevel == false
		/// </summary>
		[LuaFunction("hookFunctionToLevelUnloadEvent", "Hook up a lua function so it will run whenever a level is about to unload.",
			"function(LevelChangedEventArgs)")]
		public static void HookFunctionToLevelUnloadEvent(LevelEvent func) {
			LevelManager.OnLevelPreUnload += func;
		}

		[LuaFunction("getOption", "Get an option", "string - the name of the option you want")]
		public static string GetOption(string option) {
			return Options.Get(option);
		}

		[LuaFunction("getBoolOption", "Get an option as a bool", "string - the name of the option you want")]
		public static bool GetBoolOption(string option) {
			return Options.GetBool(option);
		}
	}
}
