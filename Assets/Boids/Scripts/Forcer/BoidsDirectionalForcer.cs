using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsDirectionalForcer : BoidsForcer
    {
        protected virtual float recalculatedWeight => Weight;
        protected virtual Vector3 recalculatedDirection => CachedTransform.forward;

        public override InForcerData inputForcerData
        {
            get
            {
                return new InForcerData(
                        recalculatedWeight,
                        CachedTransform.position, recalculatedDirection, LocalHalfExtents,
                        CachedTransform.worldToLocalMatrix
                    );
            }
        }

        protected virtual void OnDrawArrowGizmos()
        {
            float Length = LocalHalfExtents.z;
            float Width = Mathf.Min(LocalHalfExtents.x, LocalHalfExtents.y) * 0.1f;
            Vector3 TetrahedronSize = Vector3.one * Width * 2;
            Matrix4x4 RotateMatrix = Matrix4x4.Rotate(Quaternion.FromToRotation(CachedTransform.forward, recalculatedDirection));
            BoidsHelper.DrawArrow(BoidsHelper.GirdColor, transform.localToWorldMatrix * RotateMatrix, Length, Width, TetrahedronSize);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            OnDrawArrowGizmos();
        }
    }
}
