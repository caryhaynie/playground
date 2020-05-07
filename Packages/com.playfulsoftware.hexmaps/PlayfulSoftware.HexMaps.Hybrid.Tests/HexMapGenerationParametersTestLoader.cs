using System;
using PlayfulSoftware.HexMaps.Hybrid;
using UnityEngine;

namespace Tests
{
    public sealed class HexMapGenerationParametersTestLoader : MonoBehaviour
    {
        public HexMapGenerationParameters parametersAsset;

        void OnDisable()
        {
            HexMetrics.parametersAsset = null;
        }

        void OnEnable()
        {
            HexMetrics.parametersAsset = parametersAsset;
        }
    }
}