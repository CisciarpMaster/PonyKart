using System;
using Mogre;

namespace Ponykart.Stuff {
	public class DebugDrawerHandler : IDisposable {

		public DebugDrawerHandler() {
			Launch.Log("[Loading] Creating DebugDrawerHandler");

#if DEBUG
			DebugDrawer.SetSingleton(new DebugDrawer(LKernel.Get<SceneManager>(), 0.6f));

			LKernel.Get<Root>().FrameStarted += FrameStarted;
			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		bool FrameStarted(FrameEvent evt) {
			DebugDrawer.Singleton.Build();
			return true;
		}

		bool FrameEnded(FrameEvent evt) {
			//DebugDrawer.Singleton.Clear();
			return true;
#endif
		}

		public void Dispose() {
#if DEBUG
			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			LKernel.Get<Root>().FrameEnded -= FrameEnded;
			DebugDrawer.Singleton.Dispose();
#endif
		}
	}
}
