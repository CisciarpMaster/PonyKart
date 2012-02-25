using PonykartParsers;
using Ponykart.Levels;
using Mogre;
using System;
using System.Collections.Generic;

namespace Ponykart.Players {
    public class ComputerPlayer : Player
    {
        private Player Human;
        private LinkedList<Vector3> Waypoints;
        public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id)
        {
            Human = LKernel.GetG<PlayerManager>().Players[0];
            LKernel.GetG<Root>().FrameEnded += FrameEnded;
            //Waypoints.AddLast(new Vector3(100, 0, 100));
        }

        bool FrameEnded(FrameEvent evt)
        {
            // use LKernel.GetG<LevelManager>().CurrentLevel.Definition.Get__Property() to retrieve your waypoints
            Kart.TurnMultiplier = SteerTowards(new Vector3(10, 0, 10));
            Kart.Acceleration = 0.05f;
            return true;
        }

        private float SteerTowards(Vector3 target)
        {
            float steer = 0.0f;
            Vector3 pos = NodePosition;
            Vector3 facing = Kart.Body.Orientation.ZAxis;
            Vector3 vecToTar = target - NodePosition;
            // Math.ATan2 probably plays into the solution to this problem
            if (vecToTar.x < 0.0)
                steer = 0.5f;
            if (vecToTar.x > 0.0)
                steer = -0.5f;
            return steer;
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
