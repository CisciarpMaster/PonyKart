using System;
using Lymph.Actors;
using Lymph.Core;
using Lymph.Lua;
using Lymph.Phys;
using Lymph.Stuff;
using MOIS;

namespace Lymph.Handlers
{
	/// <summary>
	/// This class is hooked up to the keyboard events and does miscellaneous things depending on what keys are pressed.
	/// It's mostly for debugging functions
	/// </summary>
	public class MiscKeyboardHandler : IDisposable
	{

		public MiscKeyboardHandler() {
			Launch.Log("[Loading] Creating MiscKeyboardHandler");
			LKernel.Get<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing MiscKeyboardHandler");
			LKernel.Get<InputMain>().OnKeyboardPress_Anything -= OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed()) // if the input is swallowed, do nothing
				return;

			switch (ke.key) {
				case KeyCode.KC_MINUS: // the -_ key
					LKernel.Get<DebugOverlayManager>().ToggleDebugOverlay();
					break;
				case KeyCode.KC_F3:
					LKernel.Get<PhysXMain>().ShootBox();
					break;
				case KeyCode.KC_NUMPAD1:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.red;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.blue; break;
				case KeyCode.KC_NUMPAD2:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.yellow;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.green; break;
				case KeyCode.KC_NUMPAD3:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.orange;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.magenta; break;
				case KeyCode.KC_NUMPAD4:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.purple;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.cyan; break;
				case KeyCode.KC_NUMPAD5:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.white;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.black; break;
				case KeyCode.KC_NUMPAD6:
				case KeyCode.KC_NUMPAD7:
				case KeyCode.KC_NUMPAD8:
				case KeyCode.KC_NUMPAD9:
				case KeyCode.KC_NUMPAD0:
					LKernel.Get<Player>().AntibodyColourLeft = AntigenColour.red;
					LKernel.Get<Player>().AntibodyColourRight = AntigenColour.blue; break;
				case KeyCode.KC_Z:
					LKernel.Get<Spawner>().Spawn(ThingEnum.GenericEnemy, "GenericEnemy", LKernel.Get<Player>().Node.Position);
					break;
				case KeyCode.KC_X:
					LKernel.Get<Spawner>().Spawn(ThingEnum.GenericEnemy, "GenericEnemy", LKernel.Get<Player>().Node.Position);
					break;
				case KeyCode.KC_M:
					Constants.MUSIC = !Constants.MUSIC;
					break;
				case KeyCode.KC_P:
					Constants.SOUNDS = !Constants.SOUNDS;
					break;
				case KeyCode.KC_N:
					LKernel.Get<Sound.SoundMain>().CreateAmbientSound("media/sound/Casey LaLonde - Danced In The Alleyway.ogg", "bgmusic", true);
					break;
				case KeyCode.KC_F2:
					//LKernel.Get<Player>().Move(new Vector3(1000, 1000, 1000));
					break;
				case KeyCode.KC_T:
					Constants.TRIPPY = !Constants.TRIPPY;
					break;
				case KeyCode.KC_F:
					//LKernel.Get<Player>().Move(new Vector3(50000, 0, 0));
					break;
				case KeyCode.KC_L:
					LKernel.Get<LuaMain>().DoFile("media/scripts/test.lua");
					break;
				case KeyCode.KC_C:
					System.Diagnostics.ProcessStartInfo p = new System.Diagnostics.ProcessStartInfo("syncmedia.cmd");
					System.Diagnostics.Process proc = new System.Diagnostics.Process();
					proc.StartInfo = p;
					proc.Start();
					proc.WaitForExit();

					LKernel.Get<LuaMain>().Restart();
					break;
			}
		}
	}
}
