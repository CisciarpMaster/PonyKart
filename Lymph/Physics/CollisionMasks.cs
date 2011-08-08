using System;

namespace Ponykart.Physics {
	/// <summary>
	/// more info here:
	/// http://bulletphysics.org/mediawiki-1.5.8/index.php/Collision_Filtering
	/// 
	/// TODO:
	/// - set up these
	/// - we'll probably need a custom NearCallback eventually for detecting when things collide, like when an item collides with a kart.
	/// </summary>
	public abstract class CollisionMasks {
		public static readonly int KartsCollideWith = (int) CollisionTypes.Karts | (int) CollisionTypes.Stuff | (int) CollisionTypes.Items | (int) CollisionTypes.Walls;
		public static readonly int StuffCollidesWith = (int) CollisionTypes.Karts | (int) CollisionTypes.Stuff | (int) CollisionTypes.Walls;
		public static readonly int ItemsCollideWith = (int) CollisionTypes.Karts | (int) CollisionTypes.Walls;
		public static readonly int WallsCollideWith = (int) CollisionTypes.Karts | (int) CollisionTypes.Stuff | (int) CollisionTypes.Items;
	}

	[Flags]
	public enum CollisionTypes {
		Nothing = 0,
		Karts = 1,
		Stuff = 2,
		Items = 4,
		Walls = 8
	}
}
