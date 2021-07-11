Shader "Shaders/OldFilm"
{
    HLSLINCLUDE
#include "Packages/com.yetman.render-pipelines.universal.postprocess/ShaderLibrary/Core.hlsl"

        TEXTURE2D_X(_MainTex);

    float _Blend;

	// The MIT License
	// Copyright © 2013 Javier Meseguer
	// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#define BLACK_AND_WHITE
#define LINES_AND_FLICKER
#define BLOTCHES
#define GRAIN

#define FREQUENCY 15.0

	float rand(float2 co) {
		return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
	}

	float rand(float c) {
		return rand(float2(c, 1.0));
	}

	float randomLine(float2 uv, float seed)
	{
		float b = 0.01 * rand(seed);
		float a = rand(seed + 1.0);
		float c = rand(seed + 2.0) - 0.5;
		float mu = rand(seed + 3.0);

		float l = 1.0;

		if (mu > 0.2)
			l = pow(abs(a * uv.x + b * uv.y + c), 1.0 / 8.0);
		else
			l = 2.0 - pow(abs(a * uv.x + b * uv.y + c), 1.0 / 8.0);

		return lerp(0.5, 1.0, l);
	}

	// Generate some blotches.
	float randomBlotch(float2 uv, float seed)
	{
		float x = rand(seed);
		float y = rand(seed + 1.0);
		float s = 0.01 * rand(seed + 2.0);

		float2 p = float2(x, y) - uv;
		p.x *= _ScreenSize.x / _ScreenSize.y;
		float a = atan2(p.y, p.x);
		float v = 1.0;
		float ss = s * s * (sin(6.2831 * a * x) * 0.1 + 1.0);

		if (dot(p, p) < ss) v = 0.2;
		else
			v = pow(abs(dot(p, p) - ss), 1.0 / 16.0);

		return lerp(0.3 + 0.2 * (1.0 - (s / 0.02)), 1.0, v);
	}

    float4 OldFilmFragmentProgram(PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        float2 pos = uv * _ScreenSize.xy;

        float4 origColor = LOAD_TEXTURE2D_X(_MainTex, pos);

		float3 newColor = float3(1, 0, 0);

		// Set frequency of global effect to 15 variations per second
		float t = float(int(_Time.y * FREQUENCY));

		// Get some image movement
		float2 suv = uv + 0.002 * float2(rand(t), rand(t + 23.0));

#ifdef BLACK_AND_WHITE
		// Convert it to B/W
		float luma = dot(float3(0.2126, 0.7152, 0.0722), origColor.rgb);
		float3 oldImage = luma * float3(0.7, 0.7, 0.7);
#else
		float3 oldImage = origColor.rgb;
#endif

		// Create a time-varying vignetting effect
		float vI = 16.0 * (uv.x * (1.0 - uv.x) * uv.y * (1.0 - uv.y));
		vI *= lerp(0.7, 1.0, rand(t + 0.5));

		// Add additive flicker
		vI += 1.0 + 0.4 * rand(t + 8.);

		// Add a fixed vignetting (independent of the flicker)
		vI *= pow(abs(16.0 * uv.x * (1.0 - uv.x) * uv.y * (1.0 - uv.y)), 0.4);

		// Add some random lines (and some multiplicative flicker. Oh well.)
#ifdef LINES_AND_FLICKER
		int l = int(8.0 * rand(t + 7.0));

		if (0 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(0));
		if (1 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(1));
		if (2 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(2));
		if (3 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(3));
		if (4 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(4));
		if (5 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(5));
		if (6 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(6));
		if (7 < l) vI *= randomLine(uv, t + 6.0 + 17. * float(7));

#endif

		// Add some random blotches.
#ifdef BLOTCHES
		int s = int(max(8.0 * rand(t + 18.0) - 2.0, 0.0));

		if (0 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(0));
		if (1 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(1));
		if (2 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(2));
		if (3 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(3));
		if (4 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(4));
		if (5 < s) vI *= randomBlotch(uv, t + 6.0 + 19. * float(5));

#endif

		// Show the image modulated by the defects
		newColor = oldImage * vI;

		// Add some grain (thanks, Jose!)
#ifdef GRAIN
		newColor *= (1.0 + (rand(uv + t * .01) - .2) * .15);
#endif		

        float4 color = float4(0,0,0,1);
        color.rgb = lerp(origColor.rgb, newColor, _Blend);

        return color;
    }


        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always
            Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment OldFilmFragmentProgram
            ENDHLSL
        }
    }
    Fallback Off
}
