using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;

namespace Ponykart.Lua {
	//[LuaPackage("Spawner", "This is a wrapper for the Spawner. Use it to spawn game objects.")]
	[LuaPackage(null, null)]
	public class SpawnerWrapper {

		public SpawnerWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("spawn", "Spawns a game object! Returns null if it didn't spawn correctly",
			"string type - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing Spawn(string type, Vector3 pos) {
			Spawner spawner = LKernel.Get<Spawner>();
			if (spawner != null) {
				try {
					return spawner.Spawn(type, pos);
				}
				catch {
					LKernel.Get<LuaMain>().Print("[SpawnerWrapper] ERROR: The specified type \"" + type + "\" was not found!");
					return null;
				}
			}
			else
				return null;
		}
	}
}
