using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Boids.Internal;

namespace Boids.Samples
{
    public class BoidsSpawner : MonoBehaviour
    {
        public Boids boids;
        public BoidsUnit BoidsUnitPrefab;
        public float Radius = 10;

        int ReservedCreate = 0;
        int ReservedDestroy = 0;
        List<BoidsUnit> Units;

        void Awake()
        {
            Units = new List<BoidsUnit>();
        }

        void LateUpdate()
        {
            for(int i = 0; i < ReservedCreate; ++i)
            {
                CreateNewUnit();
            }
            for(int i = 0; i < ReservedDestroy; ++i)
            {
                DestroyUnit();
            }
            ReservedCreate = 0;
            ReservedDestroy = 0;
        }

        void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (boids == null)
            {
                return;
            }

            DrawButtons();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(BoidsHelper.GirdColor.r, BoidsHelper.GirdColor.g, BoidsHelper.GirdColor.b, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, Radius);
        }

        BoidsUnit CreateNewUnit()
        {
            if (!BoidsUnitPrefab)
            {
                return null;
            }

            float LocalDistance = Random.Range(-Radius, Radius);
            Quaternion LocalRotaion = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            Vector3 WorldDirection = LocalRotaion * Vector3.forward;
            Vector3 LocalPosition = WorldDirection * LocalDistance;
            Vector3 WorldPosition = transform.TransformPoint(LocalPosition);
            Quaternion WorldRotation = transform.rotation * LocalRotaion;

            BoidsUnit NewUnit = Instantiate<BoidsUnit>(BoidsUnitPrefab);
            NewUnit.transform.position = WorldPosition;
            NewUnit.transform.rotation = WorldRotation;

            boids.RegistUnit(NewUnit);
            Units.Add(NewUnit);

            return NewUnit;
        }

        void DestroyUnit()
        {
            if (Units.Count > 0)
            {
                Destroy(Units[Units.Count - 1].gameObject);
                Units.RemoveAt(Units.Count - 1);
            }
        }

        void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create 1 Unit", GUILayout.Width(200)))
            {
                ReservedCreate += 1;
            }
            if (GUILayout.Button("Destroy 1 Unit", GUILayout.Width(200)))
            {
                ReservedDestroy += 1;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create 10 Units", GUILayout.Width(200)))
            {
                ReservedCreate += 10;
            }
            if (GUILayout.Button("Destroy 10 Unit", GUILayout.Width(200)))
            {
                ReservedDestroy += 10;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create 100 Units", GUILayout.Width(200)))
            {
                ReservedCreate += 100;
            }
            if (GUILayout.Button("Destroy 100 Unit", GUILayout.Width(200)))
            {
                ReservedDestroy += 100;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("CPU", GUILayout.Width(100)))
            {
                boids.settings.Processing = BoidsProcessing.CPU;
            }
            if (GUILayout.Button("GPU", GUILayout.Width(100)))
            {
                boids.settings.Processing = BoidsProcessing.GPU;
            }
            GUILayout.EndHorizontal();
            GUIStyle FontStyle = new GUIStyle();
            FontStyle.fontSize = 20;
            GUILayout.Label($"Units: {boids.units.Length}", FontStyle);
            GUILayout.Label($"[{boids.settings.Processing}] FPS: {1.0f / Time.smoothDeltaTime}", FontStyle);
        }
    }
}
