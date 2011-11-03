using Mogre;

namespace Ponykart.Core {
	public abstract class LCamera : LDisposable {
		public Camera Camera { get; protected set; }

		public LCamera() {
			LKernel.GetG<Root>().FrameStarted += UpdateCamera;
		}

		/// <summary>
		/// shorthand
		/// </summary>
		public void Register() {
			LKernel.GetG<CameraManager>().RegisterCamera(this);
		}

		/// <summary>
		/// Update the camera every frame! 
		/// </summary>
		protected virtual bool UpdateCamera(FrameEvent evt) {
			return true;
		}

		/// <summary>
		/// Unhook from the framestarted event
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing)
				LKernel.GetG<Root>().FrameStarted -= UpdateCamera;

			base.Dispose(disposing);
		}
	}
}
