using Mogre;
using Ponykart.Actors;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Core {
	/// <summary>
	/// A basic third-person camera with some smoothing.
	/// </summary>
	public class PlayerCamera : LCamera {
		SceneNode TargetNode;
		SceneNode CameraNode;
		Kart followKart;
		SceneNode kartCamNode;
		SceneNode kartTargetNode;

		public PlayerCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);

			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 3500f;
			Camera.AutoAspectRatio = true;

			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_PlayerCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			TargetNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "_PlayerCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.SetAutoTracking(true, TargetNode);
			CameraNode.SetFixedYawAxis(true);

			CameraNode.AttachObject(Camera);

			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			kartCamNode = followKart.RootNode.CreateChildSceneNode(name + "_KartCameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			kartTargetNode = followKart.RootNode.CreateChildSceneNode(name + "_KartCameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

			CameraNode.Position = kartCamNode._getDerivedPosition();
			TargetNode.Position = kartTargetNode._getDerivedPosition();
		}

		private readonly float _cameraTightness = Settings.Default.CameraTightness;
		/// <summary>
		/// Updates the camera
		/// TODO: stop it from going through the terrain
		/// </summary>
		protected override bool UpdateCamera(FrameEvent evt) {
			Vector3 displacement;

			displacement = (kartCamNode._getDerivedPosition() - CameraNode.Position) * _cameraTightness * evt.timeSinceLastFrame;
			CameraNode.Translate(displacement);

			displacement = (kartTargetNode._getDerivedPosition() - TargetNode.Position) * _cameraTightness * evt.timeSinceLastFrame;
			TargetNode.Translate(displacement);

			return true;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var sceneMgr = LKernel.GetG<SceneManager>();

				sceneMgr.DestroyCamera(Camera);
				sceneMgr.DestroySceneNode(CameraNode);
				sceneMgr.DestroySceneNode(TargetNode);
			}

			Camera.Dispose();
			CameraNode.Dispose();
			TargetNode.Dispose();

			base.Dispose(disposing);
		}
	}
}
