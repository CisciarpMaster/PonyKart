using System.Diagnostics;
using Mogre;
using MOIS;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Sound;
using Ponykart.Stuff;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class is hooked up to the keyboard events and does miscellaneous things depending on what keys are pressed.
	/// It's mostly for debugging functions
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MiscKeyboardHandler {

		public MiscKeyboardHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed()) // if the input is swallowed, do nothing
				return;

			switch (ke.key) {
				case KeyCode.KC_MINUS: // the -_ key
					LKernel.GetG<DebugOverlayManager>().ToggleDebugOverlay();
					break;
#if DEBUG
				case KeyCode.KC_X:
					MogreDebugDrawer.Singleton.Clear();
					break;
				case KeyCode.KC_I:
					PhysicsMain.DrawLines = !PhysicsMain.DrawLines;
					break;
#endif
				case KeyCode.KC_M:
					Settings.Default.EnableMusic = !Settings.Default.EnableMusic;
					break;
				case KeyCode.KC_P:
					Settings.Default.EnableSounds = !Settings.Default.EnableSounds;
					break;
				case KeyCode.KC_N:
					LKernel.GetG<SoundMain>().Play2D("Sweet Apple Acres 128bpm.ogg", true);
					break;
				case KeyCode.KC_U:
					LKernel.GetG<PlayerManager>().MainPlayer.Body.LinearVelocity += new Vector3(0, 20, 0);
					break;
				case KeyCode.KC_F:
					LKernel.GetG<PlayerManager>().MainPlayer.Body.LinearVelocity *= 2f;
					break;
				case KeyCode.KC_F1:
					LKernel.GetG<StaticGeometryManager>().ToggleVisible();
					break;
				case KeyCode.KC_F2:
					LKernel.GetG<InstancedGeometryManager>().ToggleVisible();
					break;
				case KeyCode.KC_F3:
					var level = LKernel.GetG<LevelManager>().CurrentLevel;
					foreach (LThing t in level.Things.Values) {
						foreach (BillboardSetComponent bsc in t.BillboardSetComponents) {
							bsc.BillboardSet.Visible = !bsc.BillboardSet.Visible;
						}
					}
					break;
				case KeyCode.KC_F11:
					LKernel.GetG<RenderWindow>().SetFullscreen(!LKernel.GetG<RenderWindow>().IsFullScreen, Settings.Default.WindowWidth, Settings.Default.WindowHeight);
					break;
				case KeyCode.KC_G:
					System.GC.Collect(0, System.GCCollectionMode.Forced);
					break;
				case KeyCode.KC_C:
					ProcessStartInfo p = new ProcessStartInfo("syncmedia.cmd");
					Process proc = new Process();
					proc.StartInfo = p;
					proc.Start();
					proc.WaitForExit();

					LKernel.GetG<LuaMain>().Restart();
					LKernel.Get<WheelFactory>().ReadWheelsFromFiles();
					LKernel.Get<PhysicsMaterialFactory>().ReadMaterialsFromFiles();
					LKernel.GetG<CollisionShapeManager>().Clear();

					ResourceGroupManager.Singleton.InitialiseResourceGroup("General");
					if (ResourceGroupManager.Singleton.ResourceGroupExists(LKernel.GetG<LevelManager>().CurrentLevel.Name))
						ResourceGroupManager.Singleton.InitialiseResourceGroup(LKernel.GetG<LevelManager>().CurrentLevel.Name);
					//MaterialManager.Singleton.ReloadAll(false);
					//MeshManager.Singleton.ReloadAll(false);

					Settings.Default.Reload();
					break;
			}
		}
	}
}
