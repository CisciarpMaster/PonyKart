using Mogre;
using Ponykart.IO;
using Ponykart.Levels;

namespace Ponykart.Actors {
	public class ModelComponent : IThingComponent {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public int ID { get; protected set; }
		public string Name { get; protected set; }

		/// <summary>
		/// Creates a model component for a Thing.
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public ModelComponent(LThing lthing, ThingInstanceTemplate template, ModelBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.Get<SceneManager>();

			string name;
			if (block.StringTokens.TryGetValue("name", out name))
				Name = name;
			else
				Name = template.Name;

			Node = lthing.RootNode.CreateChildSceneNode(Name + "Node" + ID);

			// position
			Vector3 pos;
			if (block.VectorTokens.TryGetValue("position", out pos))
				Node.Position = pos;

			// rotation (in degrees)
			Vector3 rot;
			if (block.VectorTokens.TryGetValue("rotation", out rot))
				Node.Orientation = rot.DegreeVectorToGlobalQuaternion();

			// scale
			Vector3 sca;
			if (block.VectorTokens.TryGetValue("scale", out sca))
				Node.SetScale(sca);

			// make our entity
			Entity = sceneMgr.CreateEntity(Name + "Entity" + ID, block.StringTokens["mesh"]);

			// material name
			string materialName;
			if (block.StringTokens.TryGetValue("material", out materialName))
				Entity.SetMaterialName(materialName);

			// then attach it to the node!
			Node.AttachObject(Entity);

			CreateMore();
		}

		protected virtual void CreateMore() {

		}

		public void Dispose() {
			var sceneMgr = LKernel.Get<SceneManager>();
			bool valid = LKernel.Get<LevelManager>().IsValidLevel;

			if (Entity != null) {
				if (valid)
					sceneMgr.DestroyEntity(Entity);
				Entity.Dispose();
				Entity = null;
			}
			if (Node != null) {
				if (valid)
					sceneMgr.DestroySceneNode(Node);
				Node.Dispose();
			}
		}

		public override string ToString() {
			return Node.Name;
		}
	}
}
