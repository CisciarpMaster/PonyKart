using Mogre;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a bunch of extra stuff that needs to go in the scene, such as ambient light, shadow info, a sunlight, etc.
	/// </summary>
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

			LKernel.Get<Spawner>().Spawn("ZergShip", "zerg", new Vector3(10, 5, 0));
		}
	}
}
