using LuaNetInterface;
using Ponykart.Levels;

namespace Ponykart.Lua {
	//[LuaPackage("LevelManager", "A wrapper for the level manager class.")]
	[LuaPackage(null, null)]
	public class LevelManagerWrapper {

		public LevelManagerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("loadLevel", "Unloads the current level and loads a new one. If you know the level's ID, use loadLevel instead. The name is case insensitive.",
			"string newLevelName - The name of the new level. Case insensitive.")]
		public static void LoadLevel(string newLevelName) {
			LKernel.GetG<LevelManager>().LoadLevel(newLevelName);
		}

		/// <summary>
		/// Duplicate of LevelWrapper.GetName
		/// </summary>
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
			LKernel.GetG<LevelManager>().OnLevelPreUnload += func;
		}
	}
}
