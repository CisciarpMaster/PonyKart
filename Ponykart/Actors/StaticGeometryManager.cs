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
			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			if (eventArgs.NewLevel.Type != LevelType.EmptyLevel) {
				geom = LKernel.Get<SceneManager>().CreateStaticGeometry(eventArgs.NewLevel.Name);

				geom.RegionDimensions = new Vector3(50, 1000, 50);
				geom.RenderingDistance = 1000;
				geom.CastShadows = false;
			}
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			ents.Clear();
			if (geom != null)
				geom.Dispose();
		}

		/// <summary>
		/// Adds all of the geometry used by a model component to the static geometry.
		/// This is used by the ModelComponent.
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
			Quaternion orient = def.GetQuatProperty("orientation", Quaternion.IDENTITY) * template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 sca = def.GetVectorProperty("scale", Vector3.UNIT_SCALE);
			
			geom.AddEntity(ent, pos, orient, sca);
		}

		/// <summary>
		/// Adds all of the geometry used by an entity to the static geometry.
		/// This is used by the DotSceneLoader.
		/// </summary>
		public void Add(Entity ent, Vector3 pos, Quaternion orient, Vector3 sca) {
			// add the entity to the static geometry
			geom.AddEntity(ent, pos, orient, sca);

			if (!ents.ContainsKey(ent.GetMesh().Name))
				// if the entity dictionary doesn't contain this entity, add it
				ents.Add(ent.GetMesh().Name, ent);
			else {
				// otherwise we already have it and should get rid of it
				LKernel.GetG<SceneManager>().DestroyEntity(ent);
				ent.Dispose();
			}
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
