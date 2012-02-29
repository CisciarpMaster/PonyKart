using LuaNetInterface;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;

namespace Ponykart.Lua {
	//[LuaPackage("Spawner", "This is a wrapper for the Spawner. Use it to spawn game objects.")]
	[LuaPackage(null, null)]
	public class SpawnerWrapper {

		public SpawnerWrapper() {
			LKernel.GetG<LuaMain>().RegisterLuaFunctions(this);
		}

		[LuaFunction("spawn", "Spawns a game object! Returns null if it didn't spawn correctly",
			"string thingName - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing Spawn(string thingName, Vector3 pos) {
			if (thingName.StartsWith("BgPony") || thingName == "LyraSitting")
				return LKernel.Get<Spawner>().SpawnBgPony(thingName, pos);
			else
				return LKernel.Get<Spawner>().Spawn(thingName, pos);
		}

		[LuaFunction("relativeSpawn", "Spawns a game object relative to another game object. This does not return anything.", 
			"string thingName - the name of the .thing you want to spawn. Not case sensitive.",
			"LThing thing - the 'parent' game object",
			"Vector3 offset")]
		public static void RelativeSpawn(string thingName, LThing thing, Vector3 offset) {
			Vector3 pos = thing.RootNode.ConvertLocalToWorldPosition(offset);
			Spawn(thingName, pos);
		}

		[LuaFunction("spawnBgPony", "Spawns a background pony! Returns null if it didn't spawn correctly",
			"string thingName - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing SpawnBgPony(string thingName, Vector3 pos) {
			return LKernel.Get<Spawner>().SpawnBgPony(thingName, pos);
		}
	}
}
