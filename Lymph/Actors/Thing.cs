using System;
using System.Collections.Generic;
using Ponykart.Levels;
using Ponykart.Phys;
using Ponykart.Stuff;
using Mogre;

namespace Ponykart.Actors {
	/// <summary>
	/// Base class for all game objects.
	/// </summary>
	public abstract class Thing : IDisposable {
		#region Fields
		/// <summary>
		/// This thing's "mesh" node. This is the one with all of the entities attached
		/// </summary>
		public SceneNode Node { get; private set; }
		/// <summary>
		/// This thing's entity
		/// </summary>
		public Entity Entity { get; private set; }
		/// <summary>
		/// ID number
		/// </summary>
		public int ID { get; private set; }
		/// <summary>
		/// The ribbon emitter
		/// </summary>
		public RibbonTrail Ribbon { get; private set; }
		/// <summary>
		/// The SceneNode that the ribbon is attached to
		/// </summary>
		public SceneNode RibbonNode { get; private set; }
		#endregion

		#region Default abstracts
		/*
		 * defaults. Don't read from these - these are just to set the object's properties if the template doesn't contain them.
		 */
		/// <summary>
		/// The model file to use.
		/// </summary>
		protected abstract string DefaultModel { get; }
		/// <summary>
		/// Set me to null if you just want to read the material from the model file
		/// </summary>
		protected abstract string DefaultMaterial { get; }
		/// <summary>
		/// How fast this thing moves.
		/// </summary>
		protected abstract float DefaultMoveSpeed { get; }
		/// <summary>
		/// How does it move around? Does it move towards the player, move around randomly, or just stand there?
		/// Eventually I think I'll want to define a way for things to move via scripts (like making something
		/// move around on a certain path), but I can get to that later.
		/// </summary>
		protected abstract MoveBehaviour DefaultMoveBehaviour { get; }
		/// <summary>
		/// Collision group
		/// </summary>
		protected abstract uint DefaultCollisionGroupID { get; }
		#endregion

		/*
		 * ===================================================================
		 * Don't forget to add these to the GetOptional methods at the bottom!
		 * ===================================================================
		 */
		#region Properties
		/*
		 * Required tokens
		 */
		/// <summary> 
		/// Required. "Name" - Does not include the ID! Add it yourself!
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Required. "Position" - Position where this thing should spawn.
		/// </summary>
		public Vector3 SpawnPosition { get; private set; }
		/*
		 * optional tokens
		 */
		/// <summary>
		/// "Behaviour" - default is IGNORE
		/// </summary>
		public MoveBehaviour MoveBehaviour { get; private set; }
		/// <summary>
		/// "Orientation" - default is 0
		/// </summary>
		public float SpawnOrientation { get; private set; }
		/// <summary>
		/// "Script" - default is null
		/// </summary>
		public string Script { get; private set; }
		/// <summary>
		/// "Model"
		/// </summary>
		public string Model { get; private set; }
		/// <summary>
		/// "Material"
		/// </summary>
		public string Material { get; private set; }
		/// <summary>
		/// "Speed"
		/// </summary>
		public float MoveSpeed { get; private set; }
		/// <summary>
		/// "CollisionGroup"
		/// </summary>
		public uint CollisionGroupID { get; private set; }
		#endregion

