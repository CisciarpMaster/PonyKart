using LymphThing;
using Mogre;
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
		public ModelComponent(LThing lthing, ThingBlock template, ModelBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.Get<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			Node = lthing.RootNode.CreateChildSceneNode(Name + "Node" + ID);

			// position
			Node.Position = block.GetVectorProperty("position", Vector3.ZERO);
			// orientation
			Node.Orientation = block.GetQuatProperty("orientation", Quaternion.IDENTITY);
			// if orientation was not found, we fall back to rotation
			if (Node.Orientation == Quaternion.IDENTITY) {
				Vector3 rot = block.GetVectorProperty("rotation", Vector3.ZERO);
				if (rot != Vector3.ZERO)
					Node.Orientation = rot.DegreeVectorToGlobalQuaternion();
			}
			// scale
			Node.Scale(block.GetVectorProperty("scale", Vector3.UNIT_SCALE));

			// make our entity
			Entity = sceneMgr.CreateEntity(Name + "Entity" + ID, block.GetStringProperty("mesh", null));

			// material name
			string materialName = block.GetStringProperty("material", "");
			if (materialName != "")
				Entity.SetMaterialName(materialName);

			// some other properties
			Entity.CastShadows = block.GetBoolProperty("castsshadows", true);
			Entity.Visible = block.GetBoolProperty("visible", true);
			Entity.RenderingDistance = block.GetFloatProperty("renderingdistance", 0);

			// then attach it to the node!
			Node.AttachObject(Entity);

			Node.InheritScale = true;
			Node.InheritOrientation = true;

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
