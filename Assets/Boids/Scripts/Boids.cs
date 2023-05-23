using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;
using Unity.Jobs;
using Unity.Collections;

namespace Boids
{
    public class Boids : BoidsObject
    {
        [SerializeField]
        BoidsSettings Settings;

        [Header("GPU Processing")]
        [SerializeField]
        ComputeShader BoidsComputeShader;

        [Header("Area")]
        public BoidsBox AreaBox;
        public BoidsPositionLock PositionLock = BoidsPositionLock.None;
        bool mHasCachedValidPositionLockBox;

        [Header("Forcer")]
        [SerializeField]
        BoidsForcers Forcers;

        List<BoidsUnit> Units;

        public BoidsSettings settings => Settings;
        public BoidsUnit[] units => Units.ToArray();
        public bool HasCachedValidPositionLockBox => mHasCachedValidPositionLockBox;

        public void RegistUnit(BoidsUnit InBoidsUnit)
        {
            Units.Add(InBoidsUnit);
            InBoidsUnit.Initialize(Settings);
        }

        public void UnregistUnit(BoidsUnit InBoidsUnit)
        {
            Units.Remove(InBoidsUnit);
        }

        protected override void Awake()
        {
            base.Awake();

            if (Settings == null)
            {
                Settings = new BoidsSettings();
            }

            if (Forcers == null)
            {
                Forcers = new BoidsForcers();
            }

            Units = new List<BoidsUnit>();

            Reset();
        }

        void Reset()
        {
            if (BoidsComputeShader == null)
            {
                BoidsComputeShader = Resources.Load<ComputeShader>("ComputeShaders/CS_BoidUnit");
            }
        }

        void Update()
        {
            mHasCachedValidPositionLockBox = (AreaBox != null) && (AreaBox.isActiveAndEnabled);

            switch (settings.Processing)
            {
                case BoidsProcessing.CPU:
                UpdateBoidsUnits_CPU();
                break;

                case BoidsProcessing.GPU:
                if (!UpdateBoidsUnits_GPU())
                {
                    UpdateBoidsUnits_CPU();
                }
                break;
            }
        }

