using System;
using System.Collections.Generic;
using System.Linq;
using Mogre;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	public class StaticGeometryManager {
		IDictionary<string, StaticGeometry> sgeoms;
		IDictionary<string, Entity> ents;
		readonly Vector3 regionDimensions = new Vector3(100, 1000, 100);

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
		public void Add(ModelComponent mc, ThingBlock template, ModelBlock block) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			string meshName = block.GetStringProperty("mesh", null);
			string mapRegion = template.GetStringProperty("MapRegion", string.Empty);
			Entity ent;

			// get our entity if it already exists
			if (!ents.TryGetValue(meshName, out ent)) {
				// getting the entity was not successful, so we have to create it
				ent = sceneMgr.CreateEntity(meshName + mc.ID, meshName);
				ent.SetMaterialName(block.GetStringProperty("Material", string.Empty));
				ents.Add(meshName, ent);
			}

			Vector3 pos;
			// two ways to get the position
			// inherit it from the lthing, the default (if we were using nodes, this would be the default too)
			if (block.GetBoolProperty("InheritOrientation", true)) {
				pos = (mc.Owner.SpawnOrientation * block.GetVectorProperty("position", Vector3.ZERO)) + template.VectorTokens["position"];
			}
			// or we can choose not to inherit it for whatever reason
			else {
				pos = block.GetVectorProperty("position", Vector3.ZERO) + template.VectorTokens["position"];
			}
			Quaternion orient = block.GetQuatProperty("orientation", Quaternion.IDENTITY) * template.GetQuatProperty("orientation", Quaternion.IDENTITY);
			Vector3 sca = block.GetVectorProperty("scale", Vector3.UNIT_SCALE);

			StaticGeometry sg;
			if (!sgeoms.TryGetValue(mapRegion, out sg)) {
				sg = LKernel.GetG<SceneManager>().CreateStaticGeometry(mapRegion);

				sg.RegionDimensions = regionDimensions;
				sg.RenderingDistance = 300;
				sg.CastShadows = false;

				sgeoms.Add(mapRegion, sg);
			}
			
			sg.AddEntity(ent, pos, orient, sca);
		}

		/// <summary>
		/// builds the geometry. Is called after everything else has been created.
		/// </summary>
		public void Build() {
			foreach (StaticGeometry sg in sgeoms.Values) {
				System.Console.WriteLine("Static Geometry: " + sg.Name);
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

		/// <summary>
		/// Sets the visibility of all static geometry objects in the specified map region.
		/// </summary>
		/// <param name="regionName">The name of the map region. Case insensitive</param>
		/// <param name="visible">Do you want to make them visible or not?</param>
		public void SetVisibility(string regionName, bool visible) {
			var matchingGeoms = sgeoms.Where(k => k.Key.StartsWith(regionName, StringComparison.CurrentCultureIgnoreCase));
			foreach (var pair in matchingGeoms) {
				pair.Value.SetVisible(visible);
			}
		}
	}
}
