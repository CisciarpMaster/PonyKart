using MOIS;

namespace Lymph.Core {
	public delegate void PauseEventHandler(bool isPaused);

	public class Pauser {
		/// <summary>
		/// An event for things that need it
		/// </summary>
		public event PauseEventHandler PauseEvent;
		/// <summary>
		/// I think most things will be fine with just a boolean.<br />
		/// Setting this to true will pause the spawner, physics engine, level changer, movement managers, etc.
		/// It won't pause animations, UI, scripts, cameras, and so on.
		/// </summary>
		public static bool Paused = false;

		public Pauser() {
			Launch.Log("[Loading] Creating Pauser");

			// if we press `, then pause
			LKernel.Get<InputMain>().OnKeyboardPress_Anything += (ke) => {
				if (ke.key == KeyCode.KC_GRAVE)
					PauseWithEvent();
			};

			LKernel.Get<InputSwallowerManager>().AddSwallower(() => Paused, this);
		}
		/// <summary>
		/// Use this to pause things but it also fires off a pause event, which may cause other things to happen that you don't want.
		/// </summary>
		public void PauseWithEvent() {
			Launch.Log("Pause!");
			Paused = !Paused;
			if (PauseEvent != null)
				PauseEvent(Paused);
		}
	}
}
