#ifndef HEXMAPS_TERRAIN_FUNCTIONS_INCLUDED
#define HEXMAPS_TERRAIN_FUNCTIONS_INCLUDED

void GetTerrainColor_float(float3 worldPos, float3 terrain, float3 color, TEXTURE2D_ARRAY_PARAM(Tex, sampler_Tex), int index, out float3 c)
{
    float2 uv = worldPos.xz * 0.02;
    c = SAMPLE_TEXTURE2D_ARRAY(Tex, sampler_Tex, uv, terrain[index]);
    c *= color[index];
}

#endif // HEXMAPS_TERRAIN_FUNCTIONS_INCLUDED