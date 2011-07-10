// uncomment if you want all input to be printed
//#define PRINTINPUT

using System;
using Mogre;
using MOIS;
using Type = MOIS.Type;

namespace Lymph {
	/// <summary>
	/// Class that handles all of the input. This acts as a "layer" between the input library and the rest of the program.
	/// This makes it easier to change libraries and stuff, since you'd only need to change this class instead of everything
	/// that uses input.
	/// 
	/// This class does not do anything with the input besides fire off more events.
	/// 
	/// Other classes (mostly handlers) should only use events fired off from this class and not ones fired off from the
	/// input library.
	/// </summary>
	public class InputMain {
		public InputManager inputManager { get; private set; }
		public Keyboard inputKeyboard { get; private set; }
		public Mouse inputMouse { get; private set; }

		public InputMain() {
			Launch.Log("[Loading] Initialising MOIS input system");

			ParamList pl = new ParamList();
			IntPtr windowHnd;
			LKernel.Get<RenderWindow>().GetCustomAttribute("WINDOW", out windowHnd); // window is your RenderWindow!
			pl.Insert("WINDOW", windowHnd.ToString());
#if DEBUG
			// this stops MOIS from swallowing the mouse
			pl.Insert("w32_mouse", "DISCL_NONEXCLUSIVE");
			pl.Insert("w32_mouse", "DISCL_FOREGROUND");
#endif

			inputManager = InputManager.CreateInputSystem(pl);

			// Create all devices (except joystick, as most people have Keyboard/Mouse) using buffered input.
			inputKeyboard = (Keyboard)inputManager.CreateInputObject(Type.OISKeyboard, true);
			inputMouse = (Mouse)inputManager.CreateInputObject(Type.OISMouse, true);

			// sets the mouseState initial width and height (default is too low)
			MouseState_NativePtr mouseState = inputMouse.MouseState;
			mouseState.width = LKernel.Get<Viewport>().ActualWidth;
			mouseState.height = LKernel.Get<Viewport>().ActualHeight;

			LKernel.Get<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			CreateEventHandlers();

			Launch.Log("[Loading] MOIS input system loaded!");
		}

		/// <summary>
		/// Hook up to MOIS' event handlers
		/// </summary>
		private void CreateEventHandlers() {
			if (inputKeyboard != null) {
				Launch.Log("[Loading] Setting up keyboard listeners");
				inputKeyboard.KeyPressed += new KeyListener.KeyPressedHandler(KeyPressed);
				inputKeyboard.KeyReleased += new KeyListener.KeyReleasedHandler(KeyReleased);
			}
			if (inputMouse != null) {
				Launch.Log("[Loading] Setting up mouse listeners");
				inputMouse.MousePressed += new MouseListener.MousePressedHandler(MousePressed);
				inputMouse.MouseReleased += new MouseListener.MouseReleasedHandler(MouseReleased);
				inputMouse.MouseMoved += new MouseListener.MouseMovedHandler(MouseMotion);
			}
		}

		// ============================================================


		float timeSinceLastFrame = 0;

		bool FrameStarted(FrameEvent e) {
			if (!LKernel.Get<Levels.LevelManager>().IsValidLevel)
				return true;

			timeSinceLastFrame += e.timeSinceLastFrame;
			if (timeSinceLastFrame >= Constants.INPUT_CAPTURE_RATE)
			{
				// Capture all key presses since last check.
				inputKeyboard.Capture();
				// Capture all mouse movements and button presses since last check.
				inputMouse.Capture();
				timeSinceLastFrame = 0;
			}

			return true;
		}

		// ============================================================

		#region Event firing helpers
		/// <summary>
		/// Fires an event. Helper method so I don't have to check every single event for null.
		/// </summary>
		/// <typeparam name="T">The parameter type of the event</typeparam>
		/// <param name="handler">The event handler</param>
		/// <param name="eventArgs">The parameter of the event</param>
		void FireEvent<T>(LymphInputEventHandler<T> handler, T eventArgs) {
			if (handler != null)
				handler(eventArgs);
		}

		/// <summary>
		/// Fires an event. Helper method so I don't have to check every single event for null.
		/// </summary>
		/// <typeparam name="T">The first parameter type of the event</typeparam>
		/// <typeparam name="U">The second parameter type of the event</typeparam>
		/// <param name="handler">The event handler</param>
		/// <param name="eventArg1">The first parameter of the event</param>
		/// <param name="eventArg2">The second parameter of the event</param>
		void FireEvent<T, U>(LymphInputEventHandler<T, U> handler, T eventArg1, U eventArg2) {
			if (handler != null)
				handler(eventArg1, eventArg2);
		}
		#endregion

		/// <summary>
		/// Handles key pressing and fires appropriate events
		/// </summary>
		bool KeyPressed(KeyEvent ke) {
#if PRINTINPUT
			Console.WriteLine("Pressed: " + ke.key);
#endif
			switch (ke.key) {
				case KeyCode.KC_W:
				case KeyCode.KC_UP:
					FireEvent<KeyEvent>(OnKeyboardPress_Up, ke); break;
				case KeyCode.KC_S:
				case KeyCode.KC_DOWN:
					FireEvent<KeyEvent>(OnKeyboardPress_Down, ke); break;
				case KeyCode.KC_A:
				case KeyCode.KC_LEFT:
					FireEvent<KeyEvent>(OnKeyboardPress_Left, ke); break;
				case KeyCode.KC_D:
				case KeyCode.KC_RIGHT:
					FireEvent<KeyEvent>(OnKeyboardPress_Right, ke); break;
				case KeyCode.KC_ESCAPE:
					FireEvent<KeyEvent>(OnKeyboardPress_Escape, ke); break;
			}
			FireEvent<KeyEvent>(OnKeyboardPress_Anything, ke);
			return true;
		}
		
