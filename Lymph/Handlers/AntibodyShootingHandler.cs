using System;
using Lymph.Actors;
using Lymph.Core;
using Lymph.Stuff;
using MOIS;
using Vector3 = Mogre.Vector3;

namespace Lymph.Handlers {
	/// <summary>
	/// Simple class to handle antibody shooting.
	/// </summary>
	public class AntibodyShootingHandler : IDisposable
	{

		/// <summary>
		/// hook up to events
		/// </summary>
		public AntibodyShootingHandler()
		{
			Launch.Log("[Loading] Creating AntibodyShootingHandler");
			var input = LKernel.Get<InputMain>();
			input.OnMousePress_Left += OnMousePress;
			input.OnMousePress_Right += OnMousePress;
		}

		/// <summary>
		/// disconnect from events
		/// </summary>
		public void Dispose()
		{
			Launch.Log("[Loading] Disposing of AntibodyShootingHandler");
			var input = LKernel.Get<InputMain>();
			input.OnMousePress_Left -= OnMousePress;
			input.OnMousePress_Right -= OnMousePress;
		}

		/// <summary>
		/// fires whenever the left or right mouse button is pressed
		/// </summary>
		/// <param name="me">the event</param>
		/// <param name="id">which button was pressed</param>
		void OnMousePress(MouseEvent me, MouseButtonID id)
		{
			// is the input swallowed?
			if (LKernel.Get<InputSwallowerManager>().IsSwallowed())
				return;

			// is that point obstructed by a UI element?
			bool isObstructed = LKernel.Get<UI.UIMain>().IsPointObstructed(me.state.X.abs, me.state.Y.abs);
			if (isObstructed)
				return;

			// first figure out where to shoot it
			float differenceX = (me.state.X.abs - LKernel.Get<Main>().Width / 2f);
			float differenceZ = (me.state.Y.abs - LKernel.Get<Main>().Height / 2f);
			Vector3 vec3 = new Vector3(differenceX, 0, differenceZ);

			// Get the antibody's appropriate colour
			Antibody ant;
			AntigenColour colour;
			Player player = LKernel.Get<Player>();

			// figure out which button was pressed and set the colour appropriately
			if (id == MouseButtonID.MB_Left && player.AntibodyColourLeft != null)
				colour = player.AntibodyColourLeft;
			else if (id == MouseButtonID.MB_Right && player.AntibodyColourRight != null)
				colour = player.AntibodyColourRight;
			else
				// No antibody color selected for that button
				return;

			// Create the antibody
			ant = LKernel.Get<Spawner>().Spawn(ThingEnum.Antibody, "Antibody", LKernel.Get<Player>().Node.Position) as Antibody;
			// And launch it!
			ant.Actor.AddForce(vec3 * ant.MoveSpeed, Mogre.PhysX.ForceModes.Force);
		}
	}
}
