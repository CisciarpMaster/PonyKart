using Mogre;
using Ponykart.IO;
using Ponykart.Levels;

namespace Ponykart.Actors {
	public class RibbonComponent : IThingComponent {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		/// <summary>
		/// The ribbon emitter
		/// </summary>
		public RibbonTrail Ribbon { get; private set; }
		/// <summary>
		/// The SceneNode that the ribbon is attached to
		/// </summary>
		public SceneNode RibbonNode { get; private set; }

		public RibbonComponent(LThing lthing, ThingInstanceTemplate template, RibbonBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.Get<SceneManager>();

			Name = block.GetStringProperty("name", template.Name);

			Ribbon = LKernel.Get<SceneManager>().CreateRibbonTrail(Name + ID + "Ribbon");


			Ribbon.SetMaterialName(block.GetStringProperty("material", "ribbon"));
			Ribbon.TrailLength = block.GetFloatProperty("length", 5);
			Ribbon.MaxChainElements = (uint) block.GetFloatProperty("elements", 10);
			Ribbon.SetInitialWidth(0, block.GetFloatProperty("width", 1));

			// have to convert a quat to a colour
			Quaternion colorQuat = block.GetQuatProperty("colour", new Quaternion(1, 1, 1, 1));
			Ribbon.SetInitialColour(0, new ColourValue(colorQuat.x, colorQuat.y, colorQuat.z, colorQuat.w));

			colorQuat = block.GetQuatProperty("colourchange", new Quaternion(0, 0, 0, 3));
			Ribbon.SetColourChange(0, new ColourValue(colorQuat.x, colorQuat.y, colorQuat.z, colorQuat.w));

			// attach it to the node
			RibbonNode = LKernel.Get<SceneManager>().RootSceneNode.CreateChildSceneNode(Name + ID + "RibbonNode");
			Ribbon.AddNode(lthing.RootNode);
			RibbonNode.AttachObject(Ribbon);

			RibbonNode.Position = block.GetVectorProperty("position", Vector3.ZERO);
		}

		public void Dispose() {
			if (Ribbon != null && RibbonNode != null) {
				RibbonNode.DetachObject(Ribbon);
				foreach (SceneNode n in Ribbon.GetNodeIterator())
					Ribbon.RemoveNode(n);
				if (LKernel.Get<LevelManager>().IsValidLevel)
					LKernel.Get<SceneManager>().DestroyRibbonTrail(Ribbon);
				Ribbon.Dispose();
				Ribbon = null;
				RibbonNode = null;
			}
		}
	}
}
