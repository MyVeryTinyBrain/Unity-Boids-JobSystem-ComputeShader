using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsTwowayForcer : BoidsForcer
    {
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            float Length = LocalHalfExtents.z;
            float Width = Mathf.Min(LocalHalfExtents.x, LocalHalfExtents.y) * 0.1f;
            Vector3 TetrahedronSize = Vector3.one * Width * 2;
            BoidsHelper.DrawTwoSideArrow(BoidsHelper.GirdColor, transform.localToWorldMatrix, Length, Width, TetrahedronSize);
        }
    }
}
