using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Core {
	/// <summary>
	/// A basic third-person camera with some smoothing.
	/// TODO: Make more camera types and a way to switch between them more effectively.
	/// </summary>
	public class PlayerCamera : ILevelHandler {
		
		public Camera Camera { get; private set; }
		SceneNode TargetNode;
		SceneNode CameraNode;
		Kart followKart;
		SceneNode kartCamNode;
		SceneNode kartTargetNode;

		public PlayerCamera() {
			var manager = LKernel.GetG<SceneManager>();
			Launch.Log("[Loading] Creating new PlayerCamera");

			Camera = manager.CreateCamera("Camera");

			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 1000f;
			Camera.AspectRatio = ((float) Constants.WINDOW_WIDTH) / ((float) Constants.WINDOW_HEIGHT);

			CameraNode = manager.RootSceneNode.CreateChildSceneNode("CameraNode", new Vector3(0, Constants.CAMERA_NODE_Y_OFFSET, Constants.CAMERA_NODE_Z_OFFSET));
			TargetNode = manager.RootSceneNode.CreateChildSceneNode("CameraTargetNode", new Vector3(0, Constants.CAMERA_TARGET_Y_OFFSET, 0));

			CameraNode.SetAutoTracking(true, TargetNode);
			CameraNode.SetFixedYawAxis(true);

			CameraNode.AttachObject(Camera);

			// don't want to do any camera shenanigans on the first level
			if (LKernel.GetG<LevelManager>().IsPlayableLevel) {
				OnKartCreation(LKernel.GetG<PlayerManager>().MainPlayer.Kart);

				LKernel.GetG<Root>().FrameStarted += UpdateCamera;
			}
		}

		/// <summary>
		/// Attaches two SceneNodes to the main kart so we can use them for camera stuff.
		/// </summary>
		void OnKartCreation(Kart kart) {
			if (kart == LKernel.GetG<PlayerManager>().MainPlayer.Kart) {
				kartCamNode = kart.RootNode.CreateChildSceneNode(kart.Name + "_cam", new Vector3(0, Constants.CAMERA_NODE_Y_OFFSET, Constants.CAMERA_NODE_Z_OFFSET));
				kartTargetNode = kart.RootNode.CreateChildSceneNode(kart.Name + "_camtarget", new Vector3(0, Constants.CAMERA_TARGET_Y_OFFSET, 0));
				followKart = kart;
			}
		}

		/// <summary>
		/// Updates the camera
		/// TODO: stop it from going through the terrain
		/// </summary>
		bool UpdateCamera(FrameEvent evt) {
			Vector3 displacement;

			displacement = (kartCamNode._getDerivedPosition() - CameraNode.Position) * Constants.CAMERA_TIGHTNESS * evt.timeSinceLastFrame;
			CameraNode.Translate(displacement);

			displacement = (kartTargetNode._getDerivedPosition() - TargetNode.Position) * Constants.CAMERA_TIGHTNESS * evt.timeSinceLastFrame;
			TargetNode.Translate(displacement);

			return true;
		}

		public void Dispose() {
			//LKernel.Get<Spawner>().OnKartCreation -= OnKartCreation;
			LKernel.GetG<Root>().FrameStarted -= UpdateCamera;

			var sceneMgr = LKernel.GetG<SceneManager>();

			sceneMgr.DestroyCamera(Camera);
			Camera.Dispose();
			sceneMgr.DestroySceneNode(CameraNode);
			CameraNode.Dispose();
			sceneMgr.DestroySceneNode(TargetNode);
			TargetNode.Dispose();
		}
	}
}
