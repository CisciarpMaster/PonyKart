using PonykartParsers;
using Ponykart.Levels;
using Mogre;
using System;
using System.Collections.Generic;
using Ponykart.Core;
namespace Ponykart.Players {
    public class ComputerPlayer : Player
    {
        const float DecelThreshold = 10.0f;
        private Player Human;
        private LinkedList<Vector3> Waypoints;
        public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id)
        {
            Human = LKernel.GetG<PlayerManager>().Players[0];
            LKernel.GetG<Root>().FrameEnded += FrameEnded;
            LKernel.Get<Spawner>().Spawn("Axis", new Vector3(100, 0, 0));
            //Waypoints.AddLast(new Vector3(100, 0, 100));
        }

        bool FrameEnded(FrameEvent evt)
        {
            // use LKernel.GetG<LevelManager>().CurrentLevel.Definition.Get__Property() to retrieve your waypoints
            Vector3 target = new Vector3(0, 0, 50);
            Vector3 vecToTar = NodePosition - target;
            float distToTar = vecToTar.Length;
            Kart.TurnMultiplier = SteerTowards(target);
            if (distToTar > DecelThreshold)
                Kart.Acceleration = 1.0f;
            else
                Kart.Acceleration *= (1-(distToTar-DecelThreshold)); //slow down on approach  
            return true;
        }

        private float SteerTowards(Vector3 target)
        {
           Vector3 xaxis = Kart.Body.Orientation.XAxis;
           Vector3 vecToTar = target - NodePosition;

            xaxis.Normalise();
          vecToTar.Normalise();

            float result = xaxis.DotProduct(vecToTar);

   if (result > 0f)
    return 1.0f;
   else if (result < 0f)
    return -1.0f;
            return 0;
        }

        public override void Detach()
        {
            LKernel.GetG<Root>().FrameEnded -= FrameEnded;

            base.Detach();
        }

        protected override void UseItem() {
            throw new System.NotImplementedException();
        }
    }
}
