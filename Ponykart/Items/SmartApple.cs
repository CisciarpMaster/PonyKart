﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ponykart.Players;
using Mogre;
namespace Ponykart.Items
{
    class SmartApple : Item
    {
        private PlayerManager playerManager;
        private Player target;
        private float age = 0;
        bool isActive = true;
        private float HomingSpeed = 100;

        public SmartApple(Player user)
            : base(user, "SmartApple")
        {
            playerManager = LKernel.GetG<PlayerManager>();
        }

        protected override void OnUse()
        {
            base.OnUse();

            //Initial velocity: Forward at player speed + 10, with a good bit of up.
            //Vector3 itemVel = User.Kart.Vehicle.ForwardVector;
            //itemVel *= User.Kart.VehicleSpeed + 10.0f;
            //itemVel.y = 200.0f;
            //Body.Body.ApplyCentralImpulse(itemVel);


            Vector3 itemVel = User.Kart.Body.LinearVelocity * 1.2f;       // using a multiplier is better, because if you add, the apple will go off at a weird angle sometimes
            itemVel.y = 200f;
            Body.Body.ApplyCentralImpulse(itemVel);

            Vector3 vecToPlayer;
            float leastLength = 0;
            foreach (Player p in LKernel.GetG<PlayerManager>().Players)
            {
                if (p != User)
                {
                    vecToPlayer = Body.RootNode.Position - p.Kart.ActualPosition;
                    //first kart is selected as initial target
                    if (target == null)
                    {
                        target = p;
                        leastLength = vecToPlayer.SquaredLength;
                    }
                    else
                    {
                        if (vecToPlayer.SquaredLength < leastLength)
                        {
                            leastLength = vecToPlayer.SquaredLength;
                            target = p;
                        }
                    }

                }
            }
        }

        protected override void EveryTenth(object o)
        {
            base.EveryTenth(o);
            if (isActive)
            {
                age += 0.1f;
                Vector3 vecToTarget = Body.RootNode.Position - target.Kart.ActualPosition;
                vecToTarget.Normalise();
                vecToTarget *= -1;
                if (Body.Body.LinearVelocity.Length < 30.0f)
                    vecToTarget *= HomingSpeed;
                vecToTarget.y = 0.0f;

               
                //vecToTarget.y += Body.Body.Gravity.y;
                vecToTarget *= 1.5f;
                Body.Body.ApplyCentralImpulse(vecToTarget);

                //after 10 seconds, destroy object
                if (age > 10.0f)
                {
                    isActive = false;
                    Dispose(true);
                }
            }
        }
    }
}
