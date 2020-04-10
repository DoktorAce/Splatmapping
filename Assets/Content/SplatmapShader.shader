Shader "SplatmapShader"
{
    Properties
    {
        _TextureArray ("Albedo (RGB)", 2DArray) = "white" {}
        _UVScale ("UVScale", Float) = 0.25
        _BlendDepth ("BlendDepth", Float) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 tex : TEXCOORD1;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };

            float _UVScale;
            float _BlendDepth;

            v2f vert (v2f IN)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(IN.vertex);
                o.uv = IN.uv * _UVScale;
                o.tex = IN.tex;
                o.color = IN.color;
                return o;
            }
            
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);

            half4 frag (v2f i) : SV_Target
            {
                half4 redTex = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, half3(i.uv.x, i.uv.y, i.tex.x));
                half redAlpha = (redTex.a + i.color.r) * i.color.r;

                half4 greenTex = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, half3(i.uv.x, i.uv.y, i.tex.y));
                half greenAlpha = (greenTex.a + i.color.g) * i.color.g;

                half4 blueTex = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, half3(i.uv.x, i.uv.y, i.tex.z));
                half blueAlpha = (blueTex.a + i.color.b) * i.color.b;

                half4 alphaTex = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, half3(i.uv.x, i.uv.y, i.tex.w));
                half alphaAlpha = (alphaTex.a + i.color.a) * i.color.a;

                half maximum = max(max(redAlpha, greenAlpha), max(blueAlpha, alphaAlpha)) * _BlendDepth;

                half redBlend = max(redAlpha - maximum, 0);
                half greenBlend = max(greenAlpha - maximum, 0);
                half blueBlend = max(blueAlpha - maximum, 0);
                half alphaBlend = max(alphaAlpha - maximum, 0);

                half totalBlend = redBlend + greenBlend + blueBlend + alphaBlend;
                return ((redTex * redBlend) + (greenTex * greenBlend) + (blueTex * blueBlend) + (alphaTex * alphaBlend)) / totalBlend;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

//Step 1 . Red, Green, Blue and Alpha - Add
//alpha0 = (texture0.a + blend0) * blend0;

//Step 2. Maximum
//max = Maximum(alpha0, alpha1, alpha2, alpha3) - depth (0.2);

//Step 3. Red, Green, Blue, Alpha  Blend
//blend0 = Maximum(alpha0 - max, 0);

//Step 4 Combine
//total = blend0 + blend1 + blend2 + blend3;
//result = ((texture0 * blend0) + (texture1 * blend1) + (texture2 * blend2) + (texture3 * blend3)) / total;