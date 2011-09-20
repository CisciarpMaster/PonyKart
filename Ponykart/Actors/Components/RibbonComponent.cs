using Mogre;
using Ponykart.Levels;
using Ponykart.Properties;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents an ogre ribbon
	/// </summary>
	public class RibbonComponent : LDisposable {
		public long ID { get; protected set; }
		public string Name { get; protected set; }
		/// <summary>
		/// The ribbon emitter
		/// </summary>
		public RibbonTrail Ribbon { get; private set; }
		/// <summary>
		/// The SceneNode that the ribbon is attached to
		/// </summary>
		public SceneNode RibbonNode { get; private set; }

		/// <summary>
		/// For ribbons!
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public RibbonComponent(LThing lthing, ThingBlock template, RibbonBlock block) {
			ID = IDs.New;
			var sceneMgr = LKernel.GetG<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			// if ribbons are disabled, don't bother creating anything
			if (!Settings.Default.EnableRibbons)
				return;

			Ribbon = LKernel.GetG<SceneManager>().CreateRibbonTrail(Name + ID + "Ribbon");

			// set up some properties
			Ribbon.SetMaterialName(block.GetStringProperty("material", "ribbon"));
			Ribbon.TrailLength = block.GetFloatProperty("length", 5);
			Ribbon.MaxChainElements = (uint) block.GetFloatProperty("elements", 10);
			Ribbon.SetInitialWidth(0, block.GetFloatProperty("width", 1));
			Ribbon.SetInitialColour(0, block.GetQuatProperty("colour", new Quaternion(1, 1, 1, 1)).ToColourValue());
			Ribbon.SetColourChange(0, block.GetQuatProperty("colourchange", new Quaternion(0, 0, 0, 3)).ToColourValue());

			// attach it to the node
			RibbonNode = LKernel.GetG<SceneManager>().RootSceneNode.CreateChildSceneNode(Name + ID + "RibbonNode");
			Ribbon.AddNode(lthing.RootNode);
			RibbonNode.AttachObject(Ribbon);

			RibbonNode.Position = block.GetVectorProperty("position", null);
		}

		public override string ToString() {
			return Name + ID + "Ribbon";
		}

		/// <summary>
		/// clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (Settings.Default.EnableRibbons && Ribbon != null && RibbonNode != null) {
				if (disposing) {
					RibbonNode.DetachObject(Ribbon);
					foreach (SceneNode n in Ribbon.GetNodeIterator())
						Ribbon.RemoveNode(n);
					if (valid)
						sceneMgr.DestroyRibbonTrail(Ribbon);
					if (valid)
						sceneMgr.DestroySceneNode(RibbonNode);
				}
				Ribbon.Dispose();
				Ribbon = null;
				RibbonNode.Dispose();
				RibbonNode = null;
			}

			base.Dispose(disposing);
		}
	}
}
