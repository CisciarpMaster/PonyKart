using System.IO;
using Mogre;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.UI;

namespace Ponykart.Players
{
    public class HumanPlayer : Player
    {
        KeyBindingManager bindings;
        private bool cheering = false;
        private Derpy myDerpy;

        public HumanPlayer(LevelChangedEventArgs eventArgs, int id)
            : base(eventArgs, id, false)
        {
            Vector3 derpyPos = Kart.ActualPosition;
            myDerpy = LKernel.GetG<Spawner>().Spawn<Derpy>("Derpy", derpyPos, (t, d) => new Derpy(t, d));
            myDerpy.ChangeAnimation("Forward1");
            myDerpy.AttachToKart(new Vector3(-3f, 1.5f, 3f), Kart);
            this.laps = 0;
            // hook up to input events
            bindings = LKernel.Get<KeyBindingManager>();

            bindings.PressEventsDict[LKey.Accelerate] += OnStartAccelerate;
            bindings.ReleaseEventsDict[LKey.Accelerate] += OnStopAccelerate;
            bindings.PressEventsDict[LKey.Drift] += OnStartDrift;
            bindings.ReleaseEventsDict[LKey.Drift] += OnStopDrift;
            bindings.PressEventsDict[LKey.Reverse] += OnStartReverse;
            bindings.ReleaseEventsDict[LKey.Reverse] += OnStopReverse;
            bindings.PressEventsDict[LKey.TurnLeft] += OnStartTurnLeft;
            bindings.ReleaseEventsDict[LKey.TurnLeft] += OnStopTurnLeft;
            bindings.PressEventsDict[LKey.TurnRight] += OnStartTurnRight;
            bindings.ReleaseEventsDict[LKey.TurnRight] += OnStopTurnRight;
            //bindings.AxisEvents[LKey.SteeringAxis] += OnSteeringAxisMoved;
            //bindings.AxisEvents[LKey.AccelerateAxis] += OnAccelerateAxisMoved;

            //hook logic loop
            Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;
        }

        public TriggerRegion CurrentRegion { get; private set; }

        public override bool IsControlEnabled
        {
            get
            {
                return base.IsControlEnabled;
            }
            // we need to do this so we can immediately start moving when we regain control without having to repress keys
            set
            {
                base.IsControlEnabled = value;

                if (value)
                {
                    // gain control
                    if (bindings.IsKeyPressed(LKey.Accelerate))
                        OnStartAccelerate();
                    if (bindings.IsKeyPressed(LKey.Drift))
                        OnStartDrift();
                    if (bindings.IsKeyPressed(LKey.Reverse))
                        OnStartReverse();
                    if (bindings.IsKeyPressed(LKey.TurnLeft))
                        OnStartTurnLeft();
                    if (bindings.IsKeyPressed(LKey.TurnRight))
                        OnStartTurnRight();
                }
                else
                {
                    // lose control
                    if (bindings.IsKeyPressed(LKey.Accelerate))
                        OnStopAccelerate();
                    if (bindings.IsKeyPressed(LKey.Drift))
                        OnStopDrift();
                    if (bindings.IsKeyPressed(LKey.Reverse))
                        OnStopReverse();
                    if (bindings.IsKeyPressed(LKey.TurnLeft))
                        OnStopTurnLeft();
                    if (bindings.IsKeyPressed(LKey.TurnRight))
                        OnStopTurnRight();
                }
            }
        }

        public System.Action OnAccelerateAxisMoved { get; set; }

        public System.Action OnSteeringAxisMoved { get; set; }

        public override void Detach()
        {
            bindings.PressEventsDict[LKey.Accelerate] -= OnStartAccelerate;
            bindings.ReleaseEventsDict[LKey.Accelerate] -= OnStopAccelerate;
            bindings.PressEventsDict[LKey.Drift] -= OnStartDrift;
            bindings.ReleaseEventsDict[LKey.Drift] -= OnStopDrift;
            bindings.PressEventsDict[LKey.Reverse] -= OnStartReverse;
            bindings.ReleaseEventsDict[LKey.Reverse] -= OnStopReverse;
            bindings.PressEventsDict[LKey.TurnLeft] -= OnStartTurnLeft;
            bindings.ReleaseEventsDict[LKey.TurnLeft] -= OnStopTurnLeft;
            bindings.PressEventsDict[LKey.TurnRight] -= OnStartTurnRight;
            bindings.ReleaseEventsDict[LKey.TurnRight] -= OnStopTurnRight;
            Launch.OnEveryUnpausedTenthOfASecondEvent -= EveryTenth;
            base.Detach();
        }

        protected override void OnStartAccelerate()
        {
            base.OnStartAccelerate();

            if (IsControlEnabled)
            {
                // if we have both forward and reverse pressed at the same time, do nothing
                if (bindings.IsKeyPressed(LKey.Reverse))
                    Kart.Acceleration = 0f;
                // otherwise go forwards as normal
                else
                    Kart.Acceleration = 1f;
            }
        }

        protected override void OnStartDrift()
        {
            base.OnStartDrift();

            if (IsControlEnabled)
            {
                // if left is pressed and right isn't, start drifting left
                if (bindings.IsKeyPressed(LKey.TurnLeft) && !bindings.IsKeyPressed(LKey.TurnRight))
                {
                    Kart.StartDrifting(KartDriftState.StartRight);
                }
                // otherwise if right is pressed and left isn't, start drifting right
                else if (bindings.IsKeyPressed(LKey.TurnRight) && !bindings.IsKeyPressed(LKey.TurnLeft))
                {
                    Kart.StartDrifting(KartDriftState.StartLeft);
                }
                // otherwise it wants to drift but we don't have a direction yet
                else if (Kart.VehicleSpeed > 20f)
                {
                    Kart.DriftState = KartDriftState.WantsDriftingButNotTurning;
                }
            }
        }

