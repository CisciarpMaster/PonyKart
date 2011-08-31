using LuaNetInterface;
using Ponykart.Levels;

namespace Ponykart.Lua {
	//[LuaPackage("LevelManager", "A wrapper for the level manager class.")]
	[LuaPackage(null, null)]
	public class LevelManagerWrapper {

		public LevelManagerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("loadLevel", "Unloads the current level and loads a new one. If you know the level's ID, use loadLevel instead. "
			+"The name is case insensitive.", "string newLevelName - The name of the new level. Case insensitive.")]
		public static void LoadLevel(string newLevelName) {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm != null)
				lm.LoadLevel(newLevelName);
		}

		/// <summary>
		/// Duplicate of LevelWrapper.GetName
		/// </summary>
		[LuaFunction("getLevelName", "Gets the name of the current level. Returns \"\" if the current level is not valid.")]
		public static string GetCurrentLevelName() {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm != null) {
				if (lm.IsValidLevel)
					return LKernel.GetG<LevelManager>().CurrentLevel.Name;
				else
					return "";
			}
			return "";
		}

		[LuaFunction("hookScriptToLevelUnloadEvent", "Hook up a lua script so it will run whenever a level unloads.",
			"string pathToLuaFile - the file path to the lua file you want to execute. Ex: media/scripts/example.lua")]
		public static void HookScriptToLevelUnloadEvent(string pathToLuaFile) {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm != null)
				lm.OnLevelUnload += (ea) => LKernel.GetG<LuaMain>().DoFile(pathToLuaFile);
		}

		[LuaFunction("hookFunctionToLevelUnloadEvent", "Hook up a lua function so it will run whenever a level unloads.",
			"function() level event handler - (LevelChangedEventArgs e)")]
		public static void HookFunctionToLevelUnloadEvent(LevelEventHandler func) {
			LevelManager lm = LKernel.GetG<LevelManager>();
			if (lm != null) {
				lm.OnLevelUnload += func;
			}
		}
	}
}
