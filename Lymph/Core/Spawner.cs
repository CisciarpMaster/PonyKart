using System;
using Mogre;
using Ponykart.Actors;

namespace Ponykart.Core {
	public delegate void OnThingCreation<T>(T thing);

	public class Spawner {
		/// <summary>
		/// Fires whenever anything is spawned.
		/// </summary>
		public event OnThingCreation<Thing> OnThingCreation;
		/// <summary>
		/// Fires whenever a Kart is spawned.
		/// </summary>
		public event OnThingCreation<Kart> OnKartCreation;
		/// <summary>
		/// Fires whenever an Antibody is spawned.
		/// </summary>
		public event OnThingCreation<Antibody> OnAntibodyCreation;

		/// <summary>
		/// Spawns something!
		/// </summary>
		/// <param name="type">What do you want to spawn?</param>
		/// <param name="template">The template for the thing you want to spawn</param>
		/// <exception cref="ArgumentException">If 'type' is not a valid ActorEnum</exception>
		/// <returns>The thing you just spawned. Returns null if you are paused.</returns>
		public Thing Spawn(ThingEnum type, ThingTemplate template) {
			if (Pauser.IsPaused) {
				Launch.Log("[Spawner] WARNING: Attempted to spawn something while paused!");
				return null;
			}
			Thing actor;

			switch (type) {
				case ThingEnum.Antibody:
					actor = new Antibody(template);
					Invoke<Antibody>(OnAntibodyCreation, actor as Antibody);
					break;
				case ThingEnum.Kart:
					actor = new Kart(template);
					Invoke(OnKartCreation, actor as Kart);
					break;
				case ThingEnum.Obstacle:
					actor = new Obstacle(template);
					break;
				case ThingEnum.ZergShip:
					actor = new ZergShip(template);
					break;
				default:
					throw new ArgumentException("Unknown ActorEnum: " + type, type.ToString());
			}
			Invoke(OnThingCreation, actor);

			return actor;
		}

		/// <summary>
		/// Spawns something! Use this internally or if you don't have a template. This method will make one for you!
		/// </summary>
		/// <param name="type">The type (class) of the thing you want to spawn</param>
		/// <param name="name">What is its name? (Don't include the ID)</param>
		/// <param name="spawnPos">Where should it spawn?</param>
		/// <returns>The thing you spawned</returns>
		public Thing Spawn(ThingEnum type, string name, Vector3 spawnPos) {
			ThingTemplate tt = new ThingTemplate(type.ToString(), name, spawnPos);
			return Spawn(type, tt);
		}

		/// <summary>
		/// Spawns something! Instead of passing this a type via ThingEnum, this uses the string type that's in the
		/// template. But if that type isn't valid, then an exception gets thrown, so be careful! Note that it is
		/// not case sensitive.
		/// </summary>
		/// <param name="template">The template for the thing you want to spawn</param>
		/// <exception cref="ArgumentException">If the template's Type is not valid</exception>
		/// <returns>The thing you spawned</returns>
		public Thing Spawn(ThingTemplate template) {
			ThingEnum ae;
			if (Enum.TryParse<ThingEnum>(template.Type, true, out ae))
				return Spawn(ae, template);
			else
				throw new ArgumentException("The template's Type (" + template.Type + ") is not a valid Actor type!", "template.Type");
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
		public Thing Spawn(string type, string name, Vector3 spawnPos) {
			ThingTemplate tt = new ThingTemplate(type, name, spawnPos);
			return Spawn(tt);
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
