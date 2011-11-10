using Mogre;

namespace Ponykart.Core {
	public class BasicCamera : LCamera {

		public BasicCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 3000f;
			Camera.AutoAspectRatio = true;

			Camera.Position = new Vector3(0, 5, -20);
			Camera.LookAt(new Vector3(0, 3, 0));

			sceneMgr.RootSceneNode.AttachObject(Camera);
		}
	}
}
