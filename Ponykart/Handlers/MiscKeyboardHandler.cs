using System.Diagnostics;
using LymphThing;
using MOIS;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Lua;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;
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
			LKernel.Get<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed()) // if the input is swallowed, do nothing
				return;

			switch (ke.key) {
				case KeyCode.KC_MINUS: // the -_ key
					LKernel.Get<DebugOverlayManager>().ToggleDebugOverlay();
					break;
				case KeyCode.KC_B:
					LKernel.Get<PhysicsMain>().ShootPrimitive();
					break;
				case KeyCode.KC_K:
					LKernel.Get<Spawner>().Spawn("Kart", LKernel.Get<PlayerManager>().MainPlayer.NodePosition);
					break;
				case KeyCode.KC_X:
					MogreDebugDrawer.Singleton.Clear();
					break;
				case KeyCode.KC_I:
					PhysicsMain.DrawLines = !PhysicsMain.DrawLines;
					break;
				case KeyCode.KC_M:
					Constants.MUSIC = !Constants.MUSIC;
					break;
				case KeyCode.KC_P:
					Constants.SOUNDS = !Constants.SOUNDS;
					break;
				case KeyCode.KC_N:
					LKernel.Get<Sound.SoundMain>().CreateAmbientSound("media/sound/13 Hot Roderick Race.ogg", "bgmusic", true);
					break;
				case KeyCode.KC_U:
					LKernel.Get<PlayerManager>().MainPlayer.Body.ApplyForce(new Vector3(0, 300000, 0), Vector3.ZERO);
					break;
				case KeyCode.KC_F:
					LKernel.Get<PlayerManager>().MainPlayer.Kart.Body.LinearVelocity *= 2f;
					break;
				case KeyCode.KC_L:
					LKernel.Get<LuaMain>().DoFile(Settings.Default.ScriptLocation + "test" + Settings.Default.LuaFileExtension);
					break;
				case KeyCode.KC_C:
					ProcessStartInfo p = new ProcessStartInfo("syncmedia.cmd");
					Process proc = new Process();
					proc.StartInfo = p;
					proc.Start();
					proc.WaitForExit();

					LKernel.Get<LuaMain>().Restart();
					LKernel.Get<WheelFactory>().ReadWheelsFromFiles();
					LKernel.Get<PhysicsMaterialFactory>().ReadMaterialsFromFiles();
					LKernel.Get<ThingDatabase>().ClearDatabase();
					break;
			}
		}
	}
}
