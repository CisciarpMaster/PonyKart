using System;
using Mogre;

namespace Ponykart.Handlers {
	public class DebugDrawerHandler : IDisposable {

		public DebugDrawerHandler() {
			Launch.Log("[Loading] Creating DebugDrawerHandler");

#if DEBUG
			MogreDebugDrawer.SetSingleton(new MogreDebugDrawer(LKernel.Get<SceneManager>(), 0.6f));

			LKernel.Get<Root>().FrameStarted += FrameStarted;
			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		bool FrameStarted(FrameEvent evt) {
			MogreDebugDrawer.Singleton.Build();
			return true;
		}

		bool FrameEnded(FrameEvent evt) {
			MogreDebugDrawer.Singleton.Clear();
			return true;
#endif
		}

		public void Dispose() {
#if DEBUG
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			LKernel.Get<Root>().FrameEnded -= FrameEnded;
			MogreDebugDrawer.Singleton.Dispose();
#endif
		}
	}
}
