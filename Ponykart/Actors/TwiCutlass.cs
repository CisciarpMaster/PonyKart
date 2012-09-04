using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Sound;
using PonykartParsers;

namespace Ponykart.Actors
{
    public class TwiCutlass : Kart
    {
        private AnimationState jetMax;
        private AnimationState jetMin;
        private readonly float topSpeedKmHour;

        private SoundMain soundMain;
        private ISound idleSound, fullSound;
        private ISoundSource revDownSound, revUpSound;
        /// <summary>
        /// true if we're in the "play the slower sound" state, false if we're in the "play the faster sound" state
        /// </summary>
        private bool idleState;


        public TwiCutlass(ThingBlock block, ThingDefinition def)
            : base(block, def)
        {

            // sounds
            soundMain = LKernel.GetG<SoundMain>();

           
            revDownSound = soundMain.GetSource("enginedrone.ogg");
            revUpSound = soundMain.GetSource("enginedroneloud.ogg");

            // convert from linear velocity to KPH
            topSpeedKmHour = DefaultMaxSpeed * 3.6f;
            idleState = true;

            LKernel.GetG<Root>().FrameStarted += FrameStarted;
        }

        /// <summary>
        /// Change the width of the jet engine based on our current speed
        /// </summary>
        bool FrameStarted(FrameEvent evt)
        {
            // crop it to be between 0 and 1
            float relSpeed = _vehicle.CurrentSpeedKmHour / topSpeedKmHour;
              

           
         

           

            return true;
        }

        /// <summary>
        /// Unhook from the event
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            LKernel.GetG<Root>().FrameStarted -= FrameStarted;

            base.Dispose(disposing);
        }
    }
}
