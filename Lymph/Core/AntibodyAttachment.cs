using System;
using Lymph.Actors;
using Lymph.Stuff;
using Mogre;

namespace Lymph.Core {
	/// <summary>
	/// Antibody attachment. Does not inherit anything from Thing or Antibody. Physics engine independent.
	/// </summary>
	public class AntibodyAttachment : IDisposable {
		public Enemy AttachedEnemy { get; set; }
		private int ID;
		private SceneNode parent;
		public long TimeCreated { get; set; }
		public SceneNode Node { get; set; }
		public RibbonTrail Ribbon { get; set; }
		public AntigenColour Colour { get; set; }
		public SceneNode RibbonNode { get; set; }
		private Entity entity;

		/// <summary>
		/// Constructor for an AntibodyAttachment. These are the antibodies you see that stick to cells. They are created when an Antibody collides with
		/// a Cell (after certain conditions are met)
		/// </summary>
		/// <param name="nodeName">The name of the SceneNode to use</param>
		/// <param name="entityName">The name of the Entity to use</param>
		/// <param name="colour">The color of the original antibody</param>
		/// <param name="parent">The SceneNode of the cell we are attaching this to</param>
		/// <param name="original">The SceneNode of the original antibody</param>
		/// <param name="timeCreated">The time that the original antibody was created</param>
		/// <param name="position">The AntibodyAttachment's "position" relative to the cell</param>
		public AntibodyAttachment(string name, AntigenColour colour, SceneNode parent, SceneNode original, long timeCreated, Vector3 position)
		{
			this.ID = IDs.New;
			this.parent = parent;
			this.TimeCreated = timeCreated;
			this.Colour = colour;

			string antibodyName = name + ID;

			Node = parent.CreateChildSceneNode(antibodyName, position);
			entity = LKernel.Get<SceneManager>().CreateEntity(antibodyName, "antibody.mesh");
			Node.AttachObject(entity);
			Node.InheritScale = false;
			
			Launch.Log("Creating antibody attachment #" + ID);
			// extra entities for effects go here
			if (Constants.RIBBONS) {
				string ribbonName = name + "Ribbon" + ID;

				this.Ribbon = LKernel.Get<SceneManager>().CreateRibbonTrail(ribbonName);
				Ribbon.SetMaterialName("ribbon");
				Ribbon.TrailLength = 4;
				Ribbon.MaxChainElements = 5;
				Ribbon.SetInitialColour(0, colour.Colour);
				Ribbon.SetColourChange(0, new ColourValue(0, 0, 0, 3));
				Ribbon.SetInitialWidth(0, 0.2f);
				Ribbon.RenderQueueGroup = (byte)RQGL.RIBBONS;
				// attach it to the node
				RibbonNode = LKernel.Get<SceneManager>().RootSceneNode.CreateChildSceneNode(ribbonName);
				Ribbon.AddNode(Node);
				RibbonNode.AttachObject(Ribbon);
			}

			Node.Orientation = original.Orientation;
			// align the antibody so the bottom of the "Y" points towards the cell
			Node.LookAt(parent.Position, Mogre.Node.TransformSpace.TS_WORLD);
			Node.Yaw(new Radian(1.57f), Mogre.Node.TransformSpace.TS_LOCAL);
		}

		#region IDisposable stuff

		public void Dispose() {
			// destroy ribbon
			// need to detach twice?
			RibbonNode.DetachObject(Ribbon);
			foreach (SceneNode n in Ribbon.GetNodeIterator())
				Ribbon.RemoveNode(n);
			//Ribbon.DetatchFromParent();
			SceneManager sceneMgr = LKernel.Get<SceneManager>();
			sceneMgr.DestroyRibbonTrail(Ribbon);
			Ribbon.Dispose();
			sceneMgr.DestroySceneNode(RibbonNode);

			// destroy entity
			sceneMgr.DestroyEntity(entity);

			// destroy node
			sceneMgr.DestroySceneNode(Node);
		}
		#endregion
	}
}