        bool UpdateBoidsUnits_GPU()
        {
            // Can't Update if Doesn't Support Compute Shaders

            if (!SystemInfo.supportsComputeShaders || !BoidsComputeShader)
            {
                return false;
            }

            int ExecuteKernel = BoidsComputeShader.FindKernel("Execute");
            int BoidsUnitsCount = Units.Count;

            // Can't Compute if Unit Count is zero

            if (BoidsUnitsCount == 0)
            {
                return true;
            }

            // Setup Unit Datas

            InBoidsUnitData[] InputBoidsUnitDataArray = new InBoidsUnitData[BoidsUnitsCount];
            for (int i = 0; i < BoidsUnitsCount; ++i)
            {
                InputBoidsUnitDataArray[i] = Units[i].inputBoidsUnitData;
            }
            ComputeBuffer InputBoidsUnitDatas = new ComputeBuffer(BoidsUnitsCount, InBoidsUnitData.Size);
            InputBoidsUnitDatas.SetData(InputBoidsUnitDataArray);

            OutBoidsUnitData[] OutputBoidsUnitDataArray = new OutBoidsUnitData[BoidsUnitsCount];
            ComputeBuffer OutputBoidsUnitDatas = new ComputeBuffer(BoidsUnitsCount, OutBoidsUnitData.Size);

            BoidsComputeShader.SetInt("BoidsUnitsCount", BoidsUnitsCount);
            BoidsComputeShader.SetBuffer(ExecuteKernel, "InputBoidsUnitDatas", InputBoidsUnitDatas);
            BoidsComputeShader.SetBuffer(ExecuteKernel, "OutputBoidsUnitDatas", OutputBoidsUnitDatas);
            BoidsComputeShader.SetFloat("NeighborRadius", Settings.NeighborRadius);
            BoidsComputeShader.SetFloat("SeperateRadius", Settings.SeperateRadius);

            // Setup Directional Forcer Datas

            int DirectionalForcersCount = Forcers.DirectionalForcers.Count;
            ComputeBuffer InputDirectionalForcerDatas = null;
            bool DIRECTIONAL_FORCER = DirectionalForcersCount > 0;
            if (DIRECTIONAL_FORCER)
            {
                InForcerData[] InDirectionalForcerDataArray = new InForcerData[DirectionalForcersCount];
                for (int i = 0; i < DirectionalForcersCount; ++i)
                {
                    InDirectionalForcerDataArray[i] = Forcers.DirectionalForcers[i].inputForcerData;
                }

                InputDirectionalForcerDatas = new ComputeBuffer(DirectionalForcersCount, InForcerData.Size);
                InputDirectionalForcerDatas.SetData(InDirectionalForcerDataArray);
                BoidsComputeShader.SetInt("DirectionalForcersCount", DirectionalForcersCount);
                BoidsComputeShader.SetBuffer(ExecuteKernel, "InputDirectionalForcerDatas", InputDirectionalForcerDatas);
                BoidsComputeShader.EnableKeyword("DIRECTIONAL_FORCER");
            }
            else
            {
                BoidsComputeShader.DisableKeyword("DIRECTIONAL_FORCER");
            }

            // Setup Point Forcer Datas

            int PointForcersCount = Forcers.PointForcers.Count;
            ComputeBuffer InputPointForcerDatas = null;
            bool POINT_FORCER = PointForcersCount > 0;
            if (POINT_FORCER)
            {
                InForcerData[] InPointForcerDataArray = new InForcerData[PointForcersCount];
                for (int i = 0; i < PointForcersCount; ++i)
                {
                    InPointForcerDataArray[i] = Forcers.PointForcers[i].inputForcerData;
                }

                InputPointForcerDatas = new ComputeBuffer(PointForcersCount, InForcerData.Size);
                InputPointForcerDatas.SetData(InPointForcerDataArray);
                BoidsComputeShader.SetInt("PointForcersCount", PointForcersCount);
                BoidsComputeShader.SetBuffer(ExecuteKernel, "InputPointForcerDatas", InputPointForcerDatas);
                BoidsComputeShader.EnableKeyword("POINT_FORCER");
            }
            else
            {
                BoidsComputeShader.DisableKeyword("POINT_FORCER");
            }

            // Setup Twoway Forcer Datas

            int TwowayForcersCount = Forcers.TwowayForcers.Count;
            ComputeBuffer InputTwowayForcerDatas = null;
            bool TWOWAY_FORCER = TwowayForcersCount > 0;
            if (TWOWAY_FORCER)
            {
                InForcerData[] InTwowayForcerDataArray = new InForcerData[TwowayForcersCount];
                for (int i = 0; i < TwowayForcersCount; ++i)
                {
                    InTwowayForcerDataArray[i] = Forcers.TwowayForcers[i].inputForcerData;
                }

                InputTwowayForcerDatas = new ComputeBuffer(TwowayForcersCount, InForcerData.Size);
                InputTwowayForcerDatas.SetData(InTwowayForcerDataArray);
                BoidsComputeShader.SetInt("TwowayForcersCount", TwowayForcersCount);
                BoidsComputeShader.SetBuffer(ExecuteKernel, "InputTwowayForcerDatas", InputTwowayForcerDatas);
                BoidsComputeShader.EnableKeyword("TWOWAY_FORCER");
            }
            else
            {
                BoidsComputeShader.DisableKeyword("TWOWAY_FORCER");
            }

            // Compute

            uint ThreadX, ThreadY, ThreadZ;
            BoidsComputeShader.GetKernelThreadGroupSizes(ExecuteKernel, out ThreadX, out ThreadY, out ThreadZ);

            int ThreadGroupsX = Mathf.CeilToInt(BoidsUnitsCount / (float)ThreadX);
            BoidsComputeShader.Dispatch(ExecuteKernel, ThreadGroupsX, 1, 1);

            // Get Data And Update Units

            OutputBoidsUnitDatas.GetData(OutputBoidsUnitDataArray);
            for (int i = 0; i < BoidsUnitsCount; i++)
            {
                Units[i].UpdateBoidsUnit(this, OutputBoidsUnitDataArray[i]);
            }

            // Dispose Compute Buffers

            InputBoidsUnitDatas.Dispose();
            OutputBoidsUnitDatas.Dispose();

            if (DIRECTIONAL_FORCER)
            {
                InputDirectionalForcerDatas.Dispose();
            }
            if (POINT_FORCER)
            {
                InputPointForcerDatas.Dispose();
            }
            if (TWOWAY_FORCER)
            {
                InputTwowayForcerDatas.Dispose();
            }

            return true;
        }

