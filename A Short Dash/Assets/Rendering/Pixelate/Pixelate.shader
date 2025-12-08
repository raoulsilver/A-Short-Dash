Shader "RB/Pixelate"
{
    Properties
    {
        _PixelWidth("Pixel Width", int) = 320
        _PixelHeight("Pixel Height", int) = 180
        _MainTex("MainTex", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "UniversalMaterialType"="Unlit"
            "ShaderStage"="FullScreen"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            int _PixelWidth;
            int _PixelHeight;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float2 pixelUV = floor(float2(IN.uv.x * _PixelWidth, IN.uv.y * _PixelHeight));
                pixelUV /= float2(_PixelWidth, _PixelHeight);

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelUV);
            }

            ENDHLSL
        }
    }
}