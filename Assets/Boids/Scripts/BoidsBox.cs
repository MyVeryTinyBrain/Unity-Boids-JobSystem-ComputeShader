using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsBox : BoidsObject
    {
        public Vector3 LocalHalfExtents = Vector3.one;

        public Vector3 scaledHalfExtetns => transform.TransformVector(LocalHalfExtents);

        protected virtual void OnDrawGizmos()
        {
            BoidsHelper.DrawBox(BoidsHelper.GirdColor, transform.localToWorldMatrix, LocalHalfExtents);
        }
    }
}
