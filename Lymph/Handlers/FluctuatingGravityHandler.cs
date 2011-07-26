using System;
using Mogre;
using Mogre.PhysX;
using Ponykart.Phys;
using Math = System.Math;

namespace Ponykart.Handlers {
	/// <summary>
	/// SILLINESS AT ITS FINEST
	/// </summary>
	public class FluctuatingGravityHandler : IDisposable {
		Scene scene;

		public FluctuatingGravityHandler() {
			LKernel.Get<Root>().FrameStarted += FrameStarted;
			scene = LKernel.Get<PhysXMain>().Scene;
		}

		double elapsed;
		Vector3 vec = new Vector3(0, 0, 0);
		bool FrameStarted(FrameEvent evt) {
			if (scene == null || scene.IsDisposed || scene.Gravity == null) {
				Dispose();
				return true;
			}
			elapsed += evt.timeSinceLastFrame;
			vec.y = (float)(Math.Sin(elapsed) * 40) - 10;
			scene.Gravity = vec;

			return true;
		}

		public void Dispose() {
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
		}
	}
}
