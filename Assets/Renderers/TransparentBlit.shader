Shader "Custom/TransparentBlit" {
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
    }

        CGINCLUDE

#include "UnityCG.cginc"

#if defined(SEPARATE_TEXTURE_SAMPLER)
        Texture2D _MainTex;
    SamplerState sampler_MainTex;
#else
        sampler2D _MainTex;
#endif
    float4 _MainTex_TexelSize;
    float4 _MainTex_ST;


    struct AttributesDefault
    {
        float4 vertex : POSITION;
        float4 texcoord : TEXCOORD0;
    };

    struct Varyings
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    Varyings VertBlit(AttributesDefault v)
    {
        Varyings o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    half4 FragBlit(Varyings i) : SV_Target
    {
        half4 col = tex2D(_MainTex, i.uv);
        if (col.a > 0.0f)
            col.a = 0.5f;
        return col;
    }

        ENDCG

        SubShader
    {
        Tags{ "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
        {
            CGPROGRAM

                #pragma vertex VertBlit
                #pragma fragment FragBlit

            ENDCG
        }
    }
}