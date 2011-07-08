using System;
using System.Collections.ObjectModel;
using Lymph.Handlers;
using Lymph.Phys;
using Lymph.Stuff;
using Mogre;

namespace Lymph.Actors {
	/// <summary>
	/// This class represents the player! Yay!
	/// </summary>
	public class Player : ControllerThing {
		/// <summary>
		/// The current antibodies we will shoot from our left button
		/// </summary>
		public AntigenColour AntibodyColourLeft { set; get; }
		public AntigenColour AntibodyColourRight { get; set; }

		protected override float DefaultHeight {
			get { return 0.001f; }
		}
		protected override float DefaultRadius {
			get { return 0.5f; }
		}
		protected override string DefaultModel {
			get { return "LymphyNucleus.mesh"; }
		}
		protected override uint DefaultCollisionGroupID {
			get { return Groups.CollidablePushableID; }
		}
		protected override string DefaultMaterial {
			get { return null; }
		}
		protected override MoveBehaviour DefaultMoveBehaviour {
			get { return MoveBehaviour.IGNORE; }
		}
		protected override float DefaultMoveSpeed {
			get { return 0.1f; }
		}

		/// <summary>
		/// Things will be disposing at the end
		/// </summary>
		Collection<IDisposable> toDispose = new Collection<IDisposable>();

		/// <summary>
		/// Sets up the player, including mogre stuff and physics. Also adds the player to the kernel.
		/// </summary>
		public Player(ThingTemplate tt) : base(tt)
		{
			Launch.Log("[Loading] First Get<Player>");
			
			this.AntibodyColourLeft = AntigenColour.red;
			this.AntibodyColourRight = AntigenColour.blue;

			LKernel.AddLevelObject<Player>(this);
		}

		protected override void CreateMoreMogreStuff() {
			// Color overlay, for cytoplasm
			var sceneMgr = LKernel.Get<SceneManager>();

			Entity glowEntity = sceneMgr.CreateEntity("Lymphy_InnerGlow", "LymphyOuterMembrane.mesh");
			glowEntity.RenderQueueGroup = GlowHandler.RENDER_QUEUE_INNER_GLOW;
			glowEntity.SetMaterialName("Lymphy_InnerGlow");
			SceneNode glowNode = Node.CreateChildSceneNode("Lymphy_InnerGlowNode");
			glowNode.AttachObject(glowEntity);
			// Balloon glow
			Entity alphaGlowEntity = sceneMgr.CreateEntity("Lymphy_OuterGlow", "LymphyOuterMembrane.mesh");
			alphaGlowEntity.RenderQueueGroup = GlowHandler.RENDER_QUEUE_OUTER_GLOW;
			alphaGlowEntity.SetMaterialName("Lymphy_BalloonGlow");
			SceneNode alphaGlowNode = Node.CreateChildSceneNode("Lymphy_OuterGlowNode");
			alphaGlowNode.AttachObject(alphaGlowEntity);

			CreateRibbon(4, 30, new ColourValue(0.937f, 0.604f, 0.631f), Constants.TRIPPY ? 100 : 0.9f);

			toDispose.Add(glowEntity);
			toDispose.Add(glowNode);
			toDispose.Add(alphaGlowEntity);
			toDispose.Add(alphaGlowNode);
		}

		public override void Dispose() {
			foreach (var v in toDispose) {
				v.Dispose();
			}
			base.Dispose();
		}
	}
}
