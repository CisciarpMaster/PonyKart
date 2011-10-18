using Mogre;
using Ponykart.Core;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a bunch of extra stuff that needs to go in the scene, such as ambient light, shadow info, a sunlight, etc.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class SceneEnvironmentHandler {

		public SceneEnvironmentHandler() {
			LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			var sceneMgr = LKernel.GetG<SceneManager>();
			// ambient light
			//sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);
			// shadows
			
			// TODO read this from a file
			LKernel.GetG<Viewport>().BackgroundColour = new ColourValue(0.7373f, 0.8902f, 0.9490f);

			// sunlight
			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.DiffuseColour = new ColourValue(0.8f, 0.8f, 0.8f);
			light.SpecularColour = new ColourValue(0.8f, 0.8f, 0.8f);
			light.CastShadows = true;

			sceneMgr.SetSkyBox(true, "saa_sky", 1995f);

#if DEBUG
			// make some axes
			LKernel.GetG<Spawner>().Spawn("Axis", Vector3.ZERO);

			// for testing animation
			//LKernel.Get<Spawner>().Spawn("ZergShip", "zerg", new Vector3(10, 5, 0));
#endif
		}
	}
}
