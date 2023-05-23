using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Internal
{
    public static class BoidsHelper
    {
        public static readonly Color GirdColor = Color.cyan;

        const int NumSphereDirections = 300;
        public static readonly Vector3[] SphereDirections;

        static BoidsHelper()
        {
            SphereDirections = new Vector3[BoidsHelper.NumSphereDirections];

            float GoldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float AngleIncrement = Mathf.PI * 2 * GoldenRatio;

            for (int i = 0; i < NumSphereDirections; i++)
            {
                float T = (float)i / NumSphereDirections;
                float Inclination = Mathf.Acos(1 - 2 * T);
                float Azimuth = AngleIncrement * i;

                float X = Mathf.Sin(Inclination) * Mathf.Cos(Azimuth);
                float Y = Mathf.Sin(Inclination) * Mathf.Sin(Azimuth);
                float Z = Mathf.Cos(Inclination);
                SphereDirections[i] = new Vector3(X, Y, Z);
            }
        }

        public static void DrawBox(Color InColor, Matrix4x4 InMatrix, Vector3 InHalfExtents)
        {
            Gizmos.color = InColor;
            Gizmos.matrix = InMatrix;

            Vector3 TopCap0 = new Vector3(-InHalfExtents.x, +InHalfExtents.y, +InHalfExtents.z);
            Vector3 TopCap1 = new Vector3(+InHalfExtents.x, +InHalfExtents.y, +InHalfExtents.z);
            Vector3 TopCap2 = new Vector3(+InHalfExtents.x, +InHalfExtents.y, -InHalfExtents.z);
            Vector3 TopCap3 = new Vector3(-InHalfExtents.x, +InHalfExtents.y, -InHalfExtents.z);
            Vector3 BottomCap0 = new Vector3(-InHalfExtents.x, -InHalfExtents.y, +InHalfExtents.z);
            Vector3 BottomCap1 = new Vector3(+InHalfExtents.x, -InHalfExtents.y, +InHalfExtents.z);
            Vector3 BottomCap2 = new Vector3(+InHalfExtents.x, -InHalfExtents.y, -InHalfExtents.z);
            Vector3 BottomCap3 = new Vector3(-InHalfExtents.x, -InHalfExtents.y, -InHalfExtents.z);

            Gizmos.DrawLine(TopCap0, TopCap1);
            Gizmos.DrawLine(TopCap1, TopCap2);
            Gizmos.DrawLine(TopCap2, TopCap3);
            Gizmos.DrawLine(TopCap2, TopCap3);
            Gizmos.DrawLine(TopCap3, TopCap0);

            Gizmos.DrawLine(BottomCap0, BottomCap1);
            Gizmos.DrawLine(BottomCap1, BottomCap2);
            Gizmos.DrawLine(BottomCap2, BottomCap3);
            Gizmos.DrawLine(BottomCap2, BottomCap3);
            Gizmos.DrawLine(BottomCap3, BottomCap0);

            Gizmos.DrawLine(TopCap0, BottomCap0);
            Gizmos.DrawLine(TopCap1, BottomCap1);
            Gizmos.DrawLine(TopCap2, BottomCap2);
            Gizmos.DrawLine(TopCap3, BottomCap3);
        }

        public static void DrawTetrahedron(Color InColor, Matrix4x4 InMatrix, Vector3 InTetrahedronHalfSize)
        {
            Gizmos.color = InColor;
            Gizmos.matrix = InMatrix;

            Vector3 Head = Vector3.forward * InTetrahedronHalfSize.z;
            Vector3 Back = -Head;
            Vector3 BackCap0 = Back + new Vector3(-InTetrahedronHalfSize.x, +InTetrahedronHalfSize.y, 0);
            Vector3 BackCap1 = Back + new Vector3(+InTetrahedronHalfSize.x, +InTetrahedronHalfSize.y, 0);
            Vector3 BackCap2 = Back + new Vector3(+InTetrahedronHalfSize.x, -InTetrahedronHalfSize.y, 0);
            Vector3 BackCap3 = Back + new Vector3(-InTetrahedronHalfSize.x, -InTetrahedronHalfSize.y, 0);

            Gizmos.DrawLine(Head, BackCap0);
            Gizmos.DrawLine(Head, BackCap1);
            Gizmos.DrawLine(Head, BackCap2);
            Gizmos.DrawLine(Head, BackCap3);

            Gizmos.DrawLine(BackCap0, BackCap1);
            Gizmos.DrawLine(BackCap1, BackCap2);
            Gizmos.DrawLine(BackCap2, BackCap3);
            Gizmos.DrawLine(BackCap2, BackCap3);
            Gizmos.DrawLine(BackCap3, BackCap0);
        }

        public static void DrawArrow(Color InColor, Matrix4x4 InMatrix, float InArrowLength, float InBoxWidth, Vector3 InTetrahedronSize)
        {
            Gizmos.color = InColor;
            Gizmos.matrix = InMatrix;

            float BoxLength = InArrowLength - InTetrahedronSize.z;
            Vector3 ToCenter = new Vector3(0, 0, -InTetrahedronSize.z * 0.5f);

            Matrix4x4 BoxMatrix = InMatrix * Matrix4x4.Translate(ToCenter);
            Vector3 BoxHalfExtents = new Vector3(InBoxWidth * 0.5f, InBoxWidth * 0.5f, BoxLength * 0.5f);
            BoidsHelper.DrawBox(InColor, BoxMatrix, BoxHalfExtents);

            // Outer Tetrahedron
            Vector3 OuterTetrahedronHalfSize = InTetrahedronSize * 0.5f;
            Vector3 OuterTetrahedronTranslate = new Vector3(0, 0, BoxLength * 0.5f + OuterTetrahedronHalfSize.z);
            Matrix4x4 OuterTetrahedronMatrix = InMatrix * Matrix4x4.Translate(OuterTetrahedronTranslate + ToCenter);
            BoidsHelper.DrawTetrahedron(InColor, OuterTetrahedronMatrix, OuterTetrahedronHalfSize);

            // Inner Tetrahedron
            Vector3 InnerTetrahedronHalfSize = new Vector3(OuterTetrahedronHalfSize.x * 0.5f, OuterTetrahedronHalfSize.y * 0.5f, OuterTetrahedronHalfSize.z);
            Vector3 InnerTetrahedronTranslate = OuterTetrahedronTranslate;
            Matrix4x4 InnerTetrahedronMatrix = OuterTetrahedronMatrix;
            BoidsHelper.DrawTetrahedron(InColor, InnerTetrahedronMatrix, InnerTetrahedronHalfSize);
        }

        public static void DrawTwoSideArrow(Color InColor, Matrix4x4 InMatrix, float InArrowLength, float InBoxWidth, Vector3 InTetrahedronSize)
        {
            Gizmos.color = InColor;
            Gizmos.matrix = InMatrix;

            float BoxLength = InArrowLength - InTetrahedronSize.z * 2;

            Vector3 BoxHalfExtents = new Vector3(InBoxWidth * 0.5f, InBoxWidth * 0.5f, BoxLength * 0.5f);
            BoidsHelper.DrawBox(InColor, InMatrix, BoxHalfExtents);

            {   // First Tetrahedron
                // Outer Tetrahedron
                Vector3 OuterTetrahedronHalfSize = InTetrahedronSize * 0.5f;
                Vector3 OuterTetrahedronTranslate = new Vector3(0, 0, BoxLength * 0.5f + OuterTetrahedronHalfSize.z * 2);
                Vector3 ToCenter = new Vector3(0, 0, -InTetrahedronSize.z * 0.5f);
                Matrix4x4 OuterTetrahedronMatrix = InMatrix * Matrix4x4.Translate(OuterTetrahedronTranslate + ToCenter);
                BoidsHelper.DrawTetrahedron(InColor, OuterTetrahedronMatrix, OuterTetrahedronHalfSize);

                // Inner Tetrahedron
                Vector3 InnerTetrahedronHalfSize = new Vector3(OuterTetrahedronHalfSize.x * 0.5f, OuterTetrahedronHalfSize.y * 0.5f, OuterTetrahedronHalfSize.z);
                Vector3 InnerTetrahedronTranslate = OuterTetrahedronTranslate;
                Matrix4x4 InnerTetrahedronMatrix = OuterTetrahedronMatrix;
                BoidsHelper.DrawTetrahedron(InColor, InnerTetrahedronMatrix, InnerTetrahedronHalfSize);
            }
            {   // Second Tetrahedron
                // Outer Tetrahedron
                Vector3 OuterTetrahedronHalfSize = InTetrahedronSize * 0.5f;
                Vector3 OuterTetrahedronTranslate = new Vector3(0, 0, -BoxLength * 0.5f - OuterTetrahedronHalfSize.z * 2);
                Vector3 ToCenter = new Vector3(0, 0, +InTetrahedronSize.z * 0.5f);
                Matrix4x4 OuterTetrahedronMatrix = InMatrix * Matrix4x4.Translate(OuterTetrahedronTranslate + ToCenter) * Matrix4x4.Rotate(Quaternion.FromToRotation(Vector3.forward, Vector3.back));
                BoidsHelper.DrawTetrahedron(InColor, OuterTetrahedronMatrix, OuterTetrahedronHalfSize);

                // Inner Tetrahedron
                Vector3 InnerTetrahedronHalfSize = new Vector3(OuterTetrahedronHalfSize.x * 0.5f, OuterTetrahedronHalfSize.y * 0.5f, OuterTetrahedronHalfSize.z);
                Vector3 InnerTetrahedronTranslate = OuterTetrahedronTranslate;
                Matrix4x4 InnerTetrahedronMatrix = OuterTetrahedronMatrix;
                BoidsHelper.DrawTetrahedron(InColor, InnerTetrahedronMatrix, InnerTetrahedronHalfSize);
            }
        }

        public static Vector3 Abs(this Vector3 InVector3)
        {
            return new Vector3(
                Mathf.Abs(InVector3.x),
                Mathf.Abs(InVector3.y),
                Mathf.Abs(InVector3.z));
        }

        public static Vector3 Sign(this Vector3 InVector3)
        {
            return new Vector3(
                Mathf.Sign(InVector3.x),
                Mathf.Sign(InVector3.y),
                Mathf.Sign(InVector3.z));
        }

        public static Vector3 Clamp(this Vector3 InVector3, Vector3 Min, Vector3 Max)
        {
            InVector3.x = Mathf.Clamp(InVector3.x, Min.x, Max.x);
            InVector3.y = Mathf.Clamp(InVector3.y, Min.y, Max.y);
            InVector3.z = Mathf.Clamp(InVector3.z, Min.z, Max.z);
            return InVector3;
        }
    }
}
