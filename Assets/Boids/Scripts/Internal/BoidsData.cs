using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Internal
{
    public static class Sz
    {
        public const int IntSz = sizeof(int);
        public const int FloatSz = sizeof(float);
        public const int Vector3Sz = FloatSz * 3;
        public const int Matrix4x4Sz = FloatSz * 16;
    }

    public struct InBoidsUnitData
    {
        public Vector3 Position;
        public Vector3 Direction;
        public InBoidsUnitData(Vector3 InPosition, Vector3 InDirection)
        {
            Position = InPosition;
            Direction = InDirection;
        }
        public const int Size = Sz.Vector3Sz * 2;
    };

    public struct OutBoidsUnitData
    {
        public int NeighborCount;
        public Vector3 CohesionVector;
        public Vector3 AlignmentVector;
        public Vector3 SeperationVector;
        public Vector3 ForcerVector;
        public const int Size = Sz.IntSz * 1 + Sz.Vector3Sz * 4;
    };

    public struct InForcerData
    {
        public float Weight;
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 LocalHalfExtents;
        public Matrix4x4 WorldToLocalMatrix;
        public InForcerData(float InWeight, Vector3 InPosition, Vector3 InDirection, Vector3 InLocalHalfExtents, Matrix4x4 InWorldToLocalMatrix)
        {
            Weight = InWeight;
            Position = InPosition;
            Direction = InDirection;
            LocalHalfExtents = InLocalHalfExtents;
            WorldToLocalMatrix = InWorldToLocalMatrix;
        }
        public const int Size = Sz.FloatSz * 1 + Sz.Vector3Sz * 3 + Sz.Matrix4x4Sz * 1;
    };

    public enum BoidsProcessing
    {
        CPU,
        GPU,
    }

    public enum BoidsPositionLock
    {
        None,
        Loop,
        ToCenter,
    }

    [System.Serializable]
    public class BoidsSettings
    {
        [Header("Processing")]
        public BoidsProcessing Processing = BoidsProcessing.CPU;

        [Header("Speed")]
        public float MinSpeed = 8;
        public float MaxSpeed = 15;
        public float MaxSteerForce = 8;

        [Header("Weight")]
        public float CohesionWeight = 1;
        public float AlignmentWeight = 2;
        public float SeperationWeight = 2.5f;

        [Header("Radiuse")]
        public float NeighborRadius = 2.5f;
        public float SeperateRadius = 1;

        [Header("Collision")]
        public float CollisionAvoidanceWeight = 20;
        public LayerMask CollisionLayer = ~0;
        public float CollisionRadius = 1;
        public float CollisionDistance = 20;

        [Header("Clamp To Area")]
        public float ClampWeight = 0;
        public float ClampDistance = 0;
    }

    [System.Serializable]
    public class BoidsForcers
    {
        [SerializeField]
        public List<BoidsDirectionalForcer> DirectionalForcers;

        [SerializeField]
        public List<BoidsPointForcer> PointForcers;

        [SerializeField]
        public List<BoidsTwowayForcer> TwowayForcers;
    }
}
