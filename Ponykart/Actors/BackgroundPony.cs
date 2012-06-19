using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Mogre;
using Ponykart.Core;
using Ponykart.Players;
using PonykartParsers;
using Timer = System.Threading.Timer;

namespace Ponykart.Actors {
	/// <summary>
	/// Class for the background ponies. Manages making them play random animations and moving their neck to face the player.
	/// </summary>
	public class BackgroundPony : LThing {
		protected static BackgroundPonyLoader loader;
		protected static CultureInfo culture = CultureInfo.InvariantCulture;

		// don't care about eyes, hair, horn, or folded wings since they aren't animated
		protected Entity bodyEnt, wingsEnt, hornEnt, foldedWingsEnt, eyesEnt, hairEnt, maneEnt, tailEnt;
		protected AnimationBlender bodyBlender, wingsBlender, maneBlender, tailBlender;
		protected readonly string nameOfPonyCharacter;

		public Pose AnimPose { get; protected set; }
		public readonly Type PonyType;
		protected bool cheering = false;
		protected AnimationState blinkState;
		protected Timer animTimer;
		protected const float BLEND_TIME = 1f;
		// milliseconds
		protected const int ANIMATION_TIMESPAN_MINIMUM = 5000, ANIMATION_TIMESPAN_MAXIMUM = 8000;
		protected Random random;
		protected Euler neckFacing;
		protected Bone neckbone;
		protected Kart followKart;

