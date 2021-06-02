#ifndef UNITY_CUSTOMFUNCTIONS_INCLUDED
#define UNITY_CUSTOMFUNCTIONS_INCLUDED

// Convert rgb to luminance
// with rgb in linear space with sRGB primaries and D65 white point
void Luminance_float(float3 linearRgb, out float luminance)
{
   luminance = dot(linearRgb, real3(0.2126729, 0.7151522, 0.0721750));
}
#endif // UNITY_CUSTOMFUNCTIONS_INCLUDED
