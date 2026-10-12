Shader "SquadRush/Toon"
{
    Properties { _Flash ("Hit flash", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flash)
            UNITY_INSTANCING_BUFFER_END(Props)
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal);
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz; o.color=v.color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 n=normalize(i.normal), l=normalize(_WorldSpaceLightPos0.xyz);
                float3 eye=normalize(_WorldSpaceCameraPos-i.world);
                float diffuse=smoothstep(-0.12,0.82,dot(n,l));
                float rim=pow(1-saturate(dot(n,eye)),3);
                float spec=pow(saturate(dot(n,normalize(l+eye))),32)*0.14;
                fixed3 color=i.color.rgb*(0.46+0.57*diffuse)+rim*fixed3(0.10,0.14,0.19)+spec;
                color=lerp(color,fixed3(1,0.96,0.82),UNITY_ACCESS_INSTANCED_PROP(Props,_Flash));
                return fixed4(color,1);
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}
