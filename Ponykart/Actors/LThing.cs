using System;
using System.Collections.ObjectModel;
using BulletSharp;
using Mogre;
using Ponykart.IO;
using Ponykart.Levels;
using Ponykart.Physics;

namespace Ponykart.Actors {

	public class LThing : System.IDisposable {
		public int ID { get; protected set; }
		public string Name { get; protected set; }
		public RigidBody Body { get; protected set; }
		public SceneNode RootNode { get; protected set; }

		/// <summary>
		/// Initial motion state setter. Override this if you want something different. This is only used for initialisation!
		/// </summary>
		protected virtual MotionState DefaultMotionState { get { return new MogreMotionState(SpawnPosition, SpawnOrientation, RootNode); } }
		/// <summary>
		/// The actual motion state.
		/// </summary>
		protected MotionState MotionState { get; set; }
		public PonykartCollisionGroups CollisionGroup { get; protected set; }
		public PonykartCollidesWithGroups CollidesWith { get; protected set; }

		public Vector3 SpawnPosition { get; private set; }
		public Quaternion SpawnOrientation { get; private set; }
		public Vector3 SpawnScale { get; private set; }
		protected RigidBodyConstructionInfo Info { get; set; }

		protected Collection<ModelComponent> ModelComponents;
		protected Collection<ShapeComponent> ShapeComponents;

		public LThing(ThingInstanceTemplate template, ThingDefinition def) {
			ID = template.ID;
			Name = template.Name;

			ModelComponents = new Collection<ModelComponent>();
			ShapeComponents = new Collection<ShapeComponent>();

			// get our three basic transforms
			SpawnPosition = template.GetVectorProperty("position", null);

			SpawnOrientation = template.GetVectorProperty("rotation", Vector3.ZERO).DegreeVectorToGlobalQuaternion();

			SpawnScale = template.GetVectorProperty("scale", Vector3.UNIT_SCALE);

			// and start setting up this thing!
			Setup(template, def);
			SetupMogre(template, def);
			InitialiseComponents(template, def);

			RootNode.Position = SpawnPosition;
			RootNode.Orientation = SpawnOrientation;
			// only scale up the root node if it doesn't have any physics things attached - bullet really does not like scaling.
			// Need a few variations of identical objects with different scales? Gonna have to make different .things for them.
			// Though it might be easier to just have one general .thing for them, and all it does is run a script that randomly
			// gets one of the others.
			if (ShapeComponents.Count == 0)
				RootNode.Scale(SpawnScale);

			SetupPhysics(template, def);
		}

		/// <summary>
		/// Use this method if you need some more stuff to happen before the constructor starts setting everything up.
		/// For example if you need to get more things out of the ThingTemplate, you can use this for that.
		/// </summary>
		protected virtual void Setup(ThingInstanceTemplate template, ThingDefinition def) {}

		/// <summary>
		/// Sets up mogre stuff, like our root scene node
		/// </summary>
		protected void SetupMogre(ThingInstanceTemplate template, ThingDefinition def) {
			RootNode = LKernel.Get<SceneManager>().RootSceneNode.CreateChildSceneNode(Name + ID);
			
		}

		/// <summary>
		/// Make our components
		/// </summary>
		protected void InitialiseComponents(ThingInstanceTemplate template, ThingDefinition def) {
			// make our components
			foreach (var mblock in def.ModelBlocks)
				ModelComponents.Add(new ModelComponent(this, template, mblock));
			foreach (var sblock in def.ShapeBlocks)
				ShapeComponents.Add(new ShapeComponent(this, template, sblock));
		}

		protected void SetupPhysics(ThingInstanceTemplate template, ThingDefinition def) {
			// if we have no shape components then we don't set up physics
			if (ShapeComponents.Count == 0)
				return;

			PreSetUpBodyInfo();
			SetUpBodyInfo(def);
			PostSetUpBodyInfo();
			CreateBody(def);
			PostCreateBody();
			SetBodyUserData();
		}

		/// <summary>
		/// If you need anything before we set up the body info
		/// </summary>
		protected virtual void PreSetUpBodyInfo() { }

