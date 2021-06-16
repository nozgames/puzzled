#ifndef UNITY_CUSTOMFUNCTIONS_INCLUDED
#define UNITY_CUSTOMFUNCTIONS_INCLUDED

// Convert rgb to luminance
// with rgb in linear space with sRGB primaries and D65 white point
void Luminance_float(float3 linearRgb, out float luminance)
{
   luminance = dot(linearRgb, real3(0.2126729, 0.7151522, 0.0721750));
}

float remap (float value, float2 from, float2 to) 
{
	float f = (value - from.x) / (from.y - from.x);
	return f * (to.y - to.x) + to.x;
}

void CalculateDecalUV_float (float2 UV, float2 Scale, float Rotation, float2 Offset, float BaseRotation, float MinWidth, float MaxWidth, float MinHeight, float MaxHeight, float2 Center, out float2 Out, out bool Clip) 
{
	float c = cos(BaseRotation);
	float s = sin(BaseRotation);
	float2 o = float2(Offset.x * c - Offset.y * s, Offset.x * s + Offset.y * c);

	c = cos (BaseRotation + Rotation);
	s = sin (BaseRotation + Rotation);

	float2 uv = (UV - (Center - float2(MaxWidth, MaxHeight) * 0.5 * Offset)) / (float2(MaxWidth-MinWidth, MaxHeight-MinHeight) * Scale + float2(MinWidth, MinHeight));
	Out.x = uv.x * c - uv.y * s;
	Out.y = uv.x * s + uv.y * c;
	Out += 0.5;

	// Should the decal be clipped?
	Clip = !(Out.x < 0 || Out.y < 0 || Out.x > 1.0 || Out.y > 1.0);
}

#endif // UNITY_CUSTOMFUNCTIONS_INCLUDED
