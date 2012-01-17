using Mogre;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class CompositorHandler : ILevelHandler {
		Viewport v;

		public CompositorHandler() {
			v = LKernel.GetG<Viewport>();
			CompositorManager.Singleton.AddCompositor(v, "Bloom");
			CompositorManager.Singleton.SetCompositorEnabled(v, "Bloom", true);
		}

		public void Detach() {
			CompositorManager.Singleton.RemoveCompositor(v, "Bloom");
		}
	}
}
