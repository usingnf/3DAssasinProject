Shader "Custom/CloakingShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Opacity("Opacity", Range(0,1)) = 0.1
        _DeformIntensity("Deform by Normal Intensity", Range(0,3)) = 1
        _RimPow("Rim Power", int) = 3
        _RimColor("Rim Color", Color) = (0,1,1,1)
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
            zwrite off

            GrabPass{}

            CGPROGRAM
            #pragma surface surf CloakingLight noambient novertextlights noforwardadd
            #pragma target 3.0

            sampler2D _GrabTexture;
            sampler2D _MainTex;
            sampler2D _NormalMap;
            float _DeformIntensity;
            float _Opacity;
            float _RimPow;
            float _RimColor;

        struct Input
        {
            float4 screenPos;
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            float4 color = tex2D(_MainTex, IN.uv_MainTex);
            float2 uv_screen = IN.screenPos.xy / IN.screenPos.w;
            fixed3 mappingScreenColor = tex2D(_GrabTexture, uv_screen + o.Normal.xy * _DeformIntensity);
            float rimBrightness = 1 - saturate(dot(IN.viewDir, o.Normal));
            rimBrightness = pow(rimBrightness, _RimPow);
            o.Emission = mappingScreenColor * (1 - _Opacity) + _RimColor * rimBrightness;
            o.Albedo = color.rgb;
        }
        fixed4 LightingCloakingLight(SurfaceOutput s, float3 lightDir, float atten)
        {
            return fixed4(s.Albedo * _Opacity * _LightColor0, 1);
        }
        
        ENDCG
    }
    FallBack "Diffuse"
}
