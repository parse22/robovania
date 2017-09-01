/* OUTLINEDIFFUSE SHADER:
 * Basically the Unity Mobile Diffuse shader with outline behavior and solid color support only.
 * Outline handled by forward rendering pass of second set of geometry with its vertices scaled outwards slightly.
 * */
Shader "Custom/OutlineDiffuse" {
Properties {
	_MainColor ("Color", Color) = (1,1,1,1)
	_OutlineColor ("Outline Color", Color) = (0,0,0,1)
	_Outline ("Outline width", Range (0.002, 1.0)) = 0.005
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

    CGINCLUDE
    #include "UnityCG.cginc"
     
    struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };
     
    struct v2f {
        float4 pos : SV_POSITION;
        fixed4 color : COLOR;
    };
     
    uniform float _Outline;
    uniform float4 _OutlineColor;
     
    v2f vert(appdata v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
 
        float3 norm   = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
        float2 offset = TransformViewToProjection(norm.xy);
 
        o.pos.xy += offset * o.pos.z * _Outline;
        o.color = _OutlineColor;
        return o;
    }
    ENDCG      
 
    Pass {
        Name "OUTLINE"
        Tags { "LightMode" = "Always" }
        Cull Front
        ZWrite On
        ColorMask RGB
        Blend SrcAlpha OneMinusSrcAlpha
 
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        fixed4 frag(v2f i) : SV_Target
        {
            return i.color;
        }
        ENDCG
    }

CGPROGRAM
#pragma surface surf Lambert noforwardadd

float4 _MainColor;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	o.Albedo = _MainColor.rgb;
	o.Alpha = _MainColor.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}