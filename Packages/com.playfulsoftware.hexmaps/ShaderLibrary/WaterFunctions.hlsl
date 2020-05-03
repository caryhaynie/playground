#ifndef HEXMAPS_WATER_FUNCTIONS_INCLUDED
#define HEXMAPS_WATER_FUNCTIONS_INCLUDED

void Foam_float(float Shore, float2 WorldXZ, TEXTURE2D_PARAM(NoiseTexture, sampler_NoiseTexture), out float Foam)
{
    float2 noiseUV = WorldXZ * _Time.y + 0.25;
    float4 noise = SAMPLE_TEXTURE2D(NoiseTexture, sampler_NoiseTexture, noiseUV * 0.015);

    float distortion1 = noise.x * (1 - Shore);
    float foam1 = sin((Shore + distortion1) * 10 - _Time.y);
    foam1 *= foam1;

    float distortion2 = noise.y * (1 - Shore);
    float foam2 = sin((Shore + distortion2) * 10 - _Time.y + 2);
    foam2 *= foam2 * 0.7;

    Foam = max(foam1, foam2) * Shore;
}

void River_float(float2 RiverUV, TEXTURE2D_PARAM(NoiseTexture, sampler_NoiseTexture), out float River)
{
    float2 uv = RiverUV;
    uv.x = uv.x * 0.0625 + _Time.y * 0.005;
    uv.y -= _Time.y * 0.25;
    float4 noise = SAMPLE_TEXTURE2D(NoiseTexture, sampler_NoiseTexture, uv);

    float2 uv2 = RiverUV;
    uv2.x = uv2.x * 0.0625 - _Time.y * 0.0052;
    uv.y -= _Time.y * 0.23;
    float4 noise2 = SAMPLE_TEXTURE2D(NoiseTexture, sampler_NoiseTexture, uv2);

    River = noise.x * noise2.w;
}

void Waves_float(float2 WorldXZ, TEXTURE2D_PARAM(NoiseTexture, sampler_NoiseTexture), out float Waves)
{
    float2 uv1 = WorldXZ;
    uv1.y += _Time.y;
    float4 noise1 = SAMPLE_TEXTURE2D(NoiseTexture, sampler_NoiseTexture, uv1 * 0.025);

    float2 uv2 = WorldXZ;
    uv2.x += _Time.y;
    float4 noise2 = SAMPLE_TEXTURE2D(NoiseTexture, sampler_NoiseTexture, uv2 * 0.025);

    float blendWave = sin(
        (WorldXZ.x + WorldXZ.y) * 0.1 +
        (noise1.y + noise2.z) + _Time.y);
    blendWave *= blendWave;

    float waves = lerp(noise1.z, noise1.w, blendWave) +
                lerp(noise2.x, noise2.y, blendWave);
    Waves = smoothstep(0.75, 2, waves);
}

#endif // HEXMAPS_WATER_FUNCTIONS_INCLUDED