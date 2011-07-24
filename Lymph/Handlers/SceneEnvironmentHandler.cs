using Mogre;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	public class SceneEnvironmentHandler {

		public SceneEnvironmentHandler() {
			LKernel.Get<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			var sceneMgr = LKernel.Get<SceneManager>();
			sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);
			sceneMgr.ShadowColour = new ColourValue(0.8f, 0.8f, 0.8f);
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;

			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.CastShadows = true;
		}
	}
}
