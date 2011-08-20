using LuaNetInterface;

namespace Ponykart.Lua {
	//[LuaPackage("Level", "Wrapper for an actual level. Some of the functions here are duplicated in LevelManager.")]
	[LuaPackage(null, null)]
	public class LevelWrapper {
		public LevelWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}
	}
}
