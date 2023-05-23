using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids
{
    public class BoidsUnit : BoidsObject
    {
        Vector3 Velocity;
        Vector3 CachedPosition;
        Vector3 CachedForward;

        public InBoidsUnitData inputBoidsUnitData => new InBoidsUnitData(CachedTransform.position, CachedTransform.forward);

        public void Initialize(BoidsSettings InSettings)
        {
            float Speed = (InSettings.MinSpeed + InSettings.MaxSpeed) * 0.5f;
            Velocity = CachedTransform.forward * Speed;
        }

        public void UpdateBoidsUnit(Boids InBoids, OutBoidsUnitData InData)
        {
            CachedPosition = CachedTransform.position;
            CachedForward = CachedTransform.forward;
            Vector3 Acceleration = CalculateAcceleration(InBoids, InData);
            ApplyAcceleration(InBoids, CachedPosition, Acceleration);
        }

        Vector3 CalculateAcceleration(Boids InBoids, OutBoidsUnitData InData)
        {
            BoidsSettings Settings = InBoids.settings;
            Vector3 Acceleration = Vector3.zero;

            if (InData.NeighborCount > 0)
            {
                Vector3 CohesionForce = SteerTowards(Settings, InData.CohesionVector) * Settings.CohesionWeight;
                Vector3 AlignmentForce = SteerTowards(Settings, InData.AlignmentVector) * Settings.AlignmentWeight;
                Vector3 SeperationForce = SteerTowards(Settings, InData.SeperationVector) * Settings.SeperationWeight;

                Acceleration += CohesionForce;
                Acceleration += AlignmentForce;
                Acceleration += SeperationForce;
            }

            if (SphereCast(Settings, CachedPosition, CachedForward))
            {
                Vector3 CollisionAvoidanceDirection = CalculateCollisionAvoidanceDirection(Settings, CachedPosition, CachedTransform.localToWorldMatrix);
                Vector3 CollisionAvoidanceForce = SteerTowards(Settings, CollisionAvoidanceDirection) * Settings.CollisionAvoidanceWeight;
                Acceleration += CollisionAvoidanceForce;
            }

            if (Settings.ClampWeight > 0 && InBoids.HasCachedValidPositionLockBox) 
            {
                Vector3 ClampDireciton = CalculateClampDireciton(InBoids, CachedPosition);
                Vector3 ClampForce = SteerTowards(Settings, ClampDireciton) * Settings.ClampWeight;
                Acceleration += ClampForce;
            }

            {
                float ForcerSpeed = InData.ForcerVector.magnitude;
                Vector3 ForcerForce = SteerTowards(Settings, InData.ForcerVector) * ForcerSpeed;
                Acceleration += ForcerForce;
            }

            return Acceleration;
        }

        void ApplyAcceleration(Boids InBoids, Vector3 InPosition, Vector3 InAcceleration)
        {
            Velocity += InAcceleration * Time.deltaTime;
            Vector3 Direction = Velocity.normalized;
            float Speed = Mathf.Clamp(Velocity.magnitude, InBoids.settings.MinSpeed, InBoids.settings.MaxSpeed);
            Velocity = Direction * Speed;

            Vector3 Position = InPosition + Velocity * Time.deltaTime;
            Position = LockPosition(InBoids, Position);

            CachedTransform.position = Position;
            CachedTransform.forward = Direction;
        }

        bool SphereCast(BoidsSettings InSettings, Vector3 Position, Vector3 Direction)
        {
            Ray R = new Ray(Position - Direction * InSettings.CollisionRadius * 2, Direction);
            return Physics.SphereCast(R, InSettings.CollisionRadius, InSettings.CollisionDistance + InSettings.CollisionRadius * 2, InSettings.CollisionLayer);
        }

        Vector3 CalculateCollisionAvoidanceDirection(BoidsSettings InSettings, Vector3 Position, Matrix4x4 LocalToWorld)
        {
            Vector3[] RayDirections = BoidsHelper.SphereDirections;

            for (int i = 0; i < RayDirections.Length; i++)
            {
                Vector3 Direction = LocalToWorld.MultiplyVector(RayDirections[i]);
                if (SphereCast(InSettings, Position, Direction) == false)
                {
                    return Direction;
                }
            }

            return CachedTransform.forward;
        }

        Vector3 SteerTowards(BoidsSettings InSettings, Vector3 InVector)
        {
            Vector3 V = InVector.normalized * InSettings.MaxSpeed - Velocity;
            return Vector3.ClampMagnitude(V, InSettings.MaxSteerForce);
        }

        Vector3 LockPosition(Boids InBoids, Vector3 InWorldPosition)
        {
            if (InBoids.PositionLock == BoidsPositionLock.None || InBoids.HasCachedValidPositionLockBox == false)
            {
                return InWorldPosition;
            }

            Vector3 LocalPositionOfBox = InBoids.AreaBox.CachedTransform.InverseTransformPoint(InWorldPosition);
            Vector3 LockedLocalPositionOfBox = LocalPositionOfBox;

            switch (InBoids.PositionLock)
            {
                case BoidsPositionLock.Loop:
                LockedLocalPositionOfBox = LockLocalPositionUsingLoop(InBoids.AreaBox.LocalHalfExtents, LocalPositionOfBox);
                break;

                case BoidsPositionLock.ToCenter:
                LockedLocalPositionOfBox = LockLocalPositionUsingToCenter(InBoids.AreaBox.LocalHalfExtents, LocalPositionOfBox);
                break;
            }

            if(LocalPositionOfBox == LockedLocalPositionOfBox)
            {
                return InWorldPosition;
            }
            else
            {
                return InBoids.AreaBox.CachedTransform.TransformPoint(LockedLocalPositionOfBox);
            }
        }

        Vector3 LockLocalPositionUsingLoop(Vector3 InLocalHalfExtents, Vector3 InLocalPosition)
        {
            Vector3 AbsLocalPosition = InLocalPosition.Abs();

            if (AbsLocalPosition.x > InLocalHalfExtents.x ||
                AbsLocalPosition.y > InLocalHalfExtents.y ||
                AbsLocalPosition.z > InLocalHalfExtents.z)
            {
                Vector3 SignLocalPosition = InLocalPosition.Sign();
                AbsLocalPosition = AbsLocalPosition.Clamp(Vector3.zero, InLocalHalfExtents);
                return Vector3.Scale(-AbsLocalPosition, SignLocalPosition);
            }
            else
            {
                return InLocalPosition;
            }
        }

        Vector3 LockLocalPositionUsingToCenter(Vector3 InLocalHalfExtents, Vector3 InLocalPosition)
        {
            Vector3 AbsLocalPosition = InLocalPosition.Abs();

            if (AbsLocalPosition.x > InLocalHalfExtents.x ||
                AbsLocalPosition.y > InLocalHalfExtents.y ||
                AbsLocalPosition.z > InLocalHalfExtents.z)
            {
                return Vector3.zero;
            }
            else
            {
                return InLocalPosition;
            }
        }

        Vector3 CalculateClampDireciton(Boids InBoids, Vector3 InPosition)
        {
            Vector3 LocalInBoidsArea = InBoids.AreaBox.CachedTransform.InverseTransformPoint(InPosition);

            Vector3 AbsLocalPosition = LocalInBoidsArea.Abs();

            Vector3 ClampHalfExtents = InBoids.AreaBox.LocalHalfExtents - Vector3.one * InBoids.settings.ClampDistance;
            ClampHalfExtents.Scale(InBoids.AreaBox.CachedTransform.localScale);

            if (AbsLocalPosition.x > ClampHalfExtents.x ||
                AbsLocalPosition.y > ClampHalfExtents.y ||
                AbsLocalPosition.z > ClampHalfExtents.z)
            {
                Vector3 InvSign = LocalInBoidsArea.Sign() * (-1);

                if (AbsLocalPosition.x > ClampHalfExtents.x)
                {
                    InvSign.x *= (-1);
                }
                if (AbsLocalPosition.y > ClampHalfExtents.y)
                {
                    InvSign.y *= (-1);
                }
                if (AbsLocalPosition.z > ClampHalfExtents.z)
                {
                    InvSign.z *= (-1);
                }

                AbsLocalPosition = AbsLocalPosition.Clamp(Vector3.zero, ClampHalfExtents);

                Vector3 TargetLocalPosition = Vector3.Scale(AbsLocalPosition, InvSign);
                return (TargetLocalPosition - LocalInBoidsArea).normalized;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}
