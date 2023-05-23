using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Internal
{
    public class BoidsObject : MonoBehaviour
    {
        Transform mCachedTransform;

        public Transform CachedTransform => mCachedTransform;

        void SetupCahcedTransform()
        {
            mCachedTransform = transform;
        }

        protected virtual void Awake()
        {
            SetupCahcedTransform();
        }

        protected virtual void OnValidate()
        {
            SetupCahcedTransform();
        }
    }
}
