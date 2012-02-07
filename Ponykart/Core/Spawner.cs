using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Core {
	public delegate void SpawnEvent<T>(T thing);

	public class Spawner {
		/// <summary>
		/// Fires whenever anything is spawned.
		/// </summary>
		public static event SpawnEvent<LThing> OnThingCreation;
		/// <summary>
		/// Fires whenever a Kart is spawned.
		/// </summary>
		public static event SpawnEvent<Kart> OnKartCreation;
		/// <summary>
		/// Fires whenever a Driver is spawned.
		/// </summary>
		public static event SpawnEvent<Driver> OnDriverCreation;

		/// <summary>
		/// Spawns something!
		/// </summary>
		/// <param name="thingName">What do you want to spawn? This is the filename of the .thing file to use, minus the extension.</param>
		/// <param name="template">The template for the thing you want to spawn</param>
		/// <returns>The thing you just spawned. Returns null if you are paused.</returns>
		public LThing Spawn(string thingName, ThingBlock template) {
			if (Pauser.IsPaused) {
				Launch.Log("[Spawner] WARNING: Attempted to spawn something while paused!");
				return null;
			}

			lock (this) {
				LThing thing;

				var definition = LKernel.GetG<ThingDatabase>().GetThingDefinition(thingName);

				if (thingName == "Kart" || thingName == "TwiCutlass") {
					thing = new Kart(template, definition);
					Invoke(OnKartCreation, thing as Kart);
				}
				else if (thingName == "Twilight") {
					thing = new Driver(template, definition);
					Invoke(OnDriverCreation, thing as Driver);
				}
				else if (thingName.StartsWith("BgPony")) {
					thing = new BackgroundPony(template, definition);
				}
				else
					thing = new LThing(template, definition);

				LKernel.Get<LevelManager>().CurrentLevel.AddThing(thing);

#if DEBUG
				//Launch.Log("[Spawner] Spawning new " + type + " with ID " + thing.ID);
#endif
				Invoke(OnThingCreation, thing);
				return thing;
			}
		}

		/// <summary>
		/// Spawns something! This takes a string instead of an enum for the type, but if the string is not a valid type,
		/// then an exception gets thrown, so be careful! Note that it is not case sensitive.
		/// </summary>
		/// <param name="type">The type (class name) for the thing you want to spawn</param>
		/// <param name="spawnPos">Where should it spawn?</param>
		/// <returns>The thing you spawned</returns>
		public LThing Spawn(string type, Vector3 spawnPos) {
			var tt = new ThingBlock(type, spawnPos);

			return Spawn(type, tt);
		}

		/// <summary>
		/// helper
		/// </summary>
		void Invoke<T>(SpawnEvent<T> evt, T actor) {
			if (evt != null)
				evt(actor);
		}
	}
}
