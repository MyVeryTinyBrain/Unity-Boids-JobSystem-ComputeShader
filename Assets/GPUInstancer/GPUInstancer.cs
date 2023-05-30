using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancing
{
    struct InstancedMeshRendererKey
    {
        public Mesh mesh;
        public Material[] materials;

        public override bool Equals(object obj)
        {
            return Equals((InstancedMeshRendererKey)obj);
        }

        private bool Equals(InstancedMeshRendererKey key)
        {
            if (mesh == key.mesh && materials.Length == key.materials.Length)
            {
                for(int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != key.materials[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(mesh.GetHashCode(), materials[0].GetHashCode());
        }
    }

    public class GPUInstancer : MonoBehaviour
    {
        static GPUInstancer _instancer;

        Dictionary<InstancedMeshRendererKey, List<InstancedMeshRenderer>> Requests = new Dictionary<InstancedMeshRendererKey, List<InstancedMeshRenderer>>();

        static public GPUInstancer instancer
        {
            get
            {
                if (_instancer == null)
                {
                    GameObject newGameObject = new GameObject("GPUInstancing.Instancer");
                    _instancer = newGameObject.AddComponent<GPUInstancer>();
                }
                return _instancer;
            }
        }

        public void AddRequest(InstancedMeshRenderer renderer)
        {
            if (!renderer.IsValid())
                return;

            InstancedMeshRendererKey key = new InstancedMeshRendererKey()
            {
                mesh = renderer.mesh,
                materials = renderer.materials
            };

            List<InstancedMeshRenderer> list = null;
            if (!Requests.TryGetValue(key, out list))
            {
                list = new List<InstancedMeshRenderer>();
                Requests.Add(key, list);
            }
            list.Add(renderer);
        }

        private void LateUpdate()
        {
            DrawInstanced();
        }

        void DrawInstanced()
        {
            foreach (var group in Requests)
            {
                InstancedMeshRendererKey key = group.Key;
                List<InstancedMeshRenderer> list = group.Value;

                const int MaxInstanceCount = 1023;
                int drawCount = list.Count;
                int instanceCount = Mathf.CeilToInt(drawCount / 1023.0f);
                Matrix4x4[][] matrices = new Matrix4x4[instanceCount][];

                for (int i = 0; i < instanceCount; ++i)
                {
                    int start = i * MaxInstanceCount;
                    int end = Mathf.Min((i + 1) * MaxInstanceCount, start + (drawCount - start));
                    matrices[i] = new Matrix4x4[end - start];

                    int innerIdx = 0;
                    for (int j = start; j < end; ++j)
                    {
                        matrices[i][innerIdx] = list[j].transform.localToWorldMatrix;
                        innerIdx++;
                    }
                }

                for (int i = 0; i < key.mesh.subMeshCount; ++i)
                {
                    for (int j = 0; j < matrices.Length; ++j)
                    {
                        Graphics.DrawMeshInstanced(key.mesh, i, key.materials[i], matrices[j]);
                    }
                }
            }

            Requests.Clear();
        }
    }
}

