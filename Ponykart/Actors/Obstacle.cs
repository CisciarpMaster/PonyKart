using BulletSharp;
using Ponykart.Physics;

namespace Ponykart.Actors {
	public class Obstacle : DynamicThing {
		protected override string DefaultMaterial {
			get { return "yellowbrick"; }
		}
		protected override string DefaultModel {
			get { return "primitives/box.mesh"; }
		}
		protected override CollisionShape CollisionShape {
			get { return new BoxShape(SpawnScale); }
		}
		protected override float Mass {
			get { return 0; } // immovable
		}
		protected override CollisionTypes CollisionType {
			get { return CollisionTypes.Walls; }
		}
		protected override int CollidesWith {
			get { return CollisionMasks.WallsCollideWith; }
		}


		public Obstacle(ThingTemplate tt) : base(tt) { }

	}
}
