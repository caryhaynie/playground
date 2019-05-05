using UnityEngine;

namespace PlayfulSoftware.HexMaps
{
    public interface INoiseSource
    {
        Vector4 SampleNoise(Vector3 position);
    }
}