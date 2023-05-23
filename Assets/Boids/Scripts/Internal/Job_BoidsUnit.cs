using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Boids.Internal
{
    [BurstCompile] 
    struct Job_BoidsUnit : IJobFor
    {
        const float MinDistance = 0.01f;

        [ReadOnly]
        public int BoidsUnitsCount;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<InBoidsUnitData> InputBoidsUnitDatas;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<OutBoidsUnitData> OutputBoidsUnitDatas;

        [ReadOnly]
        public float NeighborRadius;

        [ReadOnly]
        public float SeperateRadius;

        [ReadOnly]
        public int DirectionalForcersCount;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<InForcerData> InputDirectionalForcerDatas;

        [ReadOnly]
        public int PointForcersCount;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<InForcerData> InputPointForcerDatas;

        [ReadOnly]
        public int TwowayForcersCount;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<InForcerData> InputTwowayForcerDatas;

        public void Execute(int index)
        {
            OutBoidsUnitData Output = new OutBoidsUnitData();

            // Weight Processing

            int NeighborCount = 0;
            Vector3 PositionSum = Vector3.zero;
            Vector3 DirectionSum = Vector3.zero;
            Vector3 SeperationSum = Vector3.zero;

            float SqrNeighborRadius = NeighborRadius * NeighborRadius;
            float SqrSeperateRadius = SeperateRadius * SeperateRadius;

            for (int i = 0; i < BoidsUnitsCount; ++i)
            {
                if (index != i)
                {
                    Vector3 OtherToThis = InputBoidsUnitDatas[index].Position - InputBoidsUnitDatas[i].Position;
                    float SqrDistance = (OtherToThis.x * OtherToThis.x) + (OtherToThis.y * OtherToThis.y) + (OtherToThis.z * OtherToThis.z);
                    SqrDistance = Mathf.Max(MinDistance, SqrDistance);

                    if (SqrDistance < SqrNeighborRadius)
                    {
                        NeighborCount += 1;
                        PositionSum += InputBoidsUnitDatas[i].Position;
                        DirectionSum += InputBoidsUnitDatas[i].Direction;

                        if (SqrDistance < SqrSeperateRadius)
                        {
                            SeperationSum += OtherToThis / SqrDistance;
                        }
                    }
                }
            }

            Vector3 CohesionCenter = PositionSum / NeighborCount;
            Output.NeighborCount = NeighborCount;
            Output.CohesionVector = CohesionCenter - InputBoidsUnitDatas[index].Position;
            Output.AlignmentVector = DirectionSum;
            Output.SeperationVector = SeperationSum;

            // Forcer Processing

            Vector3 ForcerVector = Vector3.zero;

            for (int i = 0; i < DirectionalForcersCount; ++i)
            {
                if (Contains(InputBoidsUnitDatas[index].Position, InputDirectionalForcerDatas[i]))
                {
                    Vector3 Force = InputDirectionalForcerDatas[i].Direction * InputDirectionalForcerDatas[i].Weight;
                    ForcerVector += Force;
                }
            }

            for (int i = 0; i < PointForcersCount; ++i)
            {
                if (Contains(InputBoidsUnitDatas[index].Position, InputPointForcerDatas[i]))
                {
                    Vector3 ForcerToUnitDirection = (InputBoidsUnitDatas[index].Position - InputPointForcerDatas[i].Position).normalized;
                    Vector3 Force = ForcerToUnitDirection * InputPointForcerDatas[i].Weight;
                    ForcerVector += Force;
                }
            }

            for (int i = 0; i < TwowayForcersCount; ++i)
            {
                if (Contains(InputBoidsUnitDatas[index].Position, InputTwowayForcerDatas[i]))
                {
                    Vector3 ForceDirection = Vector3.Project(InputBoidsUnitDatas[index].Direction, InputTwowayForcerDatas[i].Direction);
                    Vector3 Force = ForceDirection * InputTwowayForcerDatas[i].Weight;
                    ForcerVector += Force;
                }
            }

            Output.ForcerVector = ForcerVector;

            OutputBoidsUnitDatas[index] = Output;
        }

        bool Contains(Vector3 InWorldPoint, InForcerData InputForcerData)
        {
            Vector3 LocalPoint = InputForcerData.WorldToLocalMatrix.MultiplyPoint(InWorldPoint);
            Vector3 AbsLocalPoint = LocalPoint.Abs();
            return
                AbsLocalPoint.x < InputForcerData.LocalHalfExtents.x &&
                AbsLocalPoint.y < InputForcerData.LocalHalfExtents.y &&
                AbsLocalPoint.z < InputForcerData.LocalHalfExtents.z;
        }
    }
}