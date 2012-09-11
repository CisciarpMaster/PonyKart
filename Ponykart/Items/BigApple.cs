using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ponykart.Players;

namespace Ponykart.Items
{
    class BigApple : Item
    {
        public BigApple(Player user) : base(user, "BigApple")
        {
            
        }

        protected override void OnUse()
        {
            base.OnUse();

            Mogre.Vector3 itemVel = User.Kart.Vehicle.ForwardVector;
            itemVel *= User.Kart.VehicleSpeed + 1000.0f;
            itemVel.y = 300.0f;
            Body.Body.ApplyCentralImpulse(itemVel);
        }

        protected override void EveryTenth(object o)
        {
            base.EveryTenth(o);
        }
    }
}
