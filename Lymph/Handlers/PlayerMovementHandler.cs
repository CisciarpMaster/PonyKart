using System;
using Ponykart.Actors;
using Ponykart.Handlers.NotReallyHandlers;
using Mogre;

namespace Ponykart.Handlers
{
	/// <summary>
	/// This class manages stuff to do with player movement, such as responding to WASD key presses and applying forces to the player.
	/// </summary>
	public class PlayerMovementHandler : IDisposable
	{
		// This handles the keyboard events
		private PlayerMovementKeyboardHandler keyboardHandler;

		public PlayerMovementHandler()
		{
			Launch.Log("[Loading] Creating PlayerMovementHandler");

			keyboardHandler = new PlayerMovementKeyboardHandler(LKernel.Get<InputMain>());

			LKernel.Get<Root>().FrameStarted += FrameStarted;
		}

		public void Dispose() {
			Launch.Log("[Loading] Disposing PlayerMovementHandler");

			LKernel.Get<Root>().FrameStarted -= FrameStarted;
			keyboardHandler.Dispose();
		}

		/// <summary>
		/// Apply a force every frame (can probably reduce this) to the player
		/// </summary>
		bool FrameStarted(FrameEvent evt)
		{
			// if someone is typing something into a text box, we don't want to move around!
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed())
				return true;

			Player player = LKernel.Get<Player>();
			if (player != null) {
				player.Actor.AddForceAtLocalPos(keyboardHandler.MovementVector * player.MoveSpeed, Vector3.ZERO);
			}

			return true;
		}
	}
}
