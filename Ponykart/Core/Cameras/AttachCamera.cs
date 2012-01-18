using Mogre;
using Ponykart.Actors;
using Ponykart.Players;

namespace Ponykart.Core {
	public class AttachCamera : LCamera {
		private SceneNode CameraNode;
		private Kart FollowKart;

		public AttachCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 3500f;
			Camera.AutoAspectRatio = true;

			FollowKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;

			CameraNode = FollowKart.RootNode.CreateChildSceneNode(name + "AttachCamera");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);
		}

		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			Camera.Position = FollowKart.RootNode.ConvertWorldToLocalPosition(oldCamera.Camera.Position);
			Camera.Orientation = FollowKart.RootNode.ConvertWorldToLocalOrientation(oldCamera.Camera.Orientation);
		}
	}
}
