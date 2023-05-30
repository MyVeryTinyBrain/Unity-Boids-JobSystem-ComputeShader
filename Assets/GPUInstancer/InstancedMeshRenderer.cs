using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUInstancing
{
    [AllowPropertyChangeEventTarget(typeof(InstancedMeshRenderer))]
    class InstancedMeshRendererPropertyChangeEvent : AllowPropertyChangeEvent { }

    [ExecuteInEditMode] 
    public class InstancedMeshRenderer : MonoBehaviour
    {
        [SerializeField, PropertyChangeEvent(nameof(SetupMaterialArray)), PropertyChangeEvent(nameof(ApplyMeshRendererParameters))]
        private Mesh _mesh;

        [SerializeField, PropertyChangeEvent(nameof(ApplyMeshRendererParameters))]
        private Material[] _materials = new Material[0];

        [SerializeField, PropertyChangeEvent(nameof(ApplyMeshRendererParameters))]
        private ShadowCastingMode _castShadows = ShadowCastingMode.On;

        [SerializeField, PropertyChangeEvent(nameof(ApplyMeshRendererParameters))]
        private bool _receiveShadows = true;

        public Camera _camera;

        [SerializeField, PropertyChangeEvent(nameof(ApplyMeshRendererParameters))]
        private LightProbeUsage _lightProbes = LightProbeUsage.BlendProbes;

        [SerializeField, HideInInspector]
        private MeshFilter meshFilter;

        [SerializeField, HideInInspector]
        private MeshRenderer meshRenderer;

        public Mesh mesh
        {
            get => _mesh;
            set
            {
                _mesh = value;
                SetupMaterialArray();
                ApplyMeshRendererParameters();
            }
        }

        public Material[] materials
        {
            get => _materials;
            set
            {
                _materials = value;
                ApplyMeshRendererParameters();
            }
        }

        public Material material
        {
            get => _materials.Length > 0 ? _materials[0] : null;
            set
            {
                _materials[0] = value;
                ApplyMeshRendererParameters();
            }
        }

        public bool receiveShadows
        {
            get => _receiveShadows;
            set
            {
                _receiveShadows = value;
                ApplyMeshRendererParameters();
            }
        }

        public LightProbeUsage lightProbes
        {
            get => _lightProbes;
            set
            {
                _lightProbes = value;
                ApplyMeshRendererParameters();
            }
        }

        public bool IsValid()
        {
            return _mesh && _materials.Length > 0;
        }

        private void Awake()
        {
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.hideFlags = HideFlags.HideInInspector;
                ApplyMeshRendererParameters();
            }

            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.hideFlags = HideFlags.HideInInspector;
                ApplyMeshRendererParameters();
            }
        }

        private void OnEnable()
        {
            if (meshRenderer)
            {
                if (Application.isPlaying)
                    meshRenderer.enabled = false;
                else
                    meshRenderer.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (meshRenderer)
                meshRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (gameObject.activeInHierarchy)
            {
                if (meshRenderer)
                    DestroyImmediate(meshRenderer);
                if (meshFilter)
                    DestroyImmediate(meshFilter);
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                GPUInstancer.instancer.AddRequest(this);
            }
        }

        private void SetupMaterialArray()
        {
            int n = _mesh ? _mesh.subMeshCount : 0;
            Material[] newMaterials = new Material[n];

            for (int i = 0; i < n && i < _materials.Length; ++i)
            {
                newMaterials[i] = _materials[i];
            }

            _materials = newMaterials;
        }

        private void ApplyMeshRendererParameters()
        {
            if (meshFilter)
            {
                meshFilter.mesh = _mesh;
            }

            if (meshRenderer)
            {
                meshRenderer.sharedMaterials = _materials;
                meshRenderer.shadowCastingMode = _castShadows;
                meshRenderer.receiveShadows = _receiveShadows;
                meshRenderer.lightProbeUsage = _lightProbes;
            }
        }
    }
}