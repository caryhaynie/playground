using System;
using PlayfulSoftware.HexMaps.Hybrid;
using UnityEngine;

namespace Tests
{
    sealed class HexMapGenerationParametersTestLoader : MonoBehaviour
    {
#pragma warning disable 0649
        public HexMapGenerationParameters parametersAsset;
#pragma warning restore 0649

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