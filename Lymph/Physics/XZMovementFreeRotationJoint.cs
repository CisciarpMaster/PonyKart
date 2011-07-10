using Lymph.Actors;
using Mogre;
using MogreNewt;
using System;

namespace Lymph.Phys {
    /// <summary>
    /// A joint that can rotate in all three axes but can only move along the X and Z axes.
    /// </summary>
	[Obsolete("Not used any more")]
    public class XZMovementFreeRotationJoint : CustomJoint {

        #region Fields

        Vector3 newGlobalPos;
        Thing actor;

        #endregion Fields

        /// <summary>
        /// A joint that can rotate in all three axes but can only move along the X and Z axes.
        /// </summary>
        /// <param name="body">The body to add this joint to</param>
        public XZMovementFreeRotationJoint(Body body, Thing actor)
            : base(6, body, null) {
            this.actor = actor;
            body.ForceCallback += new ForceCallbackHandler(LimitMaximumVelocity);
        }

        /// <summary>
        /// A callback that limits this body's maximum velocity.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="timeStep"></param>
        /// <param name="threadIndex"></param>
        void LimitMaximumVelocity(Body body, float timeStep, int threadIndex) {
			if (this != null && !this.m_body0.IsDisposed) {
				Vector3 veloc = body.Velocity;
				float mags = veloc.DotProduct(veloc);
				if (mags > (actor.MaxMoveSpeed * actor.MaxMoveSpeed)) {
					veloc.Normalise();
					veloc *= actor.MaxMoveSpeed;
					body.Velocity = veloc;
				}
			}
        }

        /*protected override void Dispose(bool __p1) {
            if (__p1) {
                try {
                    return;
                } finally {
                    base.Dispose(true);
                }
            }
            base.Dispose(false);
        }*/

        public override void SubmitConstraint(float timeStep, int threadIndex) {
			if (this != null && !this.m_body0.IsDisposed) {
				Vector3 globalPos = Vector3.ZERO;
				Quaternion globalOrient;
				// get the global position and orientation
				// Not sure if we need the orientation but we need the position at least
				this.m_body0.GetPositionOrientation(out globalPos, out globalOrient);
				// Set the Y of the new position to 0
				newGlobalPos = globalPos;
				newGlobalPos.y = 0f;
				// I wonder if there's a way to do this without needing the two rows. Oh well.
				// The first argument is the position we'd like to move, the second argument is where we'd like
				// to move it to, and the third argument says which vector we want to move it along
				addLinearRow(globalPos, newGlobalPos, Vector3.UNIT_Y);
			}
        }


        /// Don't delete me!
        /*public override void SubmitConstraint(float timeStep, int threadIndex) {
            Quaternion globalOrient0 = new Quaternion(),
                       globalOrient1 = new Quaternion();
            Vector3 globalPos0 = new Vector3(), 
                    globalPos1 = new Vector3();
            base.localToGlobal(this.localOrient0, this.localPos0, out globalOrient0, out globalPos0, 0);
            base.localToGlobal(this.localOrient1, this.localPos1, out globalOrient1, out globalPos1, 1);

            Vector3 appliedX0 = globalOrient0 * Vector3.UNIT_X, 
                    appliedY0 = globalOrient0 * Vector3.UNIT_Y, 
                    appliedX1 = globalOrient1 * Vector3.UNIT_X, 
                    appliedY1 = globalOrient1 * Vector3.UNIT_Y, 
                    appliedZ1 = globalOrient1 * Vector3.UNIT_Z;
            base.addLinearRow(globalPos0, globalPos1, appliedX0);

            Plane plane = new Plane(appliedX0, globalPos0);
            float distance = plane.GetDistance(globalPos1);
            if (distance > 9.9999997473787516E-05) {
                float accel = (float) (distance / timeStep);
                if (plane.GetSide(globalPos1) == Plane.Side.NEGATIVE_SIDE) {
                    accel = -accel;
                }
                base.setRowAcceleration(accel);
            }

            Vector3 xCrossProduct = appliedX0.CrossProduct(appliedX1);
            float squaredLength = xCrossProduct.SquaredLength;
            if (squaredLength > 1E-06f) {
                squaredLength = (float) Math.Sqrt(squaredLength);
                xCrossProduct.Normalise();
                Radian relativeAngleError = (float) Math.Asin(squaredLength);
                base.addAngularRow(relativeAngleError, xCrossProduct);
                Vector3 xCrossProductX1 = xCrossProduct.CrossProduct(appliedX1);
                base.addAngularRow(new Radian(0f), xCrossProductX1);
            } else {
                base.addAngularRow(new Radian(0f), appliedY1);
                base.addAngularRow(new Radian(0f), appliedZ1);
            }

            float yDotProduct = appliedY0.DotProduct(appliedY1);
            float yCrossProductDotted = appliedY0.CrossProduct(appliedY1).DotProduct(appliedX0);
            this.angle = new Radian((float) Math.Atan2(yCrossProductDotted, yDotProduct));
            if (this.limitsOn) {
                if (this.angle > this.max) {
                    base.addAngularRow(this.angle - this.max, appliedX0);
                    base.setRowStiffness(1f);
                } else if (this.angle < this.min) {
                    base.addAngularRow(this.angle - this.min, appliedX0);
                    base.setRowStiffness(1f);
                }
            } else if (this.accel != 0f) {
                base.addAngularRow(new Radian(0f), appliedX0);
                base.setRowAcceleration(this.accel);
                this.accel = 0f;
            }

        }*/
    }
}
