using Mogre;

namespace Lymph.Actors {
    ///<summary> Base class for all Actor objects. </summary>
    public abstract class Actor {
        private float rotation = 0;
        private Face face;

		public SceneNode SceneNode { get; set; }
		public Entity Entity { get; set; }

        public Actor(SceneNode n, Entity e, Face f) {
            SceneNode= n;
            Entity = e;
            face = f;
        }

        public virtual void Destroy() {
            if (Entity != null)
                Launch.main.SceneMgr.DestroyEntity(Entity);
            if (SceneNode != null)
                Launch.main.SceneMgr.DestroySceneNode(SceneNode);
            if (face != null)
                face.Destroy();
        }

        public float Rotation {
            get { return this.rotation; }
            set {
                SceneNode.Yaw(this.rotation - value, Node.TransformSpace.TS_WORLD);
                this.rotation = value;
            }
        }

    }
}