		/// <summary>
		/// Handles key releasing and fires appropriate events
		/// </summary>
		bool KeyReleased(KeyEvent ke) {
#if PRINTINPUT
			Console.WriteLine("Released: " + ke.key);
#endif
			switch (ke.key) {
				case KeyCode.KC_W:
				case KeyCode.KC_UP:
					FireEvent<KeyEvent>(OnKeyboardRelease_Up, ke); break;
				case KeyCode.KC_S:
				case KeyCode.KC_DOWN:
					FireEvent<KeyEvent>(OnKeyboardRelease_Down, ke); break;
				case KeyCode.KC_A:
				case KeyCode.KC_LEFT:
					FireEvent<KeyEvent>(OnKeyboardRelease_Left, ke); break;
				case KeyCode.KC_D:
				case KeyCode.KC_RIGHT:
					FireEvent<KeyEvent>(OnKeyboardRelease_Right, ke); break;
			}
			return true;
		}

		/// <summary>
		/// Handles mouse pressing and fires appropriate events
		/// </summary>
		bool MousePressed(MouseEvent me, MouseButtonID id) {
#if PRINTINPUT
			Console.WriteLine("Mouse " + id + " pressed");
#endif
			switch (id) {
				case MouseButtonID.MB_Left:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Left, me, id); break;
				case MouseButtonID.MB_Right:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Right, me, id); break;
				case MouseButtonID.MB_Middle:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Middle, me, id); break;
			}
			return true;

		}

		/// <summary>
		/// Handles mouse releasing and fires appropriate events
		/// </summary>
		bool MouseReleased(MouseEvent me, MouseButtonID id) {
#if PRINTINPUT
			Console.WriteLine("Mouse " + id + " released");
#endif
			switch (id) {
				case MouseButtonID.MB_Left:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Left, me, id); break;
				case MouseButtonID.MB_Right:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Right, me, id); break;
				case MouseButtonID.MB_Middle:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Middle, me, id); break;
			}
			return true;
		}

		/// <summary>
		/// Handles mouse movement and fires appropriate events
		/// - scroll wheel counts as a movement rather than a press
		/// </summary>
		bool MouseMotion(MouseEvent me) {
			// you can use handler.state.Y.rel for relative position, and handler.state.Y.abs for absolute
#if PRINTINPUT
			Console.WriteLine("Mouse moved: x " + me.state.X.rel + " | y " + me.state.Y.rel);
#endif
			FireEvent<MouseEvent>(OnMouseMove, me);
			return true;
		}

		// =========================================================

		/// <summary>
		/// Is the selected key pressed?
		/// </summary>
		/// <param name="key">The key to check</param>
		/// <returns>True if the key is pressed, false otherwise</returns>
		public bool IsKeyDown(KeyCode key) {
			return inputKeyboard.IsKeyDown(key);
		}

		#region Events
		/// <summary> When any keyboard button is pressed. This should eventually be removed once we know what all of the keys are. </summary>
		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Anything;

		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Up;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardRelease_Up;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Down;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardRelease_Down;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Left;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardRelease_Left;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Right;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardRelease_Right;
		public event LymphInputEventHandler<KeyEvent> OnKeyboardPress_Escape;

		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMousePress_Left;
		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMouseRelease_Left;
		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMousePress_Right;
		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMouseRelease_Right;
		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMousePress_Middle;
		public event LymphInputEventHandler<MouseEvent, MouseButtonID> OnMouseRelease_Middle;

		public event LymphInputEventHandler<MouseEvent> OnMouseMove;
		#endregion
	}

	public delegate void LymphInputEventHandler<T>(T eventArgs);
	public delegate void LymphInputEventHandler<T, U>(T eventArg1, U eventArg2);

	// just dumping this in here. It's the code to rotate the "face" to wherever the mouse is pointing

	/*if (!LKernel.Get<LevelManager>().IsValidLevel)
		return;
	if (handler.X != lastPosition.X && handler.Y != lastPosition.Y) {
		float differenceX = handler.X - LKernel.Get<RenderWindow>().Width / 2f;
		float differenceZ = handler.Y - LKernel.Get<RenderWindow>().Height / 2f;

		float x1 = 0, y1 = 1, x2 = differenceX, y2 = differenceZ;
		double n1 = Math.Sqrt((x1 * x1) + (y1 * y1)), n2 = Math.Sqrt((x2 * x2) + (y2 * y2));
		float angle = (float)(Math.Acos(((x1 * x2) + (y1 * y2)) / (n1 * n2)) * -Math.Sign(differenceX));
		if (!float.IsNaN(angle) && !float.IsInfinity(angle))
			// You have to check these for some reason because if you set the rotation of a node to NaN then it just disappears. :/
			LKernel.Get<Player>().Face.FaceRotation = angle;
		lastPosition.X = handler.X;
		lastPosition.Y = handler.Y;
	}*/
}
