Shader "SquadRush/Toon"
{
    Properties
    {
        _Flash ("Hit flash", Range(0,1)) = 0
        _Weathering ("Surface weathering", Range(0,1)) = 0
        _SurfaceGrain ("Surface grain", 2D) = "gray" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "Silhouette"
            Cull Front
            CGPROGRAM
            #pragma vertex outline
            #pragma fragment ink
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            float4 outline(appdata v):SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v.vertex.xyz += v.normal * 0.009;
                return UnityObjectToClipPos(v.vertex);
            }
            fixed4 ink():SV_Target { return fixed4(0.035,0.075,0.11,1); }
            ENDCG
        }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; fixed4 color:COLOR; SHADOW_COORDS(2) UNITY_FOG_COORDS(3) float3 local:TEXCOORD4; UNITY_VERTEX_INPUT_INSTANCE_ID };
            float _Weathering;
            sampler2D _SurfaceGrain;
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flash)
            UNITY_INSTANCING_BUFFER_END(Props)
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal);
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz; o.color=v.color; o.local=v.vertex.xyz;
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 n=normalize(i.normal), l=normalize(_WorldSpaceLightPos0.xyz);
                float3 eye=normalize(_WorldSpaceCameraPos-i.world);
                float diffuse=smoothstep(-0.05,0.75,dot(n,l));
                float rim=pow(1-saturate(dot(n,eye)),3);
                float spec=pow(saturate(dot(n,normalize(l+eye))),40)*0.22;
                float shadow=SHADOW_ATTENUATION(i);
                fixed3 surface=i.color.rgb;
                if (_Weathering > 0.01)
                {
                    // One cached texture fetch supplies both scales of object-space dirt.
                    float2 grain=tex2D(_SurfaceGrain,(i.local.xz+i.local.y*float2(0.37,0.23))*0.6).rg;
                    float dirt=(1-smoothstep(0.05,0.65,i.local.y))*0.18;
                    float mottling=(grain.r-0.5)*0.22+(grain.g-0.5)*0.075;
                    surface*=1+mottling-dirt;
                    surface=lerp(surface,surface*fixed3(1.10,0.94,0.77),dirt);
                    spec*=0.28;
                }
                fixed3 color=surface*(fixed3(0.31,0.37,0.46)+fixed3(0.73,0.68,0.58)*diffuse*shadow);
                color+=rim*fixed3(0.04,0.11,0.16)+spec*shadow;
                color=lerp(color,fixed3(1,0.96,0.82),UNITY_ACCESS_INSTANCED_PROP(Props,_Flash));
                fixed4 result=fixed4(color,1); UNITY_APPLY_FOG(i.fogCoord,result); return result;
            }
            ENDCG
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct v2f { V2F_SHADOW_CASTER; };
            v2f vertShadow(appdata_base v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); TRANSFER_SHADOW_CASTER_NORMALOFFSET(o) return o; }
            float4 fragShadow(v2f i):SV_Target { SHADOW_CASTER_FRAGMENT(i) }
            ENDCG
        }
    }
    Fallback "VertexLit"
}
