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
			LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
		}

		void OnLevelPostLoad(LevelChangedEventArgs eventArgs) {
			var sceneMgr = LKernel.GetG<SceneManager>();
			var def = eventArgs.NewLevel.Definition;
			// for shadows, see KernelLevelCleanup -> SetupShadows

			// background color
			LKernel.GetG<Viewport>().BackgroundColour = def.GetVectorProperty("Background", Vector3.UNIT_SCALE).ToColourValue();

			// ambient
			sceneMgr.AmbientLight = def.GetVectorProperty("Ambient", Vector3.UNIT_SCALE).ToColourValue();

			// sunlight
			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = def.GetVectorProperty("SunlightDirection", new Vector3(0.1f, -1f, 0.1f));
			light.Direction.Normalise();
			light.DiffuseColour = new ColourValue(1.5f, 0.9f, 0.4f);
			light.SpecularColour = new ColourValue(1f, 1f, 1f);
			light.CastShadows = true;

			// skybox
			sceneMgr.SetSkyBox(true, "saa_sky", 1995f);

			// fog
			FogMode mode = FogMode.FOG_NONE;
			string sMode = def.GetStringProperty("FogType", "Linear");
			// only linear atm
			if (sMode == "None")
				mode = FogMode.FOG_NONE;
			else if (sMode == "Exp")
				mode = FogMode.FOG_EXP;
			else if (sMode == "Exp2")
				mode = FogMode.FOG_EXP2;
			else if (sMode == "Linear")
				mode = FogMode.FOG_LINEAR;
			sceneMgr.SetFog(
				mode,
				def.GetQuatProperty("FogColour", Quaternion.IDENTITY).ToColourValue(),
				0.001f,
				def.GetFloatProperty("FogStart", 100),
				def.GetFloatProperty("FogEnd", 500));

#if DEBUG
			// make some axes
			LKernel.GetG<Spawner>().Spawn("Axis", Vector3.ZERO);
#endif
		}
	}
}
