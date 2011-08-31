using Mogre;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a bunch of extra stuff that needs to go in the scene, such as ambient light, shadow info, a sunlight, etc.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class SceneEnvironmentHandler {

		public SceneEnvironmentHandler() {
			LKernel.GetG<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			var sceneMgr = LKernel.GetG<SceneManager>();
			// ambient light
			sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);
			// shadows
			sceneMgr.ShadowColour = new ColourValue(0.8f, 0.8f, 0.8f);
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;
			// TODO read this from a file
			LKernel.GetG<Viewport>().BackgroundColour = new ColourValue(0.7373f, 0.8902f, 0.9490f);

			// sunlight
			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.CastShadows = true;

#if DEBUG
			// make some axes
			LKernel.GetG<Core.Spawner>().Spawn("Axis", Vector3.ZERO);

			// for testing animation
			//LKernel.Get<Core.Spawner>().Spawn("ZergShip", "zerg", new Vector3(10, 5, 0));
#endif
		}
	}
}
