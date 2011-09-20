using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.Properties;

namespace Ponykart.Core {
	/// <summary>
	/// A basic third-person camera with some smoothing.
	/// TODO: Make more camera types and a way to switch between them more effectively.
	/// </summary>
	public class PlayerCamera : LDisposable, ILevelHandler {
		
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
			Camera.AspectRatio = ((float) Settings.Default.WindowWidth) / ((float) Settings.Default.WindowHeight);

			CameraNode = manager.RootSceneNode.CreateChildSceneNode("CameraNode", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			TargetNode = manager.RootSceneNode.CreateChildSceneNode("CameraTargetNode", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));

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
			kartCamNode = kart.RootNode.CreateChildSceneNode(kart.Name + "_cam", new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset));
			kartTargetNode = kart.RootNode.CreateChildSceneNode(kart.Name + "_camtarget", new Vector3(0, Settings.Default.CameraTargetYOffset, 0));
			followKart = kart;

			CameraNode.Position = kartCamNode._getDerivedPosition();
			TargetNode.Position = kartTargetNode._getDerivedPosition();
		}

		/// <summary>
		/// Updates the camera
		/// TODO: stop it from going through the terrain
		/// </summary>
		bool UpdateCamera(FrameEvent evt) {
			Vector3 displacement;

			displacement = (kartCamNode._getDerivedPosition() - CameraNode.Position) * Settings.Default.CameraTightness * evt.timeSinceLastFrame;
			CameraNode.Translate(displacement);

			displacement = (kartTargetNode._getDerivedPosition() - TargetNode.Position) * Settings.Default.CameraTightness * evt.timeSinceLastFrame;
			TargetNode.Translate(displacement);

			return true;
		}

		public void Detach() {
			//LKernel.Get<Spawner>().OnKartCreation -= OnKartCreation;
			LKernel.GetG<Root>().FrameStarted -= UpdateCamera;

			Dispose();
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
