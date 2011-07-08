using System;
using System.Collections.Generic;
using Lymph.Phys;
using Mogre;
using Mogre.PhysX;

namespace Lymph.Actors {
	/// <summary>
	/// ControllerThings are probably the least realistic thing physx offers. But if you want something you can control very precicely without
	/// it getting stuck on stuff like stairs, then this class is perfect.
	/// 
	/// Note that Controllers are an addon to physx so they have some quirks of their own.
	/// 
	/// Controllers are very similar to kinematic actors so go read about them first! The main difference is that controllers
	/// are explicitly designed for players and stuff, so unlike kinematic actors they do come with collision. However they do not come with
	/// gravity and can't be moved by other forces.
	/// 
	/// They collide with kinematic actors if they are moved into one, however a kinematic actor can move into a controller and the controller will not react.
	/// You cannot move a dynamic actor into a controller though.
	/// </summary>
	public abstract class ControllerThing : Thing {
		#region Default abstracts
		/// <summary>
		/// The default height of the capsule for the Controller. If you want a sphere, this should be 0.
		/// </summary>
		protected abstract float DefaultHeight { get; }
		/// <summary>
		/// The default radius of the capsule for the Controller.
		/// </summary>
		protected abstract float DefaultRadius { get; }
		#endregion

		#region Properties
		/// <summary>
		/// Height of the Controller. This must be above zero!
		/// </summary>
		public float Height { get; private set; }
		/// <summary>
		/// Radius of the Controller. This must be above zero!
		/// </summary>
		public float Radius { get; private set; }

		/// <summary>
		/// The Controller for this Thing. This is disposed of in PhysXMain instead of here.
		/// </summary>
		public CapsuleController Controller { get; private set; }
		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public ControllerThing(ThingTemplate tt) : base(tt) { }

		// silly c#
		protected override void Setup(ThingTemplate tt) {
			// radius of the capsule
			float rad;
			if (tt.FloatTokens.TryGetValue("Radius", out rad))
				Radius = rad;
			else
				Radius = DefaultRadius;
			// there's a check built into physx (not the wrapper) that makes sure that the radius and height aren't 0
			// when you try to create the controller from the desc. I don't really see the problem with a 0-height controller,
			// but apparently nvidia does. Just use 0.001f or something instead.
			if (Radius <= 0f)
				throw new ApplicationException("Radius cannot be zero or negative!");
			

			// height of the capsule
			float height;
			if (tt.FloatTokens.TryGetValue("Height", out height))
				Height = height;
			else
				Height = DefaultHeight;
			if (Height <= 0f)
				throw new ApplicationException("Height cannot be zero or negative!");

			base.Setup(tt);
		}

		/// <summary>
		/// Move this thang
		/// </summary>
		/// <param name="displacement">The displacement vector. Where are you moving?</param>
		/// <param name="activeGroups">Which groups do you want to check for collision with?</param>
		/// <param name="minDist">The minimum distance to consider</param>
		/// <param name="smoothing">Adds some smoothing when we walk up stairs or something, for example</param>
		/// <remarks>
		/// If you get an exception here, it is most likely because a previous Controller with the same name
		/// was not disposed properly. They're... weird.
		/// </remarks>
		public void Move(Vector3 displacement, uint activeGroups, float minDist, float smoothing) {
			ControllerFlags flags;
			Controller.Move(displacement, activeGroups, minDist, out flags, smoothing);
		}

		/// <summary>
		/// Shorter version of MobileThing.Move(Vector3, uint, float, float).
		/// Assumes we want to check all collidable groups for collision, uses the thing's radius as the minimum distance,
		/// and uses 0.8f as a smoothing factor.
		/// </summary>
		/// <param name="displacement">The displacement vector. Where are you moving?</param>
		public void Move(Vector3 displacement) {
			Move(displacement, Groups.AllGroups, 0.05f, 0.8f);
		}

		#region PhysX stuff
		/// <summary>
		/// Creates the Controller, assigns the collision group ID to it, and then attaches it to the scene node
		/// </summary>
		protected override void SetUpPhysics() {
			CreateController();
			AttachToSceneNode();
		}

		/// <summary>
		/// Creates the controller. Also adds the controller hit report and sets its user data to link the controller to this
		/// MobileThing object.
		/// </summary>
		protected void CreateController() {

			var desc = new CapsuleControllerDesc() {
				Position = SpawnPosition,
				Height = Height,
				Radius = Radius,
				SkinWidth = 0.1f,
				SlopeLimit = Mogre.Math.DegreesToRadians(45),
				StepOffset = 0.1f, // we use this to test above us if we're in a cave or something
				UpDirection = HeightFieldAxes.Y,
				ClimbingMode = CapsuleClimbingModes.Easy,
				Callback = new StandardControllerHitReport(),
				UserData = this,
			};

			// Ah. Mystery solved. Height can't be zero.
			Controller = LKernel.Get<PhysXMain>().ControllerManager.CreateController(LKernel.Get<PhysXMain>().Scene, desc);

			Controller.Actor.Name = Name + ID;
			Controller.Actor.UserData = this;
			Controller.Actor.Group = CollisionGroupID;
		}

		/// <summary>
		/// Attaches the controller to our SceneNode
		/// </summary>
		protected void AttachToSceneNode() {
			if (Controller != null && Node != null) {
				Controller.Position = Node.Position;
				Controller.Actor.GlobalOrientationQuaternion = Node.Orientation;
			}
		}
		#endregion

		/// <summary>
		/// Since we have additional optional parameters in this class, we need to add them to this thingy
		/// </summary>
		public override IEnumerable<KeyValuePair<string, float>> GetOptionalNumbers() {
			if (Radius != DefaultRadius)
				yield return new KeyValuePair<string, float>("Radius", Radius);
			if (Height != DefaultHeight)
				yield return new KeyValuePair<string, float>("Height", Height);

			// yeah okay this is a bit of a hack but hey, it works
			var KVPsFromBase = base.GetOptionalNumbers();
			foreach (var kvp in KVPsFromBase) {
				yield return kvp;
			}
		}

		public override void Dispose() {
			// we don't dispose the controller here because that is disposed in PhysXMain due to a problem in the wrapper.
			base.Dispose();
		}
	}
}
