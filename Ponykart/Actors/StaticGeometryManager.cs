using System.Collections.Generic;
using Mogre;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	public class StaticGeometryManager {
		StaticGeometry geom;
		IDictionary<string, Entity> ents;

		public StaticGeometryManager() {
			ents = new Dictionary<string, Entity>();
			LKernel.GetG<LevelManager>().OnLevelUnload += new LevelEvent(OnLevelUnload);
			LKernel.GetG<LevelManager>().OnLevelLoad += new LevelEvent(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type != LevelType.EmptyLevel) {
				geom = LKernel.Get<SceneManager>().CreateStaticGeometry(eventArgs.NewLevel.Name);
				geom.RenderingDistance = 800;
				geom.RegionDimensions = new Vector3(200, 1000, 200);
			}
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			ents.Clear();
			if (geom != null)
				geom.Dispose();
		}

		/// <summary>
		/// Adds all of the geometry attached to a node to some static geometry
		/// </summary>
		/// <param name="name">The name this geometry is identified by</param>
		public void Add(ModelComponent mc, ThingBlock template, ModelBlock def) {
			var sceneMgr = LKernel.Get<SceneManager>();

			string meshName = def.GetStringProperty("mesh", null);
			Entity ent;

			// get our entity if it already exists
			if (!ents.TryGetValue(meshName, out ent)) {
				// getting the entity was not successful, so we have to create it
				ent = sceneMgr.CreateEntity(mc.Name + mc.ID, meshName);
				ents.Add(meshName, ent);
			}

			Vector3 pos = def.GetVectorProperty("position", Vector3.ZERO) + template.VectorTokens["position"];
			Quaternion rot = def.GetQuatProperty("orientation", Quaternion.IDENTITY) * template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 sca = def.GetVectorProperty("scale", Vector3.UNIT_SCALE);
			
			geom.AddEntity(ent, pos, rot, sca);
		}

		/// <summary>
		/// builds the geometry. Is called after everything else has been created.
		/// </summary>
		public void Build() {
			geom.Build();

			var sceneMgr = LKernel.Get<SceneManager>();
			foreach (Entity e in ents.Values) {
				sceneMgr.DestroyEntity(e);
				e.Dispose();
			}
		}
	}
}
