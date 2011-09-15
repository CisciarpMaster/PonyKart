#if DEBUG
using Mogre;
using Ponykart.Physics;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class DebugDrawerHandler : LDisposable, ILevelHandler {

		public DebugDrawerHandler() {
			MogreDebugDrawer.SetSingleton(new MogreDebugDrawer(LKernel.GetG<SceneManager>(), 0.6f));

			LKernel.GetG<Root>().FrameStarted += FrameStarted;
			LKernel.GetG<Root>().FrameEnded += FrameEnded;
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
		}

		public void Detach() {
			LKernel.GetG<Root>().FrameStarted -= FrameStarted;
			LKernel.GetG<Root>().FrameEnded -= FrameEnded;

			Dispose();
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			MogreDebugDrawer.Singleton.Dispose();

			base.Dispose(disposing);
		}
	}
}
#endif