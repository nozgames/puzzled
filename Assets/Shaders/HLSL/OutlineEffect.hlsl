#ifndef UNITY_OUTLINE_EFFECT_INCLUDED
#define UNITY_OUTLINE_EFFECT_INCLUDED

float OutlineSamplePixel(UnityTexture2D Texture, float2 UV, float2 texel, float x, float y)
{
	return tex2D(Texture, UV + float2(texel.x * x, texel.y * y)).w;
}


void OutlineSample_float(UnityTexture2D Texture, float2 UV, float2 Texel, out float o)
{
	float a = 0.0f;
	a += OutlineSamplePixel(Texture, UV, Texel, 0.0, 0.0);
	if (a == 1.0)
	{
		o = 0.0;
		return;
	}

	for (int x = -4; x <= 4; x++)
		for (int y = -4; y <= 4; y++)
			{
				a += OutlineSamplePixel(Texture, UV, Texel, x, y);
			}

	float w = a;
	if (w > 0.0f)
		o = 1.0;
	else
		o = 0.0;
}

#endif // UNITY_OUTLINE_EFFECT_INCLUDED
