using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsPointForcer : BoidsForcer
    {
        static Matrix4x4 RotateX90Matrix;
        static Matrix4x4 RotateY90Matrix;
        static BoidsPointForcer()
        {
            RotateX90Matrix = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
            RotateY90Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            float Length = LocalHalfExtents.z;
            float Width = Mathf.Min(LocalHalfExtents.x, LocalHalfExtents.y) * 0.1f;
            Vector3 TetrahedronSize = Vector3.one * Width * 2;

            BoidsHelper.DrawTwoSideArrow(BoidsHelper.GirdColor, transform.localToWorldMatrix, Length, Width, TetrahedronSize);
            BoidsHelper.DrawTwoSideArrow(BoidsHelper.GirdColor, transform.localToWorldMatrix * RotateX90Matrix, Length, Width, TetrahedronSize);
            BoidsHelper.DrawTwoSideArrow(BoidsHelper.GirdColor, transform.localToWorldMatrix * RotateY90Matrix, Length, Width, TetrahedronSize);
        }
    }
}
