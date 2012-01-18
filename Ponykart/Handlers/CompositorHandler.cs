using Mogre;
using Ponykart.Core;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class CompositorHandler : ILevelHandler {
		Viewport v;

		public CompositorHandler() {
			if (Options.ModelDetail == ModelDetailOption.High) {
				v = LKernel.GetG<Viewport>();
				CompositorManager.Singleton.AddCompositor(v, "Bloom");
				CompositorManager.Singleton.SetCompositorEnabled(v, "Bloom", true);
			}
		}

		public void Detach() {
			if (Options.ModelDetail == ModelDetailOption.High) {
				CompositorManager.Singleton.RemoveCompositor(v, "Bloom");
			}
		}
	}
}
