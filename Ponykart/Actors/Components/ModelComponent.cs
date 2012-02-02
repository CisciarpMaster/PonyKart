using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using PonykartParsers;

namespace Ponykart.Actors {
	/// <summary>
	/// Represents an ogre node and mesh
	/// </summary>
	public class ModelComponent : LDisposable {
		public SceneNode Node { get; protected set; }
		public Entity Entity { get; protected set; }
		public uint ID { get; protected set; }
		public string Name { get; protected set; }
		public AnimationBlender Animation { get; protected set; }
		public LThing Owner { get; protected set; }
		public readonly Vector3 SpawnPosition;
		public readonly Quaternion SpawnOrientation;
		public readonly Vector3 SpawnScale;

		/// <summary>
		/// Creates a model component for a Thing.
		/// </summary>
		/// <param name="lthing">The Thing this component is attached to</param>
		/// <param name="template">The template from the Thing</param>
		/// <param name="block">The block we're creating this component from</param>
		public ModelComponent(LThing lthing, ThingBlock template, ModelBlock block, ThingDefinition def) {
			ID = IDs.Incremental;
			Owner = lthing;
			var sceneMgr = LKernel.GetG<SceneManager>();

			Name = block.GetStringProperty("name", template.ThingName);

			// set these up here because static/instanced geometry might need them
			// position
			SpawnPosition = block.GetVectorProperty("position", Vector3.ZERO);

			// orientation
			SpawnOrientation = block.GetQuatProperty("orientation", Quaternion.IDENTITY);
			// if orientation was not found, we fall back to rotation
			if (SpawnOrientation == Quaternion.IDENTITY) {
				Vector3 rot = block.GetVectorProperty("rotation", Vector3.ZERO);
				if (rot != Vector3.ZERO)
					SpawnOrientation = rot.DegreeVectorToGlobalQuaternion();
			}
			// scale
			SpawnScale = block.GetVectorProperty("scale", Vector3.UNIT_SCALE);


			ThingEnum shad = block.GetEnumProperty("CastsShadows", ThingEnum.Some);
			// if we're static, set up the static geometry
			// don't set up static geometry if we want to cast shadows though, since static geometry doesn't work with shadows
			if ((block.GetBoolProperty("static", false) || def.GetBoolProperty("static", false))
				// make static if we never want shadows
				&& (shad == ThingEnum.None
				// or if the mesh has "some" shadows but we don't want any
					|| (shad == ThingEnum.Some && Options.ShadowDetail == ShadowDetailOption.None)
				// or if the mesh has "many" shadows but we only want those with "some"
					|| (shad == ThingEnum.Many && Options.ShadowDetail != ShadowDetailOption.Many)))
			{
				LKernel.GetG<StaticGeometryManager>().Add(this, template, block, def);
				Entity = null;
			}
			else if (block.GetBoolProperty("instanced", false) || def.GetBoolProperty("instanced", false)) {
				LKernel.GetG<InstancedGeometryManager>().Add(this, template, block, def);
				Entity = null;
			}
			// for attachments
			else if (block.GetBoolProperty("Attached", false)) {
				SetupEntity(sceneMgr, block);
				SetupAnimation(block);

				string boneName = block.GetStringProperty("AttachBone", null);
				int modelComponentID = (int) block.GetFloatProperty("AttachComponentID", null);
				Quaternion offsetQuat = block.GetQuatProperty("AttachOffsetOrientation", Quaternion.IDENTITY);
				Vector3 offsetVec = block.GetVectorProperty("AttachOffsetPosition", Vector3.ZERO);

				lthing.ModelComponents[modelComponentID].Entity.AttachObjectToBone(boneName, Entity, offsetQuat, offsetVec);

				//if (boneName == "Head") {
					//Vector3 v = lthing.ModelComponents[modelComponentID].Entity.Skeleton.GetBone(boneName)._getDerivedPosition();
					//System.Console.WriteLine(v);
				//}
			}
			// otherwise continue as normal
			else {
				Node = lthing.RootNode.CreateChildSceneNode(Name + "Node" + ID);

				Node.Position = SpawnPosition;
				Node.Orientation = SpawnOrientation;
				Node.Scale(SpawnScale);

				Node.InheritScale = block.GetBoolProperty("InheritScale", true);
				Node.InheritOrientation = block.GetBoolProperty("InheritOrientation", true);
				Node.SetInitialState();

				// make our entity
				SetupEntity(sceneMgr, block);

				SetupAnimation(block);

				// then attach it to the node!
				Node.AttachObject(Entity);
			}
		}

		/// <summary>
		/// Only does simple animations for now
		/// </summary>
		protected void SetupAnimation(ModelBlock block) {
			if (block.GetBoolProperty("animated", false)) {
				Animation = new AnimationBlender(Entity);
				Animation.Init(block.GetStringProperty("AnimationName", null), block.GetBoolProperty("AnimationLooping", true));

				LKernel.GetG<AnimationManager>().Add(Animation);
			}
		}

		protected void SetupEntity(SceneManager sceneMgr, ModelBlock block) {
			// make a new one if it isn't created yet, clone an existing one 
			string meshName = block.GetStringProperty("mesh", null);
			if (sceneMgr.HasEntity(meshName)) {
				Entity = sceneMgr.GetEntity(meshName).Clone(meshName + ID);
			}
			else {
				Entity = sceneMgr.CreateEntity(meshName, meshName);
			}

			if (block.FloatTokens.ContainsKey("renderingdistance"))
				Entity.RenderingDistance = block.GetFloatProperty("RenderingDistance", null);

			if (block.StringTokens.ContainsKey("skeleton")) {
				Entity.GetMesh().SkeletonName = block.GetStringProperty("Skeleton", null);
				Entity._initialise(true);
			}

			// material name
			string materialName = block.GetStringProperty("material", string.Empty);
			if (!string.IsNullOrWhiteSpace(materialName))
				Entity.SetMaterialName(materialName);

			// some other properties
			ThingEnum shad = block.GetEnumProperty("CastsShadows", ThingEnum.Some);
			if (Options.ShadowDetail == ShadowDetailOption.Many)
				Entity.CastShadows = (shad == ThingEnum.Many || shad == ThingEnum.Some);
			else if (Options.ShadowDetail == ShadowDetailOption.Some)
				Entity.CastShadows = (shad == ThingEnum.Some);
			else
				Entity.CastShadows = false;
		}

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			// stop updating the animation if we have one
			if (disposing && Animation != null) {
				LKernel.GetG<AnimationManager>().Remove(Animation);
			}

			var sceneMgr = LKernel.GetG<SceneManager>();
			bool valid = LKernel.GetG<LevelManager>().IsValidLevel;

			if (Entity != null) {
				if (valid && disposing)
					sceneMgr.DestroyEntity(Entity);
				Entity.Dispose();
				Entity = null;
			}
			if (Node != null) {
				if (valid && disposing)
					sceneMgr.DestroySceneNode(Node);
				Node.Dispose();
				Node = null;
			}

			base.Dispose(disposing);
		}

		public override string ToString() {
			return Node.Name;
		}
	}
}
