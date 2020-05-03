#ifndef MESH_CUSTOM_FUNCTIONS_INCLUDED
#define MESH_CUSTOM_FUNCTIONS_INCLUDED

void PickUVFromColor_float(in float4 Color, out float2 UV)
{
#if defined(_FACES_X)
    UV = Color.yz * 255;
#elif defined(_FACES_Y)
    UV = Color.xz * 255;
#elif defined(_FACES_Z)
    UV = Color.xy * 255;
#endif
}

#endif // MESH_CUSTOM_FUNCTIONS_INCLUDED