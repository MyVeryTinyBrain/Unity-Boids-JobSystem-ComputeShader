using Boids.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidsTargetForcer : BoidsDirectionalForcer
    {
        public GameObject Target;

        protected override float recalculatedWeight => Target ? Weight : 0;
        protected override Vector3 recalculatedDirection => Target ? (Target.transform.position - CachedTransform.position).normalized : Vector3.zero;

        protected override void OnDrawArrowGizmos()
        {
            if (Target)
            {
                base.OnDrawArrowGizmos();
            }
        }
    }
}
