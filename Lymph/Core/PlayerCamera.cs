using System;
using Mogre;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;

namespace Ponykart.Core {
	public class PlayerCamera : IDisposable {
		public Camera Camera { get; private set; }
		/// <summary> This camera's scene node </summary>
		public SceneNode Node { get; private set; }
		/// <summary> The scene node that this camera follows </summary>
		SceneNode followNode;
		/// <summary> Is the current level not the main menu? </summary>
		bool isPlayableLevel;

		public PlayerCamera() {
			var manager = LKernel.Get<SceneManager>();
			Launch.Log("[Loading] First Get<PlayerCamera>");
			isPlayableLevel = LKernel.Get<LevelManager>().IsPlayableLevel;

			Camera = manager.CreateCamera("Camera");

			Camera.NearClipDistance = 0.01f;
			Camera.FarClipDistance = 1000f;

			Camera.Position = new Vector3(0f, Constants.CAMERA_HEIGHT, Constants.CAMERA_HEIGHT);
			Camera.LookAt(new Vector3(0, 0, 0));
			Camera.AspectRatio = ((float)Constants.WINDOW_WIDTH) / ((float)Constants.WINDOW_HEIGHT);

			Node = manager.CreateSceneNode("CameraNode");
			Node.SetPosition(0, 0, 0);
			Node.AttachObject(Camera);

			// don't want to do any camera shenanigans on the first level
			if (isPlayableLevel)
				OnKartCreation(LKernel.Get<PlayerManager>().MainPlayer.Kart);

			/*camPos = new Vector3();
			camVelocity = new Vector3();
			cameraDistance = new Vector3();
			force = new Vector3();
			acceleration = new Vector3();*/
		}

		void OnKartCreation(Kart kart) {
			followNode = kart.Node;
		}

		/*Vector3 camPos;
		Vector3 camVelocity;
		readonly float cameraMass = 9;
		readonly float spring = 9;
		readonly float damping = 125;
		Vector3 cameraDistance;
		Vector3 force;
		Vector3 acceleration;*/

		/// <summary>
		/// Updates the cameraNode so it's pointing at the player
		/// </summary>
		public void UpdateCamera(FrameEvent evt) {
			if (LKernel.Get<LevelManager>().IsValidLevel) {
				/*Player player = LKernel.Get<PlayerManager>().MainPlayer;

				cameraDistance = player.Node.Position - camPos;
				force = cameraDistance * spring;
				force -= damping * camVelocity;

				acceleration = (force / cameraMass) * evt.timeSinceLastFrame;
				camVelocity += acceleration;
				camPos += camVelocity;

				//System.Console.WriteLine(cameraDistance);

				//camPos.x = player.Node.Position.x;
				//camPos.z = player.Node.Position.z;
				Node.Position = camPos;*/

				// this is a pretty crappy camera but it'll work for now
				Node.Position = followNode.ConvertLocalToWorldPosition(new Vector3(0, Constants.CAMERA_HEIGHT, -Constants.CAMERA_DISTANCE));
				Node.Translate(0f, 0f, -Constants.CAMERA_DISTANCE);
				Node.SetPosition(Node.Position.x, Constants.CAMERA_HEIGHT, Node.Position.z);
				Camera.LookAt(followNode._getDerivedPosition());
			}
		}

		public void Dispose() {
			if (isPlayableLevel)
				LKernel.Get<Spawner>().OnKartCreation -= OnKartCreation;

			LKernel.Get<SceneManager>().DestroyCamera(Camera);
			Camera.Dispose();
			LKernel.Get<SceneManager>().DestroySceneNode(Node);
			Node.Dispose();
		}
	}
}
