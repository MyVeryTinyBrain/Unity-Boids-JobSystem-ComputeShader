using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsForcer : BoidsBox
    {
        public float Weight = 1;

        public virtual InForcerData inputForcerData => new InForcerData(
                Weight,
                CachedTransform.position, CachedTransform.forward, LocalHalfExtents,
                CachedTransform.worldToLocalMatrix
            );
    }
}
