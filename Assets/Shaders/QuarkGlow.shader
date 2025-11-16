Shader "Custom/QuarkGlow"
{
    Properties
    {
        PrimaryColor   ("Primary Color",   Color) = (0,0.2,0.85,1)
        SecondaryColor ("Secondary Color", Color) = (1,0,0.03,1)
        AccentColor    ("Accent Color",    Color) = (1,1,1,1)

        _BaseEmission  ("Base Emission",   Float) = 1.0
        _GlowPulse     ("Glow Pulse",      Float) = 0.0
        _TrebleAmount  ("Treble Amount",   Float) = 0.0
        _Hover         ("Hover",           Float) = 0.0

        _FresnelPower     ("Fresnel Power",     Float) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos        : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float3 worldNormal: TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            float4 PrimaryColor;
            float4 SecondaryColor;
            float4 AccentColor;

            float _BaseEmission;
            float _GlowPulse;
            float _TrebleAmount;
            float _Hover;

            float _FresnelPower;
            float _FresnelIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float NdotV = saturate(dot(normalize(i.worldNormal), viewDir));
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;

                // Simple vertical gradient between primary and secondary
                float t = saturate(i.uv.y);
                float3 grad = lerp(PrimaryColor.rgb, SecondaryColor.rgb, t);

                // Blend in accent based on treble
                grad = lerp(grad, AccentColor.rgb, saturate(_TrebleAmount));

                // Base glow + music pulse + hover bump
                float glowFactor = _BaseEmission + _GlowPulse + _Hover * 0.5;

                float3 color = grad * glowFactor + grad * fresnel;
                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}

