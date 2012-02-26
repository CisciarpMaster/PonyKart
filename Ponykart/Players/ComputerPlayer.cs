using PonykartParsers;
using Ponykart.Levels;
using Mogre;
using System;
using System.Collections.Generic;
using Ponykart.Core;
using System.IO;

namespace Ponykart.Players {
    public class ComputerPlayer : Player
    {
        const float DecelThreshold = 110.0f;
        const float WaypointThreshold = 100.0f;
        private int loop = 0;
        private Player Human;
        private int currWaypoint = 0;
        private List<Vector3> Waypoints = new List<Vector3>();

        private StreamWriter outfile = new StreamWriter("./waypoints.txt");

        public ComputerPlayer(LevelChangedEventArgs eventArgs, int id) : base(eventArgs, id)
        {
            Human = LKernel.GetG<PlayerManager>().Players[0];
            LKernel.GetG<Root>().FrameEnded += FrameEnded;
            LKernel.Get<Spawner>().Spawn("Axis", new Vector3(0, 0, 100));
            //Waypoints.Add(new Vector3(200, 0, 200));
            //Waypoints.Add(new Vector3(200, 0, -200));
            //Waypoints.Add(new Vector3(-200, 0, -200));
            //Waypoints.Add(new Vector3(-200, 0, 200));

            //Dear David. THIS NEXT LINE IS BAD AND YOU SHOULD FEEL BAD. Sincerely, David.
            StreamReader infile = new StreamReader("../../../saa_r1 (2).waypoint");

            string line;
            string[] tempStr;
            while ((line = infile.ReadLine()) != null)
            {
                tempStr = line.Split(' ');
                Waypoints.Add(new Vector3(float.Parse(tempStr[0]), float.Parse(tempStr[1]), float.Parse(tempStr[2])));
            }     
        }

        bool FrameEnded(FrameEvent evt)
        {
            // use LKernel.GetG<LevelManager>().CurrentLevel.Definition.Get__Property() to retrieve your waypoints
            loop++;
            if (loop % 10 == 0)
            {
                string tmp;
                tmp = Human.NodePosition.x + " " + Human.NodePosition.y + " " + Human.NodePosition.z;
                //outfile.WriteLine(tmp);
            }
            Vector3 target = Waypoints[currWaypoint];
            Vector3 vecToTar = target - NodePosition;
            double distToTar = vecToTar.Length;
            Kart.TurnMultiplier = SteerTowards(target);
            if (distToTar > DecelThreshold)
                Kart.Acceleration = 0.5f;
            else
                Kart.Acceleration = 0.1f;
            if (distToTar < WaypointThreshold)
            {
                currWaypoint++;
                currWaypoint = currWaypoint % Waypoints.Count;
            }

            return true;
        }

        private float SteerTowards(Vector3 target)
        {
           Vector3 xaxis = Kart.Body.Orientation.XAxis;
           Vector3 vecToTar = target - NodePosition;

            xaxis.Normalise();
          vecToTar.Normalise();

            float result = xaxis.DotProduct(vecToTar);
            return result;
        }

        public override void Detach()
        {
            LKernel.GetG<Root>().FrameEnded -= FrameEnded;
            outfile.Close();
            base.Detach();
        }

        protected override void UseItem() {
            throw new System.NotImplementedException();
        }
    }
}
