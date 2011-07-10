using Ponykart.Phys;
using Ponykart.Stuff;
using Mogre;
using Mogre.PhysX;

namespace Ponykart.Actors {
	public class Obstacle : KinematicThing {
		protected override uint DefaultCollisionGroupID {
			get { return Groups.CollidableNonPushableID; }
		}
		protected override string DefaultMaterial {
			get { return "Fat"; }
		}
		protected override string DefaultModel {
			get { return "primitives/box.mesh"; }
		}
		protected override MoveBehaviour DefaultMoveBehaviour {
			get { return MoveBehaviour.IGNORE; }
		}
		protected override float DefaultMoveSpeed {
			get { return 0; }
		}
		protected override ShapeDesc ShapeDesc {
			get { return new BoxShapeDesc(new Vector3(0.5f, 0.5f, 0.5f)); }
		}

		public Obstacle(ThingTemplate tt) : base(tt) { }
	}
}