		/// <summary>
		/// Constructor! Yay!
		/// This gets everything out of the template. If something is not in the template, it uses the defaults.
		/// 
		/// An annoying thing about C# is that it runs the constructors higher up the heirarchy before running its own constructors.
		/// While I can see the reason behind doing that, it makes certain things a bit more frustrating.
		/// If you need to run extra stuff before this base constructor runs the methods to set up the Mogre and PhysX stuff,
		/// use Setup(ThingTemplate).
		/// </summary>
		public Thing(ThingTemplate tt) {
			// get out required tokens
			Name = tt.StringTokens["Name"];
			SpawnPosition = tt.VectorTokens["Position"];

			// get out optional ones
			// orientation
			float orient;
			if (tt.FloatTokens.TryGetValue("Orientation", out orient))
				SpawnOrientation = orient;
			else
				SpawnOrientation = 0;

			// run a lua script?
			string script;
			if (tt.StringTokens.TryGetValue("Script", out script))
				Script = script;
			else
				Script = null;

			// model file
			string model;
			if (tt.StringTokens.TryGetValue("Model", out model))
				Model = model;
			else
				Model = DefaultModel;

			// material - if the default is null, then we just use what the model file says
			string mat;
			if (tt.StringTokens.TryGetValue("Material", out mat))
				Material = mat;
			else
				Material = DefaultMaterial;

			// move speed
			float move;
			if (tt.FloatTokens.TryGetValue("Speed", out move))
				MoveSpeed = move;
			else
				MoveSpeed = DefaultMoveSpeed;

			// collision group
			float f;
			if (tt.FloatTokens.TryGetValue("CollisionGroup", out f))
				CollisionGroupID = (uint)f;
			else
				CollisionGroupID = DefaultCollisionGroupID;

			// enums are a little more annoying
			string mbs; MoveBehaviour mb;
			if (!tt.StringTokens.TryGetValue("Behaviour", out mbs)) {
				if (Enum.TryParse<MoveBehaviour>(mbs, true, out mb))
					MoveBehaviour = mb;
				else
					MoveBehaviour = DefaultMoveBehaviour;
			}

			// ==========================================

			ID = tt.ID;

			Setup(tt);
			CreateMogreStuff();
			SetUpPhysics();
			RunScript();
		}

		/// <summary>
		/// Use this method if you need some more stuff to happen before the constructor starts setting everything up.
		/// For example if you need to get more things out of the ThingTemplate, you can use this for that.
		/// 
		/// Remember, if you add more optional parameters to your subclass, don't forget to add one of those iterator methods 
		/// (see Methods to get optional properties) to somewhere in your subclass!
		/// </summary>
		protected virtual void Setup(ThingTemplate tt) { }

		#region Mogre stuff

		/// <summary>
		/// Creates the basic mogre stuff, like a node, entity, and material.
		/// If you want to do other mogre stuff, use CreateMoreMogreStuff().
		/// </summary>
		protected void CreateMogreStuff() {
			var sceneMgr = LKernel.Get<SceneManager>();
			// Create the node that the entities will be attached to
			Node = sceneMgr.RootSceneNode.CreateChildSceneNode(Name + ID, SpawnPosition);
			Entity = sceneMgr.CreateEntity(Name + ID, Model);
			if (DefaultMaterial != null) {
				Entity.SetMaterialName(Material);
			}
			Node.AttachObject(Entity);

			CreateMoreMogreStuff();
		}

		/// <summary>
		/// Override this if you want to create more mogre stuff than the basic node, entity, and material thingy.
		/// This is called after that method finishes.
		/// Keep in mind that this will run from the Thing's constructor, so you won't be able to use any parameters
		/// defined in the subclass parameters.
		/// To avoid this, just stick that sort of thing in the template!
		/// ... if that makes any sense! Not sure if I should put an example in here
		/// </summary>
		protected virtual void CreateMoreMogreStuff() {
			// subclasses can fill this in
		}

		/// <summary>
		/// If you want to create a ribbon, this needs to be called somewhere in CreateMoreMogreStuff().
		/// </summary>
		/// <param name="trailLength">The length of the ribbon trail</param>
		/// <param name="maxChainElements">How many parts are in the chain?</param>
		/// <param name="colour"><see cref="Lymph.Stuff.AntigenColour"/></param>
		/// <param name="width">The width of the ribbon</param>
		protected void CreateRibbon(float trailLength, uint maxChainElements, ColourValue colour, float width) {
			if (Constants.RIBBONS) {
				Ribbon = LKernel.Get<SceneManager>().CreateRibbonTrail(Name + ID + "Ribbon");
				Ribbon.SetMaterialName("ribbon");
				Ribbon.TrailLength = trailLength;
				Ribbon.MaxChainElements = maxChainElements;
				Ribbon.SetInitialColour(0, colour);
				Ribbon.SetColourChange(0, new ColourValue(0, 0, 0, 3));
				Ribbon.SetInitialWidth(0, width);
				Ribbon.RenderQueueGroup = (byte)RQGL.RIBBONS;
				// attach it to the node
				RibbonNode = LKernel.Get<SceneManager>().RootSceneNode.CreateChildSceneNode(Name + ID + "RibbonNode");
				Ribbon.AddNode(Node);
				RibbonNode.AttachObject(Ribbon);
			}
		}
		#endregion Mogre stuff

