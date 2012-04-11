using System;
using Ponykart.Levels;
using WiimoteLib;
using Ponykart.Core;

namespace Ponykart.Players
{
	public class WiiMotePlayer : Player
	{
		private readonly Wiimote _wiimote = new Wiimote();
		public WiiMotePlayer(LevelChangedEventArgs eventArgs, int id)
			: base(eventArgs, id, false) {
			_wiimote.Connect();
			_wiimote.SetLEDs(id + 1);
			_wiimote.WiimoteChanged += wiimote_WiimoteChanged;
		}

		private void wiimote_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
		{
			Kart.Acceleration = e.WiimoteState.ButtonState.A ? 1 : 0;
			Kart.Acceleration -= e.WiimoteState.ButtonState.B ? 1 : 0;
			Kart.TurnMultiplier = -e.WiimoteState.NunchukState.Joystick.X * 2;
		}

		protected override void UseItem()
		{
			throw new NotImplementedException();
		}
	}
}
