using System;
using Mogre;
using Ponykart.Actors;
using Ponykart.IO;

namespace Ponykart.Core {
	public delegate void OnThingCreation<T>(T thing);

	public class Spawner {
		/// <summary>
		/// Fires whenever anything is spawned.
		/// </summary>
		public event OnThingCreation<LThing> OnThingCreation;
		/// <summary>
		/// Fires whenever a Kart is spawned.
		/// </summary>
		public event OnThingCreation<Kart> OnKartCreation;

		/// <summary>
		/// Spawns something!
		/// </summary>
		/// <param name="type">What do you want to spawn?</param>
		/// <param name="template">The template for the thing you want to spawn</param>
		/// <exception cref="ArgumentException">If 'type' is not a valid ActorEnum</exception>
		/// <returns>The thing you just spawned. Returns null if you are paused.</returns>
		public LThing Spawn(string type, ThingInstanceTemplate template) {
			if (Pauser.IsPaused) {
				Launch.Log("[Spawner] WARNING: Attempted to spawn something while paused!");
				return null;
			}
			LThing thing;

			var definition = LKernel.Get<ThingDatabase>().GetThingDefinition(type);

			if (type == "Kart") {
				thing = new Kart(template, definition);
				Invoke(OnKartCreation, thing as Kart);
			}
			else 
				thing = new LThing(template, definition);

			Invoke(OnThingCreation, thing);


			return thing;
		}

		/// <summary>
		/// Spawns something! Use this internally or if you don't have a template. This method will make one for you!
		/// </summary>
		/// <param name="type">The type (class) of the thing you want to spawn</param>
		/// <param name="spawnPos">Where should it spawn?</param>
		/// <returns>The thing you spawned</returns>
		public LThing Spawn(string type, Vector3 spawnPos) {
			ThingInstanceTemplate tt = new ThingInstanceTemplate(type, spawnPos);
			return Spawn(type, tt);
		}

		/// <summary>
		/// Spawns something! This takes a string instead of an enum for the type, but if the string is not a valid type,
		/// then an exception gets thrown, so be careful! Note that it is not case sensitive.
		/// </summary>
		/// <param name="type">The type (class name) for the thing you want to spawn</param>
		/// <param name="name">What is its name? (Don't include the ID!)</param>
		/// <param name="spawnPos">Where should it spawn?</param>
		/// <exception cref="ArgumentException">If the type is not valid</exception>
		/// <returns>The thing you spawned</returns>
		public LThing Spawn(string type, string name, Vector3 spawnPos) {
			var td = LKernel.Get<ThingDatabase>().GetThingDefinition(type);
			var tt = new ThingInstanceTemplate(type, name, spawnPos);

			LThing lt = new LThing(tt, td);
			return lt;
			//return Spawn(tt);
		}

		/// <summary>
		/// helper
		/// </summary>
		void Invoke<T>(OnThingCreation<T> evt, T actor) {
			if (evt != null)
				evt(actor);
		}
	}
}
