using System;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;

namespace Ponykart.Core {
	public class PlayerCamera : IDisposable {
		public PlayerCamera() {
			var manager = LKernel.Get<SceneManager>();
			Launch.Log("[Loading] First Get<PlayerCamera>");

			Camera = manager.CreateCamera("Camera");

			Camera.NearClipDistance = 0.001f;
			Camera.FarClipDistance = 1000f;

			Camera.Position = new Vector3(0f, Constants.CAMERA_DISTANCE, Constants.CAMERA_DISTANCE);
			Camera.LookAt(new Vector3(0, 0, 0));
			Camera.AspectRatio = ((float)Constants.WINDOW_WIDTH) / ((float)Constants.WINDOW_HEIGHT);

			Node = manager.CreateSceneNode("CameraNode");
			Node.SetPosition(0, 0, 0);
			Node.AttachObject(Camera);

			camPos = new Vector3();
			camVelocity = new Vector3();
			cameraDistance = new Vector3();
			force = new Vector3();
			acceleration = new Vector3();
		}

		Vector3 camPos;
		Vector3 camVelocity;
		readonly float cameraMass = 9;
		readonly float spring = 9;
		readonly float damping = 125;
		Vector3 cameraDistance;
		Vector3 force;
		Vector3 acceleration;

		/// <summary>
		/// Updates the cameraNode so it's pointing at the player
		/// </summary>
		public void UpdateCamera(FrameEvent evt) {
			if (LKernel.Get<LevelManager>().IsValidLevel) {
				Player player = LKernel.Get<Player>();

				cameraDistance = player.Node.Position - camPos;
				force = cameraDistance * spring;
				force -= damping * camVelocity;

				acceleration = (force / cameraMass) * evt.timeSinceLastFrame;
				camVelocity += acceleration;
				camPos += camVelocity;

				//System.Console.WriteLine(cameraDistance);

				//camPos.x = player.Node.Position.x;
				//camPos.z = player.Node.Position.z;
				Node.Position = camPos;
			}
		}

		public Camera Camera { get; private set; }
		public SceneNode Node { get; private set; }


		#region IDisposable stuff
		public void Dispose() {
			LKernel.Get<SceneManager>().DestroyCamera(Camera);
			Camera.Dispose();
			LKernel.Get<SceneManager>().DestroySceneNode(Node);
			Node.Dispose();
		}
		#endregion
	}
}
