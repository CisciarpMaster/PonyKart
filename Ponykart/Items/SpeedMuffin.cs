using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ponykart.Players;
using Mogre;
using Ponykart.Sound;
using System.Timers;

namespace Ponykart.Items
{
    class SpeedMuffin : Item
    {
        private Vector3 origin;
        private System.Timers.Timer endTimer;
        //Player User;
        public SpeedMuffin(ref Player user) : base(ref user, "SpeedMuffin")
        {
            origin = user.NodePosition;
            endTimer = new System.Timers.Timer(2000);
            endTimer.Elapsed += new ElapsedEventHandler(OnEnd);
            //User = user;
        }

        protected override void OnUse()
        {
            base.OnUse();
            LKernel.GetG<SoundMain>().Play3D("Apple Firing.mp3", origin, false);
            User.Kart.MaxSpeed *= 1.25f;
            User.Kart.Acceleration *= 1.25f;
        }

        protected override void EveryTenth(object o)
        {
            base.EveryTenth(o);
        }
        protected void OnEnd(object source, ElapsedEventArgs e)
        {
            User.Kart.MaxSpeed /= 1.25f;
            User.Kart.Acceleration /= 1.25f;
        }
    }
}
