using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
#if UNITY_EDITOR
    using System;
    using UnityEditor;

    internal class CustomEditorLabelWidthScope : IDisposable
    {
        internal CustomEditorLabelWidthScope(float increment)
        {
            EditorGUIUtility.labelWidth += increment;
        }

        void IDisposable.Dispose()
        {
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    [CustomEditor(typeof(HexMapGenerationParameters))]
    internal sealed class HexMapGenerationParametersEditor : Editor
    {
        private SerializedProperty m_CellPerturbStrengthProp;
        private SerializedProperty m_ColorsProp;
        private SerializedProperty m_CornersProp;
        private SerializedProperty m_ChunkSizeXProp;
        private SerializedProperty m_ChunkSizeZProp;
        private SerializedProperty m_ElevationPerturbStrength;
        private SerializedProperty m_ElevationStepProp;
        private SerializedProperty m_NoiseScaleProp;
        private SerializedProperty m_NoiseSourceProp;
        private SerializedProperty m_OuterRadiusProp;
        private SerializedProperty m_SeedProp;
        private SerializedProperty m_SolidFactorProp;
        private SerializedProperty m_StreamBedElevationOffsetProp;

        void OnEnable()
        {
            m_CellPerturbStrengthProp = serializedObject.FindProperty("cellPerturbStrength");
            m_ColorsProp = serializedObject.FindProperty("m_Colors");
            m_CornersProp = serializedObject.FindProperty("m_Corners");
            m_ChunkSizeXProp = serializedObject.FindProperty("chunkSizeX");
            m_ChunkSizeZProp = serializedObject.FindProperty("chunkSizeZ");
            m_ElevationPerturbStrength = serializedObject.FindProperty("elevationPerturbStrength");
            m_ElevationStepProp = serializedObject.FindProperty("elevationStep");
            m_NoiseScaleProp = serializedObject.FindProperty("noiseScale");
            m_NoiseSourceProp = serializedObject.FindProperty("noiseSource");
            m_OuterRadiusProp = serializedObject.FindProperty("outerRadius");
            m_SeedProp = serializedObject.FindProperty("seed");
            m_SolidFactorProp = serializedObject.FindProperty("solidFactor");
            m_StreamBedElevationOffsetProp = serializedObject.FindProperty("streamBedElevationOffset");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Configurable Options:", EditorStyles.boldLabel);
            //DrawDefaultInspector();
            DrawConfigurationOptions();
            EditorGUILayout.LabelField("Derived Values:", EditorStyles.boldLabel);
            DrawDerivedValues();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawConfigurationOptions()
        {
            EditorGUILayout.LabelField("Map Size");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ChunkSizeXProp);
                EditorGUILayout.PropertyField(m_ChunkSizeZProp);
                EditorGUILayout.PropertyField(m_OuterRadiusProp);
            }

            EditorGUILayout.LabelField("Terrain Options");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ColorsProp);
            }

            EditorGUILayout.LabelField("Advanced Options");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ElevationStepProp);
                EditorGUILayout.PropertyField(m_SolidFactorProp);
            }

            EditorGUILayout.LabelField("Noise Options");
            using (new EditorGUI.IndentLevelScope())
            using (new CustomEditorLabelWidthScope(20f))
            {
                EditorGUILayout.PropertyField(m_NoiseSourceProp);
                EditorGUILayout.PropertyField(m_SeedProp);
                EditorGUILayout.PropertyField(m_NoiseScaleProp);
                EditorGUILayout.PropertyField(m_CellPerturbStrengthProp);
                EditorGUILayout.PropertyField(m_ElevationPerturbStrength);
            }
        }

        void DrawDerivedValues()
        {
            var obj = (HexMapGenerationParameters) target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.FloatField("Inner Radius", obj.innerRadius);
                var realExpanded = m_CornersProp.isExpanded;
                m_CornersProp.isExpanded = true;
                EditorGUILayout.PropertyField(m_CornersProp);
                m_CornersProp.isExpanded = realExpanded;
            }
        }
    }