		#region Physics
		/// <summary>
		/// Override this with the method you use to setup physics stuff
		/// </summary>
		protected abstract void SetUpPhysics();

		#endregion

		#region Lua stuff
		/// <summary>
		/// Runs a script when this thing is done creating everything. If Script is null, then this doesn't do anything.
		/// </summary>
		protected void RunScript() {
			if (Script != null) {
				LKernel.Get<Lua.LuaMain>().DoFile(Script);
			}
		}
		#endregion

		#region Methods to get optional properties
		/// <summary>
		/// Gets an Enumerable of all of the flag properties that are optional and are not the default.
		/// </summary>
		public virtual IEnumerable<KeyValuePair<string, bool>> GetOptionalFlags() {
			yield break;
		}

		/// <summary>
		/// Gets an enumerable of all of the number properties that are optional and are not the default.
		/// </summary>
		public virtual IEnumerable<KeyValuePair<string, float>> GetOptionalNumbers() {
			if (SpawnOrientation != 0)
				yield return new KeyValuePair<string, float>("Orientation", SpawnOrientation);
			if (MoveSpeed != DefaultMoveSpeed)
				yield return new KeyValuePair<string, float>("Speed", MoveSpeed);
			if (CollisionGroupID != DefaultCollisionGroupID)
				yield return new KeyValuePair<string, float>("CollisionGroup", CollisionGroupID);
		}

		/// <summary>
		/// Gets an enumerable of all of the string properties that are optional and are not the default.
		/// </summary>
		public virtual IEnumerable<KeyValuePair<string, string>> GetOptionalStrings() {
			if (Script != null)
				yield return new KeyValuePair<string, string>("Script", Script);
			if (Model != DefaultModel)
				yield return new KeyValuePair<string, string>("Model", Model);
			if (Material != DefaultMaterial)
				yield return new KeyValuePair<string, string>("Material", Material);
			if (MoveBehaviour != DefaultMoveBehaviour)
				yield return new KeyValuePair<string, string>("Behaviour", this.MoveBehaviour.ToString());
			// Name is not optional
		}
		
		/// <summary>
		/// Gets an enumerable of all of the vector properties that are optional and are not the default.
		/// </summary>
		public virtual IEnumerable<KeyValuePair<string, Vector3>> GetOptionalVectors() {
			// Position is not optional
			yield break;
		}
		#endregion

		#region IDisposable and ToString stuff
		/// <summary>
		/// Puts this thing into the disposal queue. Use this if you want to destroy things during runtime, as the 
		/// physics engine does not like it if you dispose stuff while it's busy. Queueing it makes sure that it's only disposed of
		/// after a physics "frame" and not during one.
		/// </summary>
		public void QueueDispose() {
			LKernel.Get<PhysXMain>().ThingsToDispose.Add(this);
		}

		/// <summary>
		/// Destroys this thing. Checks if all of its fields are null and if they aren't, runs their own destroy methods.
		/// This should be overridden if your thing has more fields it needs to destroy.
		/// </summary>
		public virtual void Dispose() {
			
			if (Entity != null) {
				if (LKernel.Get<LevelManager>().IsValidLevel)
					LKernel.Get<SceneManager>().DestroyEntity(Entity);
				Entity.Dispose();
				Entity = null;
			}
			if (Node != null) {
				if (LKernel.Get<LevelManager>().IsValidLevel)
					LKernel.Get<SceneManager>().DestroySceneNode(Node);
				Node.Dispose();
			}
			if (Ribbon != null && RibbonNode != null) {
				RibbonNode.DetachObject(Ribbon);
				foreach (SceneNode n in Ribbon.GetNodeIterator())
					Ribbon.RemoveNode(n);
				if (LKernel.Get<LevelManager>().IsValidLevel)
					LKernel.Get<SceneManager>().DestroyRibbonTrail(Ribbon);
				Ribbon.Dispose();
				Ribbon = null;
				RibbonNode = null;
			}
		}

		public override string ToString() {
			return Name + "#" + ID;
		}
		#endregion
	}
}
