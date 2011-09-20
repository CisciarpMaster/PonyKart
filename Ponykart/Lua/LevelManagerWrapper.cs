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
				return "";
		}

		// does this even work, since
		[LuaFunction("hookFunctionToLevelUnloadEvent", "Hook up a lua function so it will run whenever a level unloads.",
			"function(LevelChangedEventArgs)")]
		public static void HookFunctionToLevelUnloadEvent(LevelEvent func) {
			LKernel.GetG<LevelManager>().OnLevelUnload += func;
		}
	}
}
