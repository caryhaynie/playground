using System;
using System.Diagnostics;
using UnityEngine;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public static class HexMetrics
    {
        public static bool initialized => parametersAsset;
        public static HexMapGenerationParameters parametersAsset { get; set; }

        [Conditional("DEBUG")]
        private static void VerifyParameterAssetAndThrow()
        {
#if DEBUG
            if (!parametersAsset)
                throw new Exception("A valid Hex Map Generation Parameters asset has not been set!");
#endif // DEBUG
        }

        public static float blendFactor
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.blendFactor;
            }
        }

        public static float cellPerturbStrength
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.cellPerturbStrength;
            }
        }

        public static int chunkSizeX
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.chunkSizeX;
            }
        }

        public static int chunkSizeZ
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.chunkSizeZ;
            }
        }

        public static float elevationPerturbStrength
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.elevationPerturbStrength;
            }
        }

        public static float elevationStep
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.elevationStep;
            }
        }

        public static float horizontalTerraceStepSize
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.horizontalTerraceStepSize;
            }
        }

        public static float innerRadius
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.innerRadius;
            }
        }

        public static float noiseScale
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.noiseScale;
            }
        }

        public static float outerRadius
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.outerRadius;
            }
        }

        public static float solidFactor
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.solidFactor;
            }
        }

        public static float streamBedElevationOffset
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.streamBedElevationOffset;
            }
        }

        public static int terracesPerSlope
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.terracesPerSlope;
            }
        }

        public static int terracedSteps
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.terracedSteps;
            }
        }

        public static float verticalTerraceStepSize
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.verticalTerraceStepSize;
            }
        }

        public static float waterBlendFactor
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.waterBlendFactor;
            }
        }

        public static float waterFactor
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.waterFactor;
            }
        }

        public static float waterSurfaceElevationOffset
        {
            get
            {
                VerifyParameterAssetAndThrow();
                return parametersAsset.waterSurfaceElevationOffset;
            }
        }

        public const float innerToOuter = HexMapGenerationParameters.innerToOuter;
        public const float outerToInner = HexMapGenerationParameters.outerToInner;


        public static Vector3 GetBridge(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetBridge(d);
        }

        public static Vector3 GetWaterBridge(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetWaterBridge(d);
        }

        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetEdgeType(elevation1, elevation2);
        }

        public static float[] GetFeatureThresholds(int level)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetFeatureThresholds(level);
        }

        public static Vector3 GetFirstCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetFirstCorner(d);
        }

        public static Vector3 GetFirstSolidCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetFirstSolidCorner(d);
        }

        public static Vector3 GetFirstWaterCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetFirstWaterCorner(d);
        }

        public static Vector3 GetSecondCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetSecondCorner(d);
        }

        public static Vector3 GetSecondSolidCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetSecondSolidCorner(d);
        }

        public static Vector3 GetSecondWaterCorner(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetSecondWaterCorner(d);
        }

        public static Vector3 GetSolidEdgeMiddle(HexDirection d)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.GetSolidEdgeMiddle(d);
        }

        public static Vector3 Perturb(Vector3 position)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.Perturb(position);
        }

        public static HashEntry SampleHashGrid(Vector3 position)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.SampleHashGrid(position);
        }

        public static Vector4 SampleNoise(Vector3 position)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.SampleNoise(position);
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.TerraceLerp(a, b, step);
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            VerifyParameterAssetAndThrow();
            return parametersAsset.TerraceLerp(a, b, step);
        }
    }
}
