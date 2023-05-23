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
            return NewUnit;
        }

        void DrawButtons()
        {
            if (GUILayout.Button("Create 1 Unit", GUILayout.Width(200)))
            {
                CreateNewUnit();
            }
            if (GUILayout.Button("Create 10 Units", GUILayout.Width(200)))
            {
                for (int i = 0; i < 10; ++i)
                {
                    CreateNewUnit();
                }
            }
            if (GUILayout.Button("Create 100 Units", GUILayout.Width(200)))
            {
                for (int i = 0; i < 100; ++i)
                {
                    CreateNewUnit();
                }
            }
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
