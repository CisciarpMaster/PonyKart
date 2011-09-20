using IrrKlang;
using Mogre;
using Ponykart.Properties;
using Ponykart.Sound;
using PonykartParsers;

namespace Ponykart.Actors {
	public class SoundComponent {
		public ISound Sound { get; protected set; }
		public string Name { get; protected set; }
		LThing owner;

		/// <summary>
		/// For sounds!
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public SoundComponent(LThing lthing, ThingBlock template, SoundBlock block) {
			Name = block.GetStringProperty("name", template.ThingName);
			owner = lthing;

			if (!Settings.Default.EnableSounds)
				return;

			var soundMain = LKernel.GetG<SoundMain>();
			ISoundSource source = soundMain.GetSource(block.GetStringProperty("File", null));

			bool looping = block.GetBoolProperty("looping", true);
			bool sfx = block.GetBoolProperty("SpecialEffects", false);
			Vector3 pos = template.GetVectorProperty("position", null);

			Sound = soundMain.Play3D(source, pos, looping, true, sfx);

			Sound.PlaybackSpeed = block.GetFloatProperty("Speed", 1);
			Sound.Volume = block.GetFloatProperty("volume", 1);
			Sound.MaxDistance = block.GetFloatProperty("maxdistance", soundMain.Engine.Default3DSoundMaxDistance);
			Sound.MinDistance = block.GetFloatProperty("mindistance", soundMain.Engine.Default3DSoundMinDistance);

			// TODO: effects, if we end up using any of them

			Sound.Paused = false;
		}

		/// <summary>
		/// Update's the sound's position (and velocity if the owner has a Body).
		/// This is called from <see cref="Ponykart.Physics.MogreMotionState"/>.
		/// </summary>
		public void Update() {
			Sound.Position = owner.RootNode._getDerivedPosition().ToSoundVector();
			if (owner.Body != null) {
				Sound.Velocity = owner.Body.LinearVelocity.ToSoundVector();
			}
		}

		public override string ToString() {
			return Name + "Sound";
		}

		public void Dispose() {
			Sound.Stop();
			Sound.Dispose();
		}
	}
}