#endif // UNITY_EDITOR
    [CreateAssetMenu(fileName = "MapParameters", menuName = "Hex Maps/Map Parameters", order = 8)]
    public sealed class HexMapGenerationParameters : ScriptableObject
    {
        // These ratios are intrinsic to hexmaps, so they can't be changed.
        public const float outerToInner = 0.866025404f;
        public const float innerToOuter = 1f / outerToInner;

        public float bridgeDesignLength = 7f;
        [Tooltip("Constant scale factor applied to random values sampled from noise texture")]
        public float cellPerturbStrength = 4f;
        [Tooltip("Number of chunks to generate, horizontally")]
        public int chunkSizeX = 5;
        [Tooltip("Number of chunks to generate, vertically")]
        public int chunkSizeZ = 5;
        public float elevationPerturbStrength = 1.5f;
        public float elevationStep = 3f;
        public float noiseScale = 0.003f;
        public float outerRadius = 10f;
        [Tooltip("Seed value to use when initializing the Pseudorandom Number Generator")]
        public int seed = 1234;
        public float solidFactor = 0.8f;
        public float streamBedElevationOffset = -1.75f;
        public int terracesPerSlope = 2;
        public float wallHeight = 4f;
        public float wallThickness = 0.75f;
        public float wallTowerThreshold = 0.75f;
        public float wallYOffset = -1f;
        public float waterFactor = 0.6f;
        public float waterSurfaceElevationOffset = -0.25f;

        public Texture2D noiseSource;

#pragma warning disable 0649
        [SerializeField] private Color[] m_Colors;
#pragma warning restore 0649
        [SerializeField] private Vector3[] m_Corners;

        private HashGrid<HashEntry> m_HashGrid;

        public float blendFactor => 1f - solidFactor;
        public float horizontalTerraceStepSize => 1f / terracedSteps;
        public float innerRadius => outerRadius * outerToInner; // sqrt(3) / 2
        public int terracedSteps => terracesPerSlope * 2 + 1;
        public float verticalTerraceStepSize => 1f / (terracesPerSlope + 1);
        public float wallElevationOffset => verticalTerraceStepSize;
        public float waterBlendFactor => 1f - waterFactor;

        private static float[][] featureThresholds =
        {
            new float[] { 0f, 0f, 0.4f},
            new float[] { 0f, 0.4f, 0.6f},
            new float[] { 0.4f, 0.6f, 0.8f}
        };

        void OnEnable()
        {
            InitializeHashGrid(seed);
        }

#if UNITY_EDITOR
        internal void OnValidate()
        {
            m_Corners = CalculateCorners(innerRadius, outerRadius);
        }

        void Reset()
        {

        }

#if UNITY_INCLUDE_TESTS
        // These methods is used by the editor tests suite.
        internal Vector3[] GetCorners() => m_Corners;
        internal HashGrid<HashEntry> GetHashGrid() => m_HashGrid;
#endif // UNITY_INCLUDE_TESTS
#endif // UNITY_EDITOR

        internal static Vector3[] CalculateCorners(float inner, float outer)
        {
            return new[] {
                new Vector3(0f, 0f, outer), // N
                new Vector3(inner, 0f,  0.5f * outer), // NE
                new Vector3(inner, 0f, -0.5f * outer), // SE
                new Vector3(0f, 0f, -outer), // S
                new Vector3(-inner, 0f, -0.5f * outer), // SW
                new Vector3(-inner, 0f,  0.5f * outer) // NW
            };
        }

        public Vector3 GetBridge(HexDirection d)
            => (GetFirstCorner(d) + GetSecondCorner(d)) * blendFactor;

        public Vector3 GetWaterBridge(HexDirection d)
            => (GetFirstCorner(d) + GetSecondCorner(d)) * waterBlendFactor;

        public HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            var delta = Mathf.Abs(elevation2 - elevation1);
            switch (delta)
            {
                case 0: // same elevation
                    return HexEdgeType.Flat;
                case 1:
                    return HexEdgeType.Slope;
                default:
                    return HexEdgeType.Cliff;
            }
        }

        public float[] GetFeatureThresholds(int level)
            => featureThresholds[level];

        public Vector3 GetFirstCorner(HexDirection d)
            => m_Corners[(int)d];

        public Vector3 GetFirstSolidCorner(HexDirection d)
            => GetFirstCorner(d) * solidFactor;

        public Vector3 GetFirstWaterCorner(HexDirection d)
            => GetFirstCorner(d) * waterFactor;

        public Vector3 GetSecondCorner(HexDirection d)
            => m_Corners[((int)d + 1) % m_Corners.Length];

        public Vector3 GetSecondSolidCorner(HexDirection d)
            => GetSecondCorner(d) * solidFactor;

        public Vector3 GetSecondWaterCorner(HexDirection d)
            => GetSecondCorner(d) * waterFactor;

        public Vector3 GetSolidEdgeMiddle(HexDirection d)
            => (m_Corners[(int) d] + m_Corners[((int)d + 1) % m_Corners.Length])
                   * (0.5f * solidFactor);

        public Color GetTerrainColor(int index) => m_Colors[index];

        public void InitializeHashGrid(int seed)
        {
            using (new RandomSeedScope(seed))
                m_HashGrid = new HashGrid<HashEntry>((_) => HashEntry.Create(), seed);
        }

        public Vector3 Perturb(Vector3 position)
        {
            var sample = SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
            return position;
        }

        public HashEntry SampleHashGrid(Vector3 position)
            => m_HashGrid?.Sample(position) ?? default(HashEntry);

        public Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale);
        }

        public Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            var h = step * horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            var v = (step + 1) / 2 * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }

        public Color TerraceLerp(Color a, Color b, int step)
        {
            var h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }

        public Vector3 WallLerp(Vector3 near, Vector3 far)
        {
            near.x += (far.x - near.x) * 0.5f;
            near.z += (far.z - near.z) * 0.5f;
            var v = (near.y < far.y)
                ? wallElevationOffset
                : (1f - wallElevationOffset);
            near.y += (far.y - near.y) * v + wallYOffset;
            return near;
        }

        public Vector3 WallThicknessOffset(Vector3 near, Vector3 far)
            => new Vector3(far.x - near.x, 0f, far.z - near.z)
                .normalized * (wallThickness * 0.5f);
    }
}
