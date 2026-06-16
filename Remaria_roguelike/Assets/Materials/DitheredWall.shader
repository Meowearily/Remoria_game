Shader "Custom/DitheredWall"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [NoScaleOffset] _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        
        _Alpha ("Alpha (Dither)", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        // Use standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Alpha;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // --- Dithering Logic ---
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            float2 pixelPos = screenUV * _ScreenParams.xy;
            
            // 4x4 dither matrix
            float4x4 ditherMatrix = float4x4(
                0.0625, 0.5625, 0.1875, 0.6875,
                0.8125, 0.3125, 0.9375, 0.4375,
                0.25, 0.75, 0.125, 0.625,
                1.0, 0.5, 0.875, 0.375
            );
            
            uint x = uint(pixelPos.x) % 4;
            uint y = uint(pixelPos.y) % 4;
            
            // If alpha is lower than the threshold in the matrix, discard the pixel
            clip(_Alpha - ditherMatrix[x][y]);
            // -----------------------

            // Albedo
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            // Normal
            o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
            
            // Metallic and Smoothness
            fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = mg.r * _Metallic;
            o.Smoothness = mg.a * _Glossiness;
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
