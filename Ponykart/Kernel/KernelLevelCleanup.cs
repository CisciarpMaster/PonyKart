using Mogre;
using Ponykart.UI;

namespace Ponykart {
	public static partial class LKernel {
		/// <summary>
		/// This is run just after the OnLevelUnload event.
		/// </summary>
		public static void Cleanup() {
			RemakeSceneManager();
			LevelObjects.Clear();
		}

		/// <summary>
		/// destroys everything in the scene manager so it's as good as new without destroying the scene manager itself
		/// </summary>
		private static void RemakeSceneManager() {
			Launch.Log("[Loading] Remaking SceneManager...");
			// get our old scene manager
			SceneManager oldMgr = GetG<SceneManager>();
			// destroy it
			GetG<Root>().DestroySceneManager(oldMgr);
			// make our new scene manager
			SceneManager newMgr = GetG<Root>().CreateSceneManager("OctreeSceneManager", "sceneMgr");

			// re-add it to the kernel objects
			GlobalObjects[typeof(SceneManager)] = newMgr;
			// tell miyagi that it changed
			GetG<UIMain>().ChangeSceneManager(newMgr);
			// then dispose of the old one
			oldMgr.Dispose();

			newMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;
			newMgr.ShadowFarDistance = 150f;
			newMgr.ShadowColour = new ColourValue(0.8f, 0.8f, 0.8f);
			//SetupShadows(newMgr);
		}

		/*private static void SetupShadows(SceneManager sceneMgr) {
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_MODULATIVE_INTEGRATED;

			sceneMgr.SetShadowTextureCountPerLightType(Light.LightTypes.LT_DIRECTIONAL, 3);
			sceneMgr.ShadowTextureCount = 3;
			sceneMgr.SetShadowTextureConfig(0, 1024, 1024, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(1, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(2, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.ShadowTextureSelfShadow = true;
			sceneMgr.ShadowCasterRenderBackFaces = false;
			sceneMgr.ShadowFarDistance = 150;
			sceneMgr.SetShadowTextureCasterMaterial("PSSM/shadow_caster");
			sceneMgr.SetShadowTextureFadeStart(0.7f);

			PSSMShadowCameraSetup pssm = new PSSMShadowCameraSetup();
			pssm.SplitPadding = 1f;
			pssm.CalculateSplitPoints(3, 0.01f, sceneMgr.ShadowFarDistance - 10);
			pssm.SetOptimalAdjustFactor(0, 2);
			pssm.SetOptimalAdjustFactor(1, 1f);
			pssm.SetOptimalAdjustFactor(2, 0.5f);
			pssm.UseSimpleOptimalAdjust = false;

			sceneMgr.SetShadowCameraSetup(new ShadowCameraSetupPtr(pssm));
		}*/
	}
}
