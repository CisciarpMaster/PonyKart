using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using Ponykart.Core;
using Ponykart.Players;
using Ponykart.Actors;

namespace Ponykart.Core {
	public class ChaseCamera : LCamera {
		private SceneNode CameraNode;
		private Kart FollowKart;

		public ChaseCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 3500f;
			Camera.AutoAspectRatio = true;

			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "ChaseCamera");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);

			FollowKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			Camera.SetAutoTracking(true, FollowKart.RootNode, new Vector3(0, 5, 0));
		}

		public override void OnSwitchToActive(LCamera oldCamera) {
			base.OnSwitchToActive(oldCamera);

			Camera.Position = oldCamera.Camera.Position;
		}
	}
}
