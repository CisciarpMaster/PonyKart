using LuaNetInterface;
using Ponykart.Levels;

namespace Ponykart.Lua {
	/// <summary>
	/// A wrapper class for input and output
	/// </summary>
	[LuaPackage(null, null)]
	public class IOWrapper {

		public IOWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("save", "Saves the current level.")]
		public static void Save() {
			var levelManager = LKernel.Get<LevelManager>();
			if (levelManager != null && levelManager.CurrentLevel != null) {
				levelManager.CurrentLevel.Save();
			}
		}
	}
}
