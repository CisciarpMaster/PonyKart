using LuaNetInterface;
using Ponykart.Levels;

namespace Ponykart.Lua {
	[LuaPackage("Level", "Wrapper for an actual level. Some of the functions here are duplicated in LevelManager.")]
	public class LevelWrapper {
		public LevelWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("getFlag", "Gets a boolean flag from the level. If the flag is not found then this will return false and you'll get "+
			"an error message.", "string flagName - The name of the flag. It's case sensitive!")]
		public static bool GetFlag(string flagName) {
			var lm = LKernel.Get<LevelManager>();
			if (lm != null && lm.IsValidLevel) {
				if (lm.CurrentLevel.Flags.ContainsKey(flagName))
					return lm.CurrentLevel.Flags[flagName];
				else
					LKernel.Get<LuaMain>().Print("[Level Wrapper] ERROR: A flag with that name was not found!");
			}
			return false;
		}

		[LuaFunction("getNumber", "Gets a number from the level. If the number is not found then this will return -1 and you'll get " +
			"an error message.", "string numberName - The name of the number. It's case sensitive!")]
		public static float GetNumber(string numberName) {
			var lm = LKernel.Get<LevelManager>();
			if (lm != null && lm.IsValidLevel) {
				if (lm.CurrentLevel.Numbers.ContainsKey(numberName))
					return lm.CurrentLevel.Numbers[numberName];
				else
					LKernel.Get<LuaMain>().Print("[Level Wrapper] ERROR: A number with that name was not found!");
			}
			return -1;
		}

		/// <summary>
		/// Duplicate of LevelManagerWrapper.GetCurrentLevelName
		/// </summary>
		[LuaFunction("getName", "Gets the name of the current level. Returns \"\" if the current level is not valid.")]
		public static string GetName() {
			LevelManager lm = LKernel.Get<LevelManager>();
			if (lm != null) {
				if (lm.IsValidLevel)
					return LKernel.Get<LevelManager>().CurrentLevel.Name;
				else
					return "";
			}
			return "";
		}

		/// <summary>
		/// Duplicate of LevelManagerWrapper.IsValid
		/// </summary>
		[LuaFunction("isValid", "Returns whether the current level is valid or not.")]
		public static bool IsValid() {
			LevelManager lm = LKernel.Get<LevelManager>();
			if (lm != null)
				return lm.IsValidLevel;
			else
				return false;
		}
	}
}