        protected override void OnStartReverse()
        {
            base.OnStartReverse();

            if (IsControlEnabled)
            {
                // if we have both forward and reverse pressed at the same time, do nothing
                if (bindings.IsKeyPressed(LKey.Accelerate))
                    Kart.Acceleration = 0f;
                // otherwise go forwards as normal
                else
                    Kart.Acceleration = -1f;
            }
        }

        protected override void OnStartTurnLeft()
        {
            base.OnStartTurnLeft();

            if (IsControlEnabled)
            {
                // if we're waiting to drift
                if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning)
                {
                    Kart.StartDrifting(KartDriftState.StartRight);
                }
                // normal steering
                else
                {
                    // if both turns are pressed, we go straight
                    if (bindings.IsKeyPressed(LKey.TurnRight))
                        Kart.TurnMultiplier = 0f;
                    // otherwise go left
                    else
                        Kart.TurnMultiplier = 1f;
                }
            }
        }

        protected override void OnStartTurnRight()
        {
            base.OnStartTurnRight();

            if (IsControlEnabled)
            {
                if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning)
                {
                    Kart.StartDrifting(KartDriftState.StartLeft);
                }
                // normal steering
                else
                {
                    // if both turns are pressed, we go straight
                    if (bindings.IsKeyPressed(LKey.TurnLeft))
                        Kart.TurnMultiplier = 0f;
                    // otherwise go right
                    else
                        Kart.TurnMultiplier = -1f;
                }
            }
        }

        protected override void OnStopAccelerate()
        {
            base.OnStopAccelerate();

            if (IsControlEnabled)
            {
                // if reverse is still held down, then we start reversing
                if (bindings.IsKeyPressed(LKey.Reverse))
                    Kart.Acceleration = -1f;
                // otherwise we just stop accelerating
                else
                    Kart.Acceleration = 0f;
            }
        }

        /// <summary>
        /// cancel the drift
        /// </summary>
        protected override void OnStopDrift()
        {
            base.OnStopDrift();

            if (IsControlEnabled)
            {
                // if we were drifting left
                if (Kart.DriftState == KartDriftState.FullLeft || Kart.DriftState == KartDriftState.StartLeft)
                {
                    Kart.StopDrifting();
                }
                // if we were drifting right
                else if (Kart.DriftState == KartDriftState.FullRight || Kart.DriftState == KartDriftState.StartRight)
                {
                    Kart.StopDrifting();
                }
                // if we had the drift button down but weren't actually drifting
                else if (Kart.DriftState == KartDriftState.WantsDriftingButNotTurning)
                {
                    Kart.DriftState = KartDriftState.None;
                }
            }
        }

        protected override void OnStopReverse()
        {
            base.OnStopReverse();

            if (IsControlEnabled)
            {
                // if forward is still held down, then we start going forwards
                if (bindings.IsKeyPressed(LKey.Accelerate))
                    Kart.Acceleration = 1f;
                // otherwise we just stop accelerating
                else
                    Kart.Acceleration = 0f;
            }
        }

        protected override void OnStopTurnLeft()
        {
            base.OnStopTurnLeft();

            if (IsControlEnabled)
            {
                // if right is still pressed, turn right
                if (bindings.IsKeyPressed(LKey.TurnRight))
                    Kart.TurnMultiplier = -0.3f;
                // otherwise go straight
                else
                    Kart.TurnMultiplier = 0f;
            }
        }

        protected override void OnStopTurnRight()
        {
            base.OnStopTurnRight();

            if (IsControlEnabled)
            {
                // if left is still pressed, turn left
                if (bindings.IsKeyPressed(LKey.TurnLeft))
                    Kart.TurnMultiplier = 1f;
                // otherwise go straight
                else
                    Kart.TurnMultiplier = 0f;
            }
        }

        protected override void UseItem()
        {
            throw new System.NotImplementedException();
        }

        private void EveryTenth(object o)
        {
            //hacked-up laps handler

            if (atStart && pastMid)
            {
                pastMid = false;
                laps++;
                LKernel.GetG<Sound.SoundMain>().Play2D("lap.wav", false, false, false);
                myDerpy.ChangeAnimation("FlagWave2");
            }
            else if (!atStart) { myDerpy.ChangeAnimation("Forward1"); }
            if (laps >= 3)
            {
                IsControlEnabled = false;
                if (Kart.Acceleration >= 1.5f)
                    Kart.Acceleration -= 1.5f;
                else
                    Kart.Acceleration = 0.0f;
                foreach (Player p in LKernel.GetG<PlayerManager>().Players)
                {
                    if (p.laps >= 3 && p.IsComputerControlled == true)
                    {
                        //you lose
                        break;
                    }
                    else
                    {
                        if (!cheering)
                        {
                            cheering = true;
                            LKernel.GetG<Sound.SoundMain>().Play2D("Crowd Two.mp3", false, false, false);
                        }
                        //you win
                    }
                }
            }
            //end laps handler
        }

        #region key events

        #endregion key events
    }
}