		/// <summary>
		/// Set up all of the stuff needed before we create our body
		/// </summary>
		protected void SetUpBodyInfo(ThingDefinition def) {
			// set up our collision shapes
			CollisionShape shape;
			// if we just have one shape component, we use its shape as the main one
			if (ShapeComponents.Count == 1) {
				// ideally we should rotate the whole Thing instead of its shape, but if we only have one shape and for whatever reason we absolutely cannot rotate the node instead,
				// we'll have to stick it in a compound shape instead.
				if (ShapeComponents[0].Transform == ShapeBlock.UNCHANGED) {
					shape = ShapeComponents[0].Shape;
				}
				else {
					CompoundShape comp = new CompoundShape();
					comp.AddChildShape(ShapeComponents[0].Transform, ShapeComponents[0].Shape);
					shape = comp;
				}
			}
			// if we have more than one component we'll need a compound shape
			else {
				CompoundShape comp = new CompoundShape();
				foreach (var sc in ShapeComponents) {
					sc.Transform.SetScale(SpawnScale);
					comp.AddChildShape(sc.Transform, sc.Shape);
				}
				shape = comp;
			}

			// get the physics type and set up the mass of the body
			ThingEnum physicsType = def.GetEnumProperty("physics", null);
			float mass;
			if (physicsType.HasFlag(ThingEnum.Static))
				mass = 0;
			else {
				mass = def.GetFloatProperty("mass", 1);
			}

			// create our construction info thingy
			Vector3 inertia;
			shape.CalculateLocalInertia(mass, out inertia);
			MotionState = DefaultMotionState;
			Info = new RigidBodyConstructionInfo(mass, MotionState, shape, inertia);
			// TODO
			LKernel.Get<PhysicsMaterialManager>().ApplyMaterial(Info, LKernel.Get<PhysicsMaterialManager>().DefaultMaterial);

			// collision group
			string collisionGroup = def.GetStringProperty("collisiongroup", "Default");
			PonykartCollisionGroups pcg;
			if (!Enum.TryParse<PonykartCollisionGroups>(collisionGroup, true, out pcg))
				throw new FormatException("Invalid collision group!");
			CollisionGroup = pcg;

			// collides-with group
			string collidesWith = def.GetStringProperty("collideswith", "Default");
			PonykartCollidesWithGroups pcwg;
			if (!Enum.TryParse<PonykartCollidesWithGroups>(collidesWith, true, out pcwg))
				throw new FormatException("Invalid collides-with group!");
			CollidesWith = pcwg;

			Matrix4 transform = new Matrix4();
			transform.MakeTransform(SpawnPosition, SpawnScale, SpawnOrientation);
			Info.StartWorldTransform = transform;
			MotionState.WorldTransform = transform;
		}

		/// <summary>
		/// Override this if you want to do more to the construction info before it's used to create the body but after it's been created
		/// </summary>
		protected virtual void PostSetUpBodyInfo() { }

		/// <summary>
		/// Creates the body and makes it static/kinematic if specified.
		/// </summary>
		protected void CreateBody(ThingDefinition def) {
			Body = new RigidBody(Info);

			// stick on our flags
			ThingEnum te = def.GetEnumProperty("physics", null);
			if (te.HasFlag(ThingEnum.Static))
				Body.CollisionFlags |= CollisionFlags.StaticObject;
			else if (te.HasFlag(ThingEnum.Kinematic))
				Body.CollisionFlags |= CollisionFlags.KinematicObject;

			LKernel.Get<PhysicsMain>().World.AddRigidBody(Body, CollisionGroup, CollidesWith);
		}

		/// <summary>
		/// Override this if you want to do more to the rigid body
		/// </summary>
		protected virtual void PostCreateBody() { }

		/// <summary>
		/// Sets the Actor's UserData to this class so we can easily get to it.
		/// </summary>
		protected void SetBodyUserData() {
			Body.UserObject = this;
			Body.SetName(Name);
			Body.SetCollisionGroup(CollisionGroup);
		}



		public virtual void Dispose() {
			var sceneMgr = LKernel.Get<SceneManager>();
			var world = LKernel.Get<PhysicsMain>().World;
			bool valid = LKernel.Get<LevelManager>().IsValidLevel;

			foreach (ModelComponent mc in ModelComponents)
				mc.Dispose();
			foreach (ShapeComponent sc in ShapeComponents)
				sc.Dispose();

			if (RootNode != null) {
				if (valid)
					sceneMgr.DestroySceneNode(RootNode);
				RootNode.Dispose();
				RootNode = null;
			}
			if (Body != null) {
				if (valid)
					world.RemoveRigidBody(Body);
				Body.Dispose();
				Body = null;
			}

			ModelComponents.Clear();
			ShapeComponents.Clear();
		}
	}
}
