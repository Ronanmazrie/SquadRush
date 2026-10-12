Shader "SquadRush/Glow"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; fixed4 color:COLOR; };
            v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); o.pos=UnityObjectToClipPos(v.vertex); o.color=v.color; return o; }
            fixed4 frag(v2f i):SV_Target { return i.color; }
            ENDCG
        }
    }
}
