using System.Collections.Generic;
using MOIS;

namespace Ponykart.Core {
	/// <summary>
	/// Our key commands - these are for things that need to be polled. If you want to just respond to events, use the ones in InputMain.
	/// </summary>
	public enum LKey {
		Accelerate,
		TurnLeft,
		TurnRight,
		Brake,
		Reverse,
	}

	/// <summary>
	/// This class provides an interface between game commands (accelerate, etc) and key presses (WASD, etc).
	/// This way we can change which keys do things at runtime
	/// </summary>
	public class KeyBindingManager {
		/// <summary>
		/// The dictionary that converts our key commands into MOIS keys
		/// </summary>
		private IDictionary<LKey, KeyCode> LKeysDict;
		private IDictionary<KeyCode, LKey> MOISKeysDict;
		public IDictionary<LKey, LymphInputEventHandler<LKey>> PressEventsDict { get; private set; }
		public IDictionary<LKey, LymphInputEventHandler<LKey>> ReleaseEventsDict { get; private set; }


		public KeyBindingManager() {
			Launch.Log("[Loading] First Get<KeyBindingManager>");
			LKeysDict = new Dictionary<LKey, KeyCode>();
			MOISKeysDict = new Dictionary<KeyCode, LKey>();
			PressEventsDict = new Dictionary<LKey, LymphInputEventHandler<LKey>>();
			ReleaseEventsDict = new Dictionary<LKey, LymphInputEventHandler<LKey>>();

			SetupInitialBindings();

			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += new LymphInputEventHandler<KeyEvent>(OnKeyboardPressAnything);
			LKernel.GetG<InputMain>().OnKeyboardRelease_Anything += new LymphInputEventHandler<KeyEvent>(OnKeyboardReleaseAnything);
		}

		/// <summary>
		/// Set up some initial key bindings
		/// TODO read these from a file
		/// </summary>
		void SetupInitialBindings() {
			LKeysDict[LKey.Accelerate] = KeyCode.KC_W;
			LKeysDict[LKey.TurnLeft] = KeyCode.KC_A;
			LKeysDict[LKey.TurnRight] = KeyCode.KC_D;
			LKeysDict[LKey.Brake] = KeyCode.KC_SPACE;
			LKeysDict[LKey.Reverse] = KeyCode.KC_S;
			MOISKeysDict[KeyCode.KC_W] = LKey.Accelerate;
			MOISKeysDict[KeyCode.KC_A] = LKey.TurnLeft;
			MOISKeysDict[KeyCode.KC_D] = LKey.TurnRight;
			MOISKeysDict[KeyCode.KC_SPACE] = LKey.Brake;
			MOISKeysDict[KeyCode.KC_S] = LKey.Reverse;

			PressEventsDict.Add(LKey.Accelerate, null);
			PressEventsDict.Add(LKey.TurnLeft, null);
			PressEventsDict.Add(LKey.TurnRight, null);
			PressEventsDict.Add(LKey.Brake, null);
			PressEventsDict.Add(LKey.Reverse, null);

			ReleaseEventsDict.Add(LKey.Accelerate, null);
			ReleaseEventsDict.Add(LKey.TurnLeft, null);
			ReleaseEventsDict.Add(LKey.TurnRight, null);
			ReleaseEventsDict.Add(LKey.Brake, null);
			ReleaseEventsDict.Add(LKey.Reverse, null);
		}

		/// <summary>
		/// 
		/// </summary>
		void OnKeyboardPressAnything(KeyEvent ke) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			LKey key;
			if (MOISKeysDict.TryGetValue(ke.key, out key))
				Invoke(PressEventsDict[key], key);
		}

		/// <summary>
		/// 
		/// </summary>
		void OnKeyboardReleaseAnything(KeyEvent ke) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			LKey key;
			if (MOISKeysDict.TryGetValue(ke.key, out key))
				Invoke(ReleaseEventsDict[key], key);
		}

		/// <summary>
		/// Is the associated key pressed or not?
		/// </summary>
		/// <returns>Whether the key is pressed or not, or false if the input is currently swallowed.</returns>
		public bool IsKeyPressed(LKey key) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return false;

			return LKernel.GetG<InputMain>().InputKeyboard.IsKeyDown(LKeysDict[key]);
		}

		/// <summary>
		/// Changes the binding of "command" to "newKey"
		/// </summary>
		/// <param name="command"></param>
		/// <param name="newKey"></param>
		public void ChangeBinding(LKey command, KeyCode newKey) {
			LKeysDict[command] = newKey;
			MOISKeysDict[newKey] = command;
		}

		/// <summary>
		/// helper
		/// </summary>
		public void Invoke(LymphInputEventHandler<LKey> e, LKey eventArgs) {
			if (e != null)
				e.Invoke(eventArgs);
		}
	}
}
