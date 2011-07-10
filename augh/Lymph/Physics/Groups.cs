using Lymph.Levels;
using Mogre.PhysX;

namespace Lymph.Phys {
	public class Groups {
		// these are also actor groups
		#region GroupIDs
		/*public static readonly uint WallID = 0;
		public static readonly uint PlayerID = 1;
		public static readonly uint ProjectileID = 2;
		public static readonly uint AttachmentID = 3;
		public static readonly uint PowerupID = 4;
		public static readonly uint EnemyID = 5;
		public static readonly uint FriendlyID = 6;
		public static readonly uint IgnoreEverythingID = 7;*/
		/// <summary>
		/// Nothing can collide with this. Everything goes right through it as if it wasn't there.
		/// </summary>
		public static readonly uint NonCollidableID = 1;
		/// <summary>
		/// Things can collide with this, but they won't be able to move it around. Ideal for things like trees, buildings, and NPCs.
		/// </summary>
		public static readonly uint CollidableNonPushableID = 2;
		/// <summary>
		/// Things can collide with this and they will push it around. Good for things like crates.
		/// </summary>
		public static readonly uint CollidablePushableID = 4;
		#endregion

		/// <summary>
		/// All collision groups
		/// </summary>
		public static uint AllGroups {
			get {
				return NonCollidableID | CollidableNonPushableID | CollidablePushableID;
			}
		}


		public Groups() {
			LKernel.Get<LevelManager>().OnLevelLoad += new LevelEventHandler(AddGroupCollisionFlags);
		}



		public void AddGroupCollisionFlags(LevelChangedEventArgs eventArgs) {
			Scene scene = LKernel.Get<PhysXMain>().Scene;

			// No collisions
			// These might not even be needed, since the group collision flags override actor group pair flags
			
			SetFlags(NonCollidableID, NonCollidableID, false, scene);
			SetFlags(NonCollidableID, CollidableNonPushableID, false, scene);
			SetFlags(NonCollidableID, CollidablePushableID, false, scene);
			// Yes collisions
			
			SetFlags(CollidableNonPushableID, CollidableNonPushableID, true, scene);
			SetFlags(CollidableNonPushableID, CollidablePushableID, true, scene);
			SetFlags(CollidablePushableID, CollidablePushableID, true, scene);
		}

		/// <summary>
		/// Shortcut method.
		/// <br />
		/// Sets the Group Collision Flags of this pair.
		/// Sets the Actor Group Pair Flags to "0" if collisions is false or "ContactPairFlags.NotifyOnStartTouch" if collisions is true.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="collisions">true if you want to respond to collisions, false otherwise</param>
		/// <param name="scene">The physx scene</param>
		private void SetFlags(uint a, uint b, bool collisions, Scene scene) {
			SetFlags(a, b, collisions, collisions ? ContactPairFlags.NotifyOnStartTouch : 0, scene); // 0 flag basically means no notifications
		}

		/// <summary>
		/// Sets the Group Collision Flags and the Actor Group Pair Flags of this pair.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="collisions">true if you want to respond to collisions, false otherwise</param>
		/// <param name="flags">flags</param>
		/// <param name="scene">The physx scene</param>
		private void SetFlags(uint a, uint b, bool collisions, ContactPairFlags flags, Scene scene) {
			scene.GroupCollisionFlags[a, b] = collisions;
			scene.GroupCollisionFlags[b, a] = collisions;
			scene.ActorGroupPairFlags[a, b] = flags;
			scene.ActorGroupPairFlags[b, a] = flags;
		}
	}
}
