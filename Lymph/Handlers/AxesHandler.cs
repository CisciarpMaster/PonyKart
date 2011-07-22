using Mogre;

namespace Ponykart.Handlers {
	/// <summary>
	/// Makes a little axes thing at the origin of the scene. Useful for seeing which way is X and which way is Z, as well as where the origin is.
	/// </summary>
	public class AxesHandler {
		public AxesHandler() {
			var sceneMgr = LKernel.Get<SceneManager>();
			var node = sceneMgr.RootSceneNode.CreateChildSceneNode("axes node");
			var ent = sceneMgr.CreateEntity("axes entity", "axes.mesh");
			ent.SetMaterialName("Core/NodeMaterial");
			node.AttachObject(ent);
			node.SetScale(0.1f, 0.1f, 0.1f);
		}
	}
}