        void UpdateBoidsUnits_CPU()
        {
            int BoidsUnitsCount = Units.Count;

            if (BoidsUnitsCount == 0)
            {
                return;
            }

            Job_BoidsUnit Job = new Job_BoidsUnit();

            // Setup Unit Datas

            Job.InputBoidsUnitDatas = new NativeArray<InBoidsUnitData>(BoidsUnitsCount, Allocator.TempJob);
            for (int i = 0; i < BoidsUnitsCount; ++i)
            {
                Job.InputBoidsUnitDatas[i] = Units[i].inputBoidsUnitData;
            }

            Job.OutputBoidsUnitDatas = new NativeArray<OutBoidsUnitData>(BoidsUnitsCount, Allocator.TempJob);

            Job.BoidsUnitsCount = BoidsUnitsCount;
            Job.NeighborRadius = Settings.NeighborRadius;
            Job.SeperateRadius = Settings.SeperateRadius;

            // Setup Directional Forcer Datas

            int DirectionalForcersCount = Forcers.DirectionalForcers.Count;
            Job.DirectionalForcersCount = DirectionalForcersCount;
            Job.InputDirectionalForcerDatas = new NativeArray<InForcerData>(DirectionalForcersCount, Allocator.TempJob);
            for (int i = 0; i < DirectionalForcersCount; ++i)
            {
                Job.InputDirectionalForcerDatas[i] = Forcers.DirectionalForcers[i].inputForcerData;
            }

            // Setup Point Forcer Datas

            int PointForcersCount = Forcers.PointForcers.Count;
            Job.PointForcersCount = PointForcersCount;
            Job.InputPointForcerDatas = new NativeArray<InForcerData>(PointForcersCount, Allocator.TempJob);
            for (int i = 0; i < PointForcersCount; ++i)
            {
                Job.InputPointForcerDatas[i] = Forcers.PointForcers[i].inputForcerData;
            }

            // Setup Twoway Forcer Datas

            int TwowayForcersCount = Forcers.TwowayForcers.Count;
            Job.TwowayForcersCount = TwowayForcersCount;
            Job.InputTwowayForcerDatas = new NativeArray<InForcerData>(TwowayForcersCount, Allocator.TempJob);
            for (int i = 0; i < TwowayForcersCount; ++i)
            {
                Job.InputTwowayForcerDatas[i] = Forcers.TwowayForcers[i].inputForcerData;
            }

            // Compute

            JobHandle SheduleJobDependency = new JobHandle();
            JobHandle ParallelHandle = Job.ScheduleParallel(BoidsUnitsCount, 1, SheduleJobDependency);
            ParallelHandle.Complete();

            // Get Data And Update Units

            for (int i = 0; i < BoidsUnitsCount; ++i)
            {
                Units[i].UpdateBoidsUnit(this, Job.OutputBoidsUnitDatas[i]);
            }

            // Dispose Native Arrays

            Job.InputBoidsUnitDatas.Dispose();
            Job.OutputBoidsUnitDatas.Dispose();
            Job.InputDirectionalForcerDatas.Dispose();
            Job.InputPointForcerDatas.Dispose();
            Job.InputTwowayForcerDatas.Dispose();
        }
    }
}