		public BackgroundPony(string bgPonyName, ThingBlock block, ThingDefinition def) : base(block, def) {
			// initialise the loader if we haven't already
			if (loader == null)
				loader = new BackgroundPonyLoader();

			AnimPose = Pose.Standing;
			random = new Random(IDs.Random);

			if (bgPonyName == null)
				nameOfPonyCharacter = block.GetStringProperty("PonyName", null);
			else
				nameOfPonyCharacter = bgPonyName;

			// get a line to parse
			string _line;
			if (!loader.BackgroundPonyDict.TryGetValue(nameOfPonyCharacter, out _line)) {
				// if the line doesn't exist, make a random one
				_line = loader.AllValues[random.Next(loader.AllValues.Length)];
				Launch.Log("[WARNING] The specified background pony (" + nameOfPonyCharacter + ") does not exist, using random one instead...");
			}
			// split up the data
			string[] _data = _line.Split(' ');
			// get our hairstyle ID number
			int hairstyleID = int.Parse(_data[1], culture);
			// set the pony type
			if (_data[2] == "pegasus")
				PonyType = Type.Pegasus;
			else if (_data[2] == "flyingpegasus")
				PonyType = Type.FlyingPegasus;
			else if (_data[2] == "earth")
				PonyType = Type.Earth;
			else if (_data[2] == "unicorn")
				PonyType = Type.Unicorn;

			// create nodes and stuff
			var sceneMgr = LKernel.Get<SceneManager>();

#region creation
			bodyEnt = sceneMgr.CreateEntity("BgPonyBody.mesh");
			RootNode.AttachObject(bodyEnt);

			eyesEnt = sceneMgr.CreateEntity("BgPonyEyes.mesh");

			if (PonyType == Type.Unicorn) 
				hornEnt = sceneMgr.CreateEntity("BgPonyHorn.mesh");
			else if (PonyType == Type.FlyingPegasus) 
				wingsEnt = sceneMgr.CreateEntity("BgPonyWings.mesh");
			else if (PonyType == Type.Pegasus) 
				foldedWingsEnt = sceneMgr.CreateEntity("BgPonyWingsFolded.mesh");
			

			// create hair
			hairEnt = sceneMgr.CreateEntity("BgPonyHair" + hairstyleID + ".mesh");
			maneEnt = sceneMgr.CreateEntity("BgPonyMane" + hairstyleID + ".mesh");
			tailEnt = sceneMgr.CreateEntity("BgPonyTail" + hairstyleID + ".mesh");

			// attach stuff
			bodyEnt.AttachObjectToBone("Eyes", eyesEnt);
			bodyEnt.AttachObjectToBone("Hair", hairEnt);
			bodyEnt.AttachObjectToBone("Mane", maneEnt);
			bodyEnt.AttachObjectToBone("Tail", tailEnt);
			if (PonyType == Type.Unicorn)
				bodyEnt.AttachObjectToBone("Horn", hornEnt);
			else if (PonyType == Type.Pegasus)
				bodyEnt.AttachObjectToBone("Wings", foldedWingsEnt);
			else if (PonyType == Type.FlyingPegasus)
				bodyEnt.AttachObjectToBone("Wings", wingsEnt);
#endregion

#region setting up colors in materials
			// body colour
			ColourValue bodyColour = new ColourValue(float.Parse(_data[3], culture), float.Parse(_data[4], culture), float.Parse(_data[5], culture));
			ColourValue bodyAOColour = new ColourValue(float.Parse(_data[6], culture), float.Parse(_data[7], culture), float.Parse(_data[8], culture));

			MaterialPtr newMat = SetBodyPartMaterialColours("BgPony", bodyColour, bodyAOColour);
			newMat.GetTechnique(0).GetPass(1).GetTextureUnitState(1).SetTextureName(_data[18].Substring(1, _data[18].Length - 2));

			bodyEnt.SetMaterial(newMat);
			
			// extra body parts
			if (PonyType == Type.Unicorn) {
				newMat = SetBodyPartMaterialColours("BgPonyHorn", bodyColour, bodyAOColour);
				hornEnt.SetMaterial(newMat);
			}
			else if (PonyType == Type.Pegasus) {
				newMat = SetBodyPartMaterialColours("BgPonyWingsFolded", bodyColour, bodyAOColour);
				foldedWingsEnt.SetMaterial(newMat);
			}
			else if (PonyType == Type.FlyingPegasus) {
				newMat = SetBodyPartMaterialColours("BgPonyWings", bodyColour, bodyAOColour);
				wingsEnt.SetMaterial(newMat);
			}
			
			// eye colours
			{
				ColourValue eyeColour1 = new ColourValue(float.Parse(_data[9], culture), float.Parse(_data[10], culture), float.Parse(_data[11], culture));
				ColourValue eyeColour2 = new ColourValue(float.Parse(_data[12], culture), float.Parse(_data[13], culture), float.Parse(_data[14], culture));
				ColourValue eyeHighlightColour = new ColourValue(float.Parse(_data[15], culture), float.Parse(_data[16], culture), float.Parse(_data[17], culture));

				MaterialPtr originalMat = MaterialManager.Singleton.GetByName("BgPonyEyes");
				Pair<ResourcePtr, bool> pair = MaterialManager.Singleton.CreateOrRetrieve("BgPonyEyes" + nameOfPonyCharacter, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
				newMat = (MaterialPtr) pair.first;

				if (pair.second) {
					newMat = originalMat.Clone("BgPonyEyes" + eyeColour1);

					var ps = newMat.GetTechnique(0).GetPass(0).GetFragmentProgramParameters();
						ps.SetNamedConstant("TopIrisColour", eyeColour1);
						ps.SetNamedConstant("BottomIrisColour", eyeColour2);
						ps.SetNamedConstant("HighlightColour", eyeHighlightColour);
					newMat.GetTechnique(0).GetPass(0).SetFragmentProgramParameters(ps);
				}

				eyesEnt.SetMaterial(newMat);
			}

			// hair colours
			{
				ColourValue hairColour1 = new ColourValue(float.Parse(_data[20], culture), float.Parse(_data[21], culture), float.Parse(_data[22], culture));
				ColourValue hairAOColour1 = new ColourValue(float.Parse(_data[23], culture), float.Parse(_data[24], culture), float.Parse(_data[25], culture));

				// two hair colours
				if (bool.Parse(_data[19])) {
					ColourValue hairColour2 = new ColourValue(float.Parse(_data[26], culture), float.Parse(_data[27], culture), float.Parse(_data[28], culture));
					ColourValue hairAOColour2 = new ColourValue(float.Parse(_data[29], culture), float.Parse(_data[30], culture), float.Parse(_data[31], culture));

					MaterialPtr originalMat = MaterialManager.Singleton.GetByName("BgPonyHair_Double_" + hairstyleID);
					Pair<ResourcePtr, bool> pair = MaterialManager.Singleton.CreateOrRetrieve("BgPonyHair_Double_" + hairstyleID + nameOfPonyCharacter, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
					newMat = (MaterialPtr) pair.first;

					if (pair.second) {
						newMat = originalMat.Clone("BgPonyHair_Double_" + hairstyleID + hairColour1);

						var ps = newMat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
							ps.SetNamedConstant("HairColour1", hairColour1);
							ps.SetNamedConstant("AOColour1", hairAOColour1);
							ps.SetNamedConstant("HairColour2", hairColour2);
							ps.SetNamedConstant("AOColour2", hairAOColour2);
						newMat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

						if (int.Parse(_data[32], culture) == 2)
							SetMaterialFragmentParameter(newMat, 0, "OutlineColour", hairAOColour2);
						else if (int.Parse(_data[32], culture) == 1)
							SetMaterialFragmentParameter(newMat, 0, "OutlineColour", hairAOColour1);
					}
					hairEnt.SetMaterial(newMat);
					maneEnt.SetMaterial(newMat);
					tailEnt.SetMaterial(newMat);
				}
				// one colour
				else {
					MaterialPtr originalMat = MaterialManager.Singleton.GetByName("BgPonyHair_Single_" + hairstyleID);
					Pair<ResourcePtr, bool> pair = MaterialManager.Singleton.CreateOrRetrieve("BgPonyHair_Single_" + hairstyleID + nameOfPonyCharacter, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
					newMat = (MaterialPtr) pair.first;

					if (pair.second) {
						newMat = originalMat.Clone("BgPonyHair_Single_" + hairstyleID + hairColour1);

						var ps = newMat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
							ps.SetNamedConstant("HairColour", hairColour1);
							ps.SetNamedConstant("AOColour", hairAOColour1);
						newMat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

						SetMaterialFragmentParameter(newMat, 0, "OutlineColour", hairAOColour1);
					}
					hairEnt.SetMaterial(newMat);
					maneEnt.SetMaterial(newMat);
					tailEnt.SetMaterial(newMat);
				}
			}
#endregion

#region animation
			// make sure our animations add their weights and don't just average out. The AnimationBlender already handles averaging between two anims.
			Skeleton skeleton = bodyEnt.Skeleton;
			skeleton.BlendMode = SkeletonAnimationBlendMode.ANIMBLEND_CUMULATIVE;

			// set up the blink animation state with some stuff
			blinkState = bodyEnt.GetAnimationState("Blink2");
			blinkState.Enabled = true;
			blinkState.Loop = true;
			blinkState.Weight = 1;
			blinkState.AddTime(ID);

			// set up all of the animation states to not use the neck bone
			neckbone = skeleton.GetBone("Neck");
			neckbone.SetManuallyControlled(true);
			foreach (var state in bodyEnt.AllAnimationStates.GetAnimationStateIterator()) {
				// don't add a blend mask to the blink state because we'll make a different one for it
				if (state == blinkState)
					continue;

				state.CreateBlendMask(skeleton.NumBones);
				state.SetBlendMaskEntry(neckbone.Handle, 0f);
			}
			neckbone.InheritOrientation = false;

			neckFacing = new Euler(0, 0, 0);

			// set up a blend mask so only the eyebrow bones have any effect on the blink animation
			blinkState.CreateBlendMask(skeleton.NumBones, 0f);
			ushort handle = skeleton.GetBone("EyeBrowTop.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.R").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowTop.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);
			handle = skeleton.GetBone("EyeBrowBottom.L").Handle;
			blinkState.SetBlendMaskEntry(handle, 1f);

			// add the blink state to the animation manager so it has time added to it
			AnimationManager animMgr = LKernel.GetG<AnimationManager>();
			animMgr.Add(blinkState);

			// set up other animated things
			bodyBlender = new AnimationBlender(bodyEnt);
			bodyBlender.Init("Stand1", true);
			animMgr.Add(bodyBlender);

			maneBlender = new AnimationBlender(maneEnt);
			maneBlender.Init("Stand1", true);
			animMgr.Add(maneBlender);

			tailBlender = new AnimationBlender(tailEnt);
			tailBlender.Init("Stand1", true);
			animMgr.Add(tailBlender);

			if (PonyType == Type.FlyingPegasus) {
				wingsBlender = new AnimationBlender(wingsEnt);
				wingsBlender.Init("Flap1", true);
				animMgr.Add(wingsBlender);
			}

			// set up some timers to handle animation changing
			animTimer = new Timer(new TimerCallback(AnimTimerTick), null, random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);

			// add a bit of time to things so the animations aren't all synced at the beginning
			AddTimeToBodyManeAndTail();
#endregion

			followKart = LKernel.GetG<PlayerManager>().MainPlayer.Kart;
			LKernel.GetG<Root>().FrameStarted += FrameStarted;
		}

		private void SetMaterialFragmentParameter(MaterialPtr mat, ushort passIndex, string constantName, ColourValue colour) {
			var ps = mat.GetTechnique(0).GetPass(passIndex).GetFragmentProgramParameters();
				ps.SetNamedConstant(constantName, colour);
			mat.GetTechnique(0).GetPass(passIndex).SetFragmentProgramParameters(ps);
		}

		protected MaterialPtr SetBodyPartMaterialColours(string materialName, ColourValue bodyColour, ColourValue bodyAOColour) {
			MaterialPtr originalMat = MaterialManager.Singleton.GetByName(materialName);
			Pair<ResourcePtr, bool> pair = MaterialManager.Singleton.CreateOrRetrieve(materialName + nameOfPonyCharacter, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
			MaterialPtr newMat = (MaterialPtr) pair.first;
			// if the material already exists, you don't have to modify it
			if (pair.second) {
				newMat = originalMat.Clone(materialName + bodyColour);

				var ps = newMat.GetTechnique(0).GetPass(1).GetFragmentProgramParameters();
					ps.SetNamedConstant("BodyColour", bodyColour);
					ps.SetNamedConstant("AOColour", bodyAOColour);
				newMat.GetTechnique(0).GetPass(1).SetFragmentProgramParameters(ps);

				SetMaterialFragmentParameter(newMat, 0, "OutlineColour", bodyAOColour);
			}

			return newMat;
		}

		private readonly Radian NECK_YAW_LIMIT = new Degree(70f);
		private readonly Radian NECK_PITCH_LIMIT = new Degree(60f);

		/// <summary>
		/// Rotate the neck bone to face the kart. Will eventually need to redo this when we have multiple karts, to face whichever's nearest, etc.
		/// </summary>
		bool FrameStarted(FrameEvent evt) {
			if (!Pauser.IsPaused) {
				Vector3 lookat = RootNode.ConvertWorldToLocalPosition(followKart.ActualPosition);
				// temp is how much you need to rotate to get from the current orientation to the new orientation
				// we use -lookat because our bone points towards +Z, whereas this code was originally made for things facing towards -Z
				Euler temp = neckFacing.GetRotationTo(-lookat, true, true, true);
				// limit the offset so the head turns at a maximum of 3 radians per second
				Radian tempTime = new Radian(evt.timeSinceLastFrame * 3);
				temp.LimitYaw(tempTime);
				temp.LimitPitch(tempTime);

				neckFacing = neckFacing + temp;
				neckFacing.LimitYaw(NECK_YAW_LIMIT);
				neckFacing.LimitPitch(NECK_PITCH_LIMIT);
				neckbone.Orientation = neckFacing;
			}

			return true;
		}

#region animation changing
		/// <summary>
		/// helper method to add animation blending to the three main animated parts of the bg ponies
		/// </summary>
		protected virtual void AnimateBodyManeAndTail(string animationName, AnimationBlendingTransition transition, float duration, bool looping) {
			bodyBlender.Blend(animationName, transition, duration, looping);
			maneBlender.Blend(animationName, transition, duration, looping);
			tailBlender.Blend(animationName, transition, duration, looping);
		}

		/// <summary>
		/// helper method to add some initial time to all of the three main bg pony parts so they aren't all in sync
		/// </summary>
		public virtual void AddTimeToBodyManeAndTail() {
			float rand = (float) random.NextDouble();
			bodyBlender.AddTime(rand);
			maneBlender.AddTime(rand);
			tailBlender.AddTime(rand);
		}

		/// <summary>
		/// Play an animation instantly
		/// </summary>
		public override void ChangeAnimation(string animationName) {
			AnimateBodyManeAndTail(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
		}

		/// <summary>
		/// Make the wings (if we have some) change their animation instantly
		/// </summary>
		public virtual void ChangeWingAnimation(string animationName) {
			if (PonyType == Type.FlyingPegasus) {
				wingsBlender.Blend(animationName, AnimationBlendingTransition.BlendSwitch, 0, true);
				wingsBlender.AddTime(ID);
			}
		}

		/// <summary>
		/// Play a random standing animation and change our pose to Standing
		/// </summary>
		public virtual void Stand() {
			string anim = "Stand" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);
			if (PonyType == Type.FlyingPegasus)
				wingsBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Standing;
		}

		/// <summary>
		/// Play a random sitting animation and change our pose to Sitting
		/// </summary>
		public virtual void Sit() {
			string anim = "Sit" + random.Next(1, 4);
			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);
			if (PonyType == Type.FlyingPegasus)
				wingsBlender.Blend("WingsRest", AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

			AnimPose = Pose.Sitting;
		}

		/// <summary>
		/// Play a random flying animation (if this is a pegasus) and change our pose to Flying
		/// </summary>
		public virtual void Fly() {
			if (PonyType == Type.FlyingPegasus) {
				string anim = "Fly" + random.Next(1, 5);

				AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);

				anim = "Flap" + random.Next(1, 4);
				wingsBlender.Blend(anim, AnimationBlendingTransition.BlendThenAnimate, 0.2f, true);

				AnimPose = Pose.Flying;
			}
		}

		/// <summary>
		/// Blink once
		/// </summary>
		public virtual void Blink() {
			blinkState.Enabled = true;
			blinkState.TimePosition = 0;
		}

		/// <summary>
		/// Play a random cheering animation according to our pose
		/// </summary>
		public virtual void Cheer() {
			string anim = "";

			switch (AnimPose) {
				case Pose.Standing:
					int rand = random.Next(0, 2);
					anim = "Cheer" + rand;
					if (rand == 0)
						anim = "Clap";
					break;
				case Pose.Sitting:
					rand = random.Next(1, 1);
					anim = "SitCheer" + rand;
					break;
				case Pose.Flying:
					// TODO
					break;
			}

			AnimateBodyManeAndTail(anim, AnimationBlendingTransition.BlendWhileAnimating, BLEND_TIME, true);

			cheering = true;
		}

		/// <summary>
		/// Stops playing a cheering animation and plays a different appropriate one instead
		/// </summary>
		public virtual void StopCheering() {
			switch (AnimPose) {
				case Pose.Standing:
					Stand();
					break;
				case Pose.Sitting:
					Sit();
					break;
				case Pose.Flying:
					Fly();
					break;
			}

			cheering = false;
		}

		/// <summary>
		/// Plays a different, similar animation.
		/// </summary>
		public virtual void PlayNext() {
			if (cheering)
				Cheer();
			else if (AnimPose == Pose.Standing)
				Stand();
			else if (AnimPose == Pose.Sitting)
				Sit();
			else if (AnimPose == Pose.Flying)
				Fly();
		}
#endregion

		/// <summary>
		/// method for the animation timer to run
		/// </summary>
		protected void AnimTimerTick(object o) {
			if (Pauser.IsPaused) {
				// keep trying again until we're unpaused
				animTimer.Change(500, 500);
			}
			else {
				PlayNext();
				animTimer.Change(random.Next(ANIMATION_TIMESPAN_MINIMUM, ANIMATION_TIMESPAN_MAXIMUM), Timeout.Infinite);
			}
		}

		/// <summary>
		/// Clean up
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing) {
				var animMgr = LKernel.GetG<AnimationManager>();
				animMgr.Remove(blinkState);
				animMgr.Remove(bodyBlender);
				animMgr.Remove(maneBlender);
				animMgr.Remove(tailBlender);
				if (PonyType == Type.FlyingPegasus)
					animMgr.Remove(wingsBlender);

				if (LKernel.GetG<Levels.LevelManager>().IsValidLevel) {
					var sceneMgr = LKernel.Get<SceneManager>();
					sceneMgr.DestroyEntity(bodyEnt);
					bodyEnt.Dispose();
					sceneMgr.DestroyEntity(eyesEnt);
					eyesEnt.Dispose();
					sceneMgr.DestroyEntity(hairEnt);
					hairEnt.Dispose();
					sceneMgr.DestroyEntity(maneEnt);
					maneEnt.Dispose();
					sceneMgr.DestroyEntity(tailEnt);
					tailEnt.Dispose();

					if (PonyType == Type.FlyingPegasus) {
						sceneMgr.DestroyEntity(wingsEnt);
						wingsEnt.Dispose();
					}
					else if (PonyType == Type.Unicorn) {
						sceneMgr.DestroyEntity(hornEnt);
						hornEnt.Dispose();
					}
					else if (PonyType == Type.Pegasus) {
						sceneMgr.DestroyEntity(foldedWingsEnt);
						foldedWingsEnt.Dispose();
					}
				}
			}
			if (animTimer != null)
				animTimer.Dispose();

			LKernel.GetG<Root>().FrameStarted -= FrameStarted;

			base.Dispose(disposing);
		}

		//////////////////////////////////////////////

		public enum Pose {
			Sitting,
			Standing,
			Flying
		}

		public enum Type {
			/// <summary> No wings or horn </summary>
			Earth,
			/// <summary> Folded wings </summary>
			Pegasus,
			/// <summary> Spread wings, even if it's not actually flying </summary>
			FlyingPegasus,
			/// <summary> Horn </summary>
			Unicorn
		}

		/// <summary>
		/// static class to load all of the bg pony data from a file
		/// </summary>
		protected class BackgroundPonyLoader {
			public Dictionary<string, string> BackgroundPonyDict { get; private set; }
			public string[] AllValues { get; private set; }

			public BackgroundPonyLoader() {
				BackgroundPonyDict = new Dictionary<string, string>();

				using (var reader = new StreamReader(File.OpenRead("media/background ponies/all.bgp"))) {
					while (!reader.EndOfStream) {
						string line = reader.ReadLine();

						BackgroundPonyDict[line.Substring(1, line.IndexOf("\" ") - 1)] = line;
					}
				}

				AllValues = new string[BackgroundPonyDict.Values.Count];
				BackgroundPonyDict.Values.CopyTo(AllValues, 0);
			}
		}
	}

	
}
