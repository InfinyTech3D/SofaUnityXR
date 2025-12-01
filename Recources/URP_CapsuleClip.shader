Shader "Custom/URP_CapsuleClip"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)

        _CapsulePointA("Capsule Point A", Vector) = (0,0,0,0)
        _CapsulePointB("Capsule Point B", Vector) = (0,1,0,0)
        _CapsuleRadius("Capsule Radius", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // capsule params
            float3 _CapsulePointA;
            float3 _CapsulePointB;
            float _CapsuleRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 worldPos : TEXCOORD8; // <-- on ajoute
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.normalWS = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz); // <-- world pos pour capsule clip
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // capsule clip inside
                float3 p = i.worldPos;
                float3 pa = _CapsulePointA;
                float3 pb = _CapsulePointB;
                float3 ba = pb - pa;
                float3 pa_p = p - pa;
                float h = saturate(dot(pa_p, ba) / dot(ba, ba));
                float3 closest = pa + h * ba;
                float dist = distance(p, closest);
                if (dist < _CapsuleRadius)
                    discard;

                return half4(1,1,1,1); // couleur blanche simple
            }

            ENDHLSL
        }
    }
}
