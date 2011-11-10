using Mogre;
using MOIS;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Stuff;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Core {
	public class FreeCamera : LCamera {
		private SceneNode CameraNode;
		private Vector3 Offset;

		public FreeCamera(string name) : base(name) {
			var sceneMgr = LKernel.GetG<SceneManager>();

			Camera = sceneMgr.CreateCamera(name);
			Camera.NearClipDistance = 0.5f;
			Camera.FarClipDistance = 3500f;
			Camera.AutoAspectRatio = true;

			CameraNode = sceneMgr.RootSceneNode.CreateChildSceneNode(name + "FreeNode");
			CameraNode.AttachObject(Camera);
			CameraNode.SetFixedYawAxis(true);

			var inputMain = LKernel.GetG<InputMain>();
			inputMain.OnKeyboardPress_Anything += OnKeyboardPress_Anything;
			inputMain.OnKeyboardRelease_Anything += OnKeyboardRelease_Anything;
			inputMain.OnMouseMove += OnMouseMove;
			inputMain.OnMousePress_Right += OnMousePress_Right;
		}

		void OnMousePress_Right(MouseEvent eventArg1, MouseButtonID eventArg2) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			Vector3 pos = new Vector3(), norm = new Vector3();
			if (new MogreRaycaster().RaycastFromPoint(CameraNode.Position, -CameraNode.GetLocalZAxis(), ref pos, ref norm)) {
				var kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
				kart.Body.Activate();

				Matrix4 mat = new Matrix4();
				mat.MakeTransform(pos, Vector3.UNIT_SCALE, kart.RootNode.Orientation);
				kart.Body.WorldTransform = mat;
			}
		}

		/// <summary>
		/// yaw/pitch the camera around
		/// </summary>
		void OnMouseMove(MouseEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			CameraNode.Yaw(new Degree(-eventArgs.state.X.rel / 4f), Node.TransformSpace.TS_WORLD);
			CameraNode.Pitch(new Degree(-eventArgs.state.Y.rel / 4f), Node.TransformSpace.TS_LOCAL);
		}

		/// <summary>
		/// undo anything that happens when we press anything
		/// </summary>
		void OnKeyboardRelease_Anything(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			switch (eventArgs.key) {
				case KeyCode.KC_UP:
					Offset.z += 1;
					break;
				case KeyCode.KC_DOWN:
					Offset.z -= 1;
					break;
				case KeyCode.KC_LEFT:
					Offset.x += 1;
					break;
				case KeyCode.KC_RIGHT:
					Offset.x -= 1;
					break;
				case KeyCode.KC_RSHIFT:
					Offset.y -= 1;
					break;
				case KeyCode.KC_RCONTROL:
					Offset.y += 1;
					break;
				case KeyCode.KC_SLASH:
					Offset *= 0.25f;
					break;
			}
		}

		void OnKeyboardPress_Anything(KeyEvent eventArgs) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed() || !IsActive)
				return;

			switch (eventArgs.key) {
				case KeyCode.KC_UP:
					Offset.z -= 1;
					break;
				case KeyCode.KC_DOWN:
					Offset.z += 1;
					break;
				case KeyCode.KC_LEFT:
					Offset.x -= 1;
					break;
				case KeyCode.KC_RIGHT:
					Offset.x += 1;
					break;
				case KeyCode.KC_RSHIFT:
					Offset.y += 1;
					break;
				case KeyCode.KC_RCONTROL:
					Offset.y -= 1;
					break;
				case KeyCode.KC_SLASH:
					Offset *= 4f;
					break;
			}
		}

		/// <summary>
		/// make the camera jump to wherever the kart is
		/// </summary>
		public override void OnSwitchToActive() {
			base.OnSwitchToActive();

			var kart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			CameraNode.Position = kart.RootNode.Position;
			CameraNode.Orientation = kart.RootNode.Orientation;

			CameraNode.Translate(new Vector3(0, Settings.Default.CameraNodeYOffset, Settings.Default.CameraNodeZOffset), Node.TransformSpace.TS_LOCAL);
			CameraNode.LookAt(kart.RootNode.Position, Node.TransformSpace.TS_WORLD);
		}

		/// <summary>
		///  set the offset to zero so when we switch back to being active, we don't start zooming off
		/// </summary>
		public override void OnSwitchToInactive() {
			base.OnSwitchToInactive();

			Offset = Vector3.ZERO;
		}

		protected override bool UpdateCamera(FrameEvent evt) {
			CameraNode.Translate(Offset, Node.TransformSpace.TS_LOCAL);
			return true;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var inputMain = LKernel.GetG<InputMain>();
				inputMain.OnKeyboardPress_Anything -= OnKeyboardPress_Anything;
				inputMain.OnKeyboardRelease_Anything -= OnKeyboardRelease_Anything;
				inputMain.OnMouseMove -= OnMouseMove;
				inputMain.OnMousePress_Right -= OnMousePress_Right;
			}

			base.Dispose(disposing);
		}
	}
}
