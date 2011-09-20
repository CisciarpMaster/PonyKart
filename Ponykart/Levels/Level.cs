using System;
using System.Collections.Generic;
using System.IO;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Levels {
	/// <summary>
	/// Represents a level or world in our game.
	/// </summary>
	public class Level : LDisposable {
		/// <summary>
		/// The world's name - this serves as its identifier
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The type of this level
		/// </summary>
		public LevelType Type { get; private set; }

		public MuffinDefinition Definition { get; private set; }
		/// <summary>
		/// We use the thing's Name as the key
		/// </summary>
		public IDictionary<string, LThing> Things { get; private set; }

		/// <summary>
		/// Constructor - Initialises the dictionaries and hooks up to the spawn event
		/// </summary>
		/// <param name="name">The name of the level - this is case sensitive!</param>
		public Level(string name) {
			Name = name;
			Things = new Dictionary<string, LThing>();
			if (string.IsNullOrEmpty(name))
				Type = LevelType.EmptyLevel;

			// don't use anonymous methods here because we have to disconnect it when we change levels
			LKernel.GetG<Spawner>().OnThingCreation += OnSpawnEvent;
		}

		/// <summary>
		/// Reads the main .muffin file for this level and loads any extra ones that were "linked" from the main one
		/// </summary>
		public void ReadMuffin() {
			Definition = new MuffinImporter().Parse(Name);
			foreach (string file in Definition.ExtraFiles) {
				Definition = new MuffinImporter().Parse(file, Definition);
			}

			// get the type of the level
			ThingEnum tempType = Definition.GetEnumProperty("type", null);
			LevelType type;
			Enum.TryParse<LevelType>(tempType + string.Empty, true, out type);
			Type = type;
		}

		/// <summary>
		/// Parses a .scene file and tells PhysX about it
		/// </summary>
		public void CreateEnvironment() {
			LKernel.GetG<PhysicsMain>().LoadPhysicsLevel(Name);
		}

		/// <summary>
		/// Parses the level's save file and creates all of the Things that will be in it.
		/// Also creates the player.
		/// </summary>
		public void CreateEntities() {
			// load up everything into this world

			var spawner = LKernel.GetG<Spawner>();
			foreach (ThingBlock tb in Definition.ThingBlocks) {
				spawner.Spawn(tb.ThingName, tb);
			}
		}

		/// <summary>
		/// Runs all of the scripts in a level's init directory.
		/// If this documentation is up to date, it should look like:
		/// "media/scripts/" + Name + "/init/"
		/// </summary>
		public void RunLevelScripts() {
			if (Directory.Exists(Settings.Default.LevelScriptLocation + Name + "/"))
				LKernel.GetG<LuaMain>().LuaVM.Lua.GetFunction(Name).Call(this);

			LThing[] readonlythings = new LThing[Things.Values.Count];
			Things.Values.CopyTo(readonlythings, 0);
			foreach (LThing l in readonlythings)
				l.RunScript();
		}

		//public void Save() {
		//}

		/// <summary>
		/// Runs whenever we spawn something. This just adds it to the level's dictionary of Things.
		/// </summary>
		/// <param name="newThing"></param>
		void OnSpawnEvent(LThing newThing) {
			if (Things.ContainsKey(newThing.Name))
				Things[newThing.Name + newThing.ID] = newThing;
			else
				Things[newThing.Name] = newThing;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				LKernel.GetG<Spawner>().OnThingCreation -= OnSpawnEvent;
				if (Things != null) {
					foreach (LThing t in Things.Values)
						t.Dispose();
					Things.Clear();
				}
				LKernel.GetG<ThingDatabase>().ClearDatabase();

				if (Definition != null)
					Definition.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
