using Mogre;

namespace Ponykart.Core {
	/// <summary>
	/// All camera classes should subclass from this.
	/// </summary>
	public abstract class LCamera : LDisposable {
		/// <summary>
		/// The Mogre camera we're manipulating
		/// </summary>
		public Camera Camera { get; protected set; }

		/// <summary>
		/// Hook up to the frame started event
		/// </summary>
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
		/// Make sure you register the camera before calling this!
		/// </summary>
		public void MakeActive() {
			LKernel.GetG<CameraManager>().SwitchCurrentCamera(this);
		}

		/// <summary>
		/// Update the camera every frame! 
		/// </summary>
		protected virtual bool UpdateCamera(FrameEvent evt) {
			return true;
		}

		/// <summary>
		/// Unhook from the frame started event
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
