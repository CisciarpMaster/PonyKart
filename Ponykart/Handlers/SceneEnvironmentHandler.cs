﻿using Mogre;
using Ponykart.Levels;

namespace Ponykart.Handlers {
	/// <summary>
	/// Just a bunch of extra stuff that needs to go in the scene, such as ambient light, shadow info, a sunlight, etc.
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class SceneEnvironmentHandler {

		public SceneEnvironmentHandler() {
			LKernel.Get<LevelManager>().OnLevelLoad += new LevelEventHandler(OnLevelLoad);
		}

		void OnLevelLoad(LevelChangedEventArgs eventArgs) {
			var sceneMgr = LKernel.Get<SceneManager>();
			// ambient light
			sceneMgr.AmbientLight = new ColourValue(0.8f, 0.8f, 0.8f);
			// shadows
			sceneMgr.ShadowColour = new ColourValue(0.8f, 0.8f, 0.8f);
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;

			// sunlight
			Light light = sceneMgr.CreateLight("sun");
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(0.1f, -1, 0.1f);
			light.Direction.Normalise();
			light.CastShadows = true;

#if DEBUG
			// make some axes
			SceneNode axesNode = sceneMgr.RootSceneNode.CreateChildSceneNode("axes node");
			Entity axesEnt = sceneMgr.CreateEntity("axes entity", "axes.mesh");
			axesEnt.SetMaterialName("Core/NodeMaterial");
			axesNode.AttachObject(axesEnt);
			axesNode.SetScale(0.1f, 0.1f, 0.1f);

			// for testing animation
			//LKernel.Get<Core.Spawner>().Spawn("ZergShip", "zerg", new Vector3(10, 5, 0));
#endif
		}
	}
}