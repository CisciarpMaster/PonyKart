using System;
using MOIS;
using Vector3 = Mogre.Vector3;

namespace Lymph.Handlers.NotReallyHandlers
{
	/// <summary>
	/// This handler gets keyboard events for movement and then sets MovementVector appropriately.
	/// MovementVector should be multiplied with Player.MoveSpeed before using it.
	/// 
	/// Technically this isn't a handler as it's "managed" by PlayerMovementHandler.
	/// 
	/// Don't bind this to the kernel!
	/// </summary>
	public class PlayerMovementKeyboardHandler : IDisposable
	{

		float xMovement = 0;
		float zMovement = 0;
		/// <summary>
		/// This should be multiplied with Player.MoveSpeed before using.
		/// </summary>
		public Vector3 MovementVector
		{
			get { return new Vector3(xMovement, 0 , zMovement); } 
		}

		public PlayerMovementKeyboardHandler(InputMain input) {
			Launch.Log("[Loading] Creating PlayerMovementKeyboardHandler");

			input.OnKeyboardPress_Up += OnKeyboardPress_Up;
			input.OnKeyboardPress_Down += OnKeyboardPress_Down;
			input.OnKeyboardPress_Left += OnKeyboardPress_Left;
			input.OnKeyboardPress_Right += OnKeyboardPress_Right;
			input.OnKeyboardRelease_Up += OnKeyboardRelease_Up;
			input.OnKeyboardRelease_Down += OnKeyboardRelease_Down;
			input.OnKeyboardRelease_Left += OnKeyboardRelease_Left;
			input.OnKeyboardRelease_Right += OnKeyboardRelease_Right;
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing PlayerMovementKeyboardHandler");

			var input = LKernel.Get<InputMain>();
			input.OnKeyboardPress_Up -= OnKeyboardPress_Up;
			input.OnKeyboardPress_Down -= OnKeyboardPress_Down;
			input.OnKeyboardPress_Left -= OnKeyboardPress_Left;
			input.OnKeyboardPress_Right -= OnKeyboardPress_Right;
			input.OnKeyboardRelease_Up -= OnKeyboardRelease_Up;
			input.OnKeyboardRelease_Down -= OnKeyboardRelease_Down;
			input.OnKeyboardRelease_Left -= OnKeyboardRelease_Left;
			input.OnKeyboardRelease_Right -= OnKeyboardRelease_Right;
		}

		/*
		 * How this works:
		 * if we are currently moving left, pressing right will cancel it out (hence the 0).
		 * If we are currently not moving, pressing right will make us move right.
		 * We can't just do += 1 because what if we press both D and Right at the same time?
		 * 
		 * X: left -, right +
		 * Z: up -, down +
		 */

		void OnKeyboardRelease_Right(KeyEvent eventArgs) {
			// if we are currently going right, stop
			if (xMovement == 1)
				xMovement = 0;
			// if we aren't moving, check whether the opposite key is pressed
			else if (xMovement == 0) {
				if (LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_LEFT) || LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_A))
					xMovement = -1;
				else
					xMovement = 0;
			// otherwise keep going left
			} else
				xMovement = -1;
		}

		void OnKeyboardRelease_Left(KeyEvent eventArgs) {
			// if we are currently going left, stop
			if (xMovement == -1)
				xMovement = 0;
			// if we aren't moving, check whether the opposite key is pressed
			else if (xMovement == 0) {
				if (LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_RIGHT) || LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_D))
					xMovement = 1;
				else
					xMovement = 0;
				// otherwise keep going right
			} else
				xMovement = 1;
		}

		void OnKeyboardRelease_Down(KeyEvent eventArgs) {
			// if we are currently going down, stop
			if (zMovement == 1)
				zMovement = 0;
			// if we aren't moving, check whether the opposite key is pressed
			else if (zMovement == 0) {
				if (LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_UP) || LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_W))
					zMovement = -1;
				else
					zMovement = 0;
				// otherwise keep going up
			} else
				zMovement = -1;
		}

		void OnKeyboardRelease_Up(KeyEvent eventArgs) {
			// if we are currently going left, stop
			if (zMovement == -1)
				zMovement = 0;
			// if we aren't moving, check whether the opposite key is pressed
			else if (zMovement == 0) {
				if (LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_DOWN) || LKernel.Get<InputMain>().IsKeyDown(KeyCode.KC_S))
					zMovement = 1;
				else
					zMovement = 0;
				// otherwise keep going right
			} else
				zMovement = 1;
		}

		// ========================================================

		void OnKeyboardPress_Right(KeyEvent eventArgs) {
			xMovement = xMovement == -1 ? 0 : 1;
		}

		void OnKeyboardPress_Left(KeyEvent eventArgs) {
			xMovement = xMovement == 1 ? 0 : -1;
		}

		void OnKeyboardPress_Down(KeyEvent eventArgs) {
			zMovement = zMovement == -1 ? 0 : 1;
		}

		void OnKeyboardPress_Up(KeyEvent eventArgs) {
			zMovement = zMovement == 1 ? 0 : -1;
		}
	}
}
