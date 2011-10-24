using System.Collections.Generic;
using Mogre;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	public class StaticGeometryManager {
		IDictionary<string, StaticGeometry> sgeoms;
		IDictionary<string, Entity> ents;

		public StaticGeometryManager() {
			ents = new Dictionary<string, Entity>();
			sgeoms = new Dictionary<string, StaticGeometry>();

			LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
		}

		void OnLevelUnload(LevelChangedEventArgs eventArgs) {
			ents.Clear();

			var sceneMgr = LKernel.GetG<SceneManager>();
			foreach (StaticGeometry sg in sgeoms.Values) {
				if (sg != null) {
					sceneMgr.DestroyStaticGeometry(sg);
					sg.Dispose();
				}				
			}
			sgeoms.Clear();

		}

		/// <summary>
		/// Adds all of the geometry used by a model component to the static geometry.
		/// This is used by the ModelComponent.
		/// </summary>
		/// <param name="name">The name this geometry is identified by</param>
		public void Add(ModelComponent mc, ThingBlock template, ModelBlock def) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			string meshName = def.GetStringProperty("mesh", null);
			Entity ent;

			// get our entity if it already exists
			if (!ents.TryGetValue(meshName, out ent)) {
				// getting the entity was not successful, so we have to create it
				ent = sceneMgr.CreateEntity(mc.Name + mc.ID, meshName);
				ent.SetMaterialName(def.GetStringProperty("Material", ""));
				ents.Add(meshName, ent);
			}

			Vector3 pos = def.GetVectorProperty("position", Vector3.ZERO) + template.VectorTokens["position"];
			Quaternion orient = def.GetQuatProperty("orientation", Quaternion.IDENTITY) * template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 sca = def.GetVectorProperty("scale", Vector3.UNIT_SCALE);

			StaticGeometry sg;
			if (!sgeoms.TryGetValue(meshName, out sg)) {
				sg = LKernel.GetG<SceneManager>().CreateStaticGeometry(meshName);

				sg.RegionDimensions = new Vector3(50, 1000, 50);
				//sg.RenderingDistance = 1000;
				sg.CastShadows = false;

				sgeoms.Add(meshName, sg);
			}
			
			sg.AddEntity(ent, pos, orient, sca);
		}

		/// <summary>
		/// builds the geometry. Is called after everything else has been created.
		/// </summary>
		public void Build() {
			foreach (StaticGeometry sg in sgeoms.Values) {
				sg.Build();
			}

			var sceneMgr = LKernel.Get<SceneManager>();
			foreach (Entity e in ents.Values) {
				sceneMgr.DestroyEntity(e);
				e.Dispose();
			}
		}

		public void ToggleVisible() {
			foreach (StaticGeometry sg in sgeoms.Values) {
				sg.SetVisible(!sg.IsVisible);
			}
		}
	}
}
