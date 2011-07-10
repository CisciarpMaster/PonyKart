using System;
using System.Collections.Generic;
using System.IO;
using Lymph.Actors;
using Lymph.Core;
using Lymph.IO;
using Lymph.Lua;
using Lymph.Phys;
using Lymph.Stuff;
using Mogre;

namespace Lymph.Levels
{
	/// <summary>
	/// Represents a level or world in our game.
	/// </summary>
	public class Level : IDisposable
	{
		/// <summary>
		/// The world's name - this serves as its identifier
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Our level's boolean flags
		/// </summary>
		public IDictionary<string, bool> Flags { get; private set; }
		/// <summary>
		/// Our level's numbers
		/// </summary>
		public IDictionary<string, float> Numbers { get; private set; }
		/// <summary>
		/// We use the template's Name as the key
		/// </summary>
		public IDictionary<string, ThingTemplate> Templates { get; private set; }
		/// <summary>
		/// We use the thing's Name as the key
		/// </summary>
		public IDictionary<string, Thing> Things { get; private set; }

		/// <summary>
		/// Constructor - Initialises the dictionaries and hooks up to the spawn event
		/// </summary>
		/// <param name="name">The name of the level - this is case sensitive!</param>
		public Level(string name)
		{
			Name		= name;
			Flags		= new Dictionary<string, bool>();
			Numbers		= new Dictionary<string, float>();
			Templates	= new Dictionary<string, ThingTemplate>();
			Things		= new Dictionary<string, Thing>();

			// don't use anonymous methods here because we have to disconnect it when we change levels
			LKernel.Get<Spawner>().OnThingCreation += OnSpawnEvent;
		}

		/// <summary>
		/// Parses a .scene file and tells PhysX about it
		/// </summary>
		public void CreateEnvironment() {
			var levelNode = LKernel.Get<SceneManager>().RootSceneNode.CreateChildSceneNode("RootLevelNode", new Vector3(0, 0, 0));

			LKernel.Get<DotSceneLoader>().ParseDotScene(Name + ".scene", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, levelNode);

			LKernel.Get<PhysXMain>().LoadPhysicsLevel(Name);
		}

		/// <summary>
		/// Parses the level's save file and creates all of the Things that will be in it.
		/// Also creates the player.
		/// </summary>
		public void CreateEntities() {
			// load up everything into this world
			new WorldImporter().Parse(this);
			// load up the player
			ThingTemplate playerTemplate = new PlayerImporter().Parse();
			Templates[playerTemplate.StringTokens["Name"]] = playerTemplate;

			var spawner = LKernel.Get<Spawner>();
			foreach (ThingTemplate tt in Templates.Values) {
				spawner.Spawn(tt);
			}
		}

		/// <summary>
		/// Runs all of the scripts in a level's init directory.
		/// If this documentation is up to date, it should look like:
		/// "media/scripts/" + Name + "/init/"
		/// </summary>
		public void RunLevelScripts() {
			string dir = Settings.Default.LevelScriptLocation + Name + Settings.Default.LevelScriptFolderExtension;
			if (Directory.Exists(dir)) {
				IEnumerable<string> scripts = Directory.EnumerateFiles(dir);

				foreach (string script in scripts)
					LKernel.Get<LuaMain>().DoFile(script);
			}
		}

		public void Save() {
			new WorldExporter().Export(this);
		}

		/// <summary>
		/// Runs whenever we spawn something. This just adds it to the level's dictionary of Things.
		/// </summary>
		/// <param name="newThing"></param>
		void OnSpawnEvent(Thing newThing) {
			if (Things.ContainsKey(newThing.Name))
				Things[newThing.Name + newThing.ID] = newThing;
			else
				Things[newThing.Name] = newThing;
		}

		public void Dispose() {
			LKernel.Get<Spawner>().OnThingCreation -= OnSpawnEvent;
			Name = null;
			Flags.Clear();
			Numbers.Clear();
			Templates.Clear();
			Things.Clear();
		}
	}
}
