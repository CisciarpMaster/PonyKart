using LuaNetInterface;
using Mogre;
using Ponykart.Core;

namespace Ponykart.Lua
{
	//[LuaPackage("Spawner", "This is a wrapper for the Spawner. Use it to spawn game objects.")]
	[LuaPackage(null, null)]
	public class SpawnerWrapper {

		public SpawnerWrapper() {
			LKernel.Get<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("spawn", "Spawns a game object!", "string type - The class name of the thing you want to spawn.",
			"string name - The name of the thing you want to spawn. Don't worry if this isn't unique, an ID will be added.",
			"number posX - X spawn position", "number posY - Y spawn position", "number posZ - Z spawn position")]
		public static void Spawn(string type, string name, float posX, float posY, float posZ) {
			Spawner spawner = LKernel.Get<Spawner>();
			if (spawner != null) {
				try {
					spawner.Spawn(type, name, new Vector3(posX, posY, posZ));
				} catch {
					LKernel.Get<LuaMain>().Print("[Spawner Wrapper] ERROR: The specified type \"" + type + "\" was not found!");
				}
			}
		}
	}
}
