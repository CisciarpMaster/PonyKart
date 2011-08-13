using System.Collections.Generic;
using BulletSharp;

namespace Ponykart.Physics {
	/// <summary>
	/// Extensions for physics objects
	/// </summary>
	static class PhysicsExtensions {
		#region CollisionObject

		private static IDictionary<CollisionObject, string> CollisionObjectNames = new Dictionary<CollisionObject, string>();

		/// <summary>
		/// Hackish method for getting a name from a collision object
		/// </summary>
		/// <returns>The name of the collision object if it has one, or "(NoName)" if it doesn't</returns>
		public static string GetName(this CollisionObject obj) {
			string name;
			if (CollisionObjectNames.TryGetValue(obj, out name)) {
				return name;
			}
			return "(NoName)";
		}

		/// <summary>
		/// Hackish method for assigning a name to a collision object
		/// </summary>
		/// <param name="newName">The new name of the collision object</param>
		public static void SetName(this CollisionObject obj, string newName) {
			CollisionObjectNames[obj] = newName;
		}

		//---------------------------------------------------------------------------

		private static IDictionary<CollisionObject, PonykartCollisionGroups> CollisionGroups = new Dictionary<CollisionObject, PonykartCollisionGroups>();

		/// <summary>
		/// Hackish method for getting a collision group from a collision object
		/// </summary>
		/// <returns>The group of the collision object if it has one, or Default if it doesn't</returns>
		public static PonykartCollisionGroups GetCollisionGroup(this CollisionObject obj) {
			PonykartCollisionGroups group;
			if (CollisionGroups.TryGetValue(obj, out group)) {
				return group;
			}
			return PonykartCollisionGroups.Default;
		}

		/// <summary>
		/// Hackish method for assigning a group to a collision object.
		/// Note that this does NOT affect the object's actual collision group and should only be used to store its group for later reference!
		/// </summary>
		/// <param name="newGroup">The group to store in this collision object.</param>
		public static void SetCollisionGroup(this CollisionObject obj, PonykartCollisionGroups newGroup) {
			CollisionGroups[obj] = newGroup;
		}
		#endregion

		#region World
		public static void AddRigidBody(this DynamicsWorld world, RigidBody body, PonykartCollisionGroups collisionGroup, PonykartCollidesWithGroups collidesWith) {
			body.SetCollisionGroup(collisionGroup);
			world.AddRigidBody(body, collisionGroup.ToBullet(), collidesWith.ToBullet());
		}

		public static void AddCollisionObject(this DynamicsWorld world, CollisionObject obj, PonykartCollisionGroups collisionGroup, PonykartCollidesWithGroups collidesWith) {
			obj.SetCollisionGroup(collisionGroup);
			world.AddCollisionObject(obj, collisionGroup.ToBullet(), collidesWith.ToBullet());
		}
		#endregion
	}
}
