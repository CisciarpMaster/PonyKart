using System;
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
			"string type - The name of the .thing you want to spawn. Not case sensitive.",
			"Vector3 pos - Spawn position")]
		public static LThing Spawn(string type, Vector3 pos) {
			Spawner spawner = LKernel.Get<Spawner>();
			if (spawner != null) {
				try {
					return spawner.Spawn(type, pos);
				}
				catch (Exception e) {
					LKernel.GetG<LuaMain>().Print("[SpawnerWrapper] ERROR: " + e.Source + " : " + e.Message);
					return null;
				}
			}
			else
				return null;
		}

		[LuaFunction("setMaterial", "Sets all of the model components of the given LThing to use the new material.", "LThing thing", "string newMaterial")]
		public static void SetMaterial(LThing thing, string newMaterial) {
			foreach (ModelComponent mc in thing.ModelComponents) {
				mc.Entity.SetMaterialName(newMaterial);
			}
		}

		[LuaFunction("setOneMaterial", "Sets the model component with the given ID of the given LThing to use the new material.",
			"LThing thing", "int componentID", "string newMaterial")]
		public static void SetMaterial(LThing thing, int componentID, string newMaterial) {
			thing.ModelComponents[componentID].Entity.SetMaterialName(newMaterial);
		}
	}
}
