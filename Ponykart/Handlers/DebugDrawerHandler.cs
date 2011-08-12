using Mogre;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level)]
	public class DebugDrawerHandler : ILevelHandler {

		public DebugDrawerHandler() {

#if DEBUG
			MogreDebugDrawer.SetSingleton(new MogreDebugDrawer(LKernel.Get<SceneManager>(), 0.6f));

			LKernel.Get<Root>().FrameStarted += FrameStarted;
			LKernel.Get<Root>().FrameEnded += FrameEnded;
		}

		bool FrameStarted(FrameEvent evt) {
			if (PhysicsMain.DrawLines)
				MogreDebugDrawer.Singleton.Build();
			return true;
		}

		bool FrameEnded(FrameEvent evt) {
			if (PhysicsMain.DrawLines)
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
