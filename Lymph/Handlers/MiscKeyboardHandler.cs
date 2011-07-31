using System;
using MOIS;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Lua;
using Ponykart.Phys;
using Ponykart.Players;
using Ponykart.Stuff;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Handlers
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
				case KeyCode.KC_Z:
					LKernel.Get<Spawner>().Spawn(ThingEnum.Kart, "Kart", LKernel.Get<PlayerManager>().MainPlayer.ActorPosition);
					break;
#if DEBUG
				case KeyCode.KC_X:
					DebugDrawer.Singleton.Clear();
					break;
				case KeyCode.KC_I:
					Wheel.drawLines = !Wheel.drawLines;
					break;
#endif
				case KeyCode.KC_M:
					Constants.MUSIC = !Constants.MUSIC;
					break;
				case KeyCode.KC_P:
					Constants.SOUNDS = !Constants.SOUNDS;
					break;
				case KeyCode.KC_N:
					LKernel.Get<Sound.SoundMain>().CreateAmbientSound("media/sound/13 Hot Roderick Race.ogg", "bgmusic", true);
					break;
				case KeyCode.KC_F2:
					LKernel.Get<PlayerManager>().MainPlayer.Actor.AddForce(new Vector3(0, 100000, 0));
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
					LKernel.Get<WheelFactory>().ReadWheelsFromFiles();
					break;
			}
		}
	}
}
