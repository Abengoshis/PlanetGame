Shader "Custom/TransparentUnlitBillboard" 
{
	Properties 
	{		
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
	}
	SubShader 
	{
	    Tags { "RenderType"="Transparent" "Queue"="Transparent" "DisableBatching"="True" }

		ZWrite Off
		Cull Back
		ZTest LEqual
		Blend One One

    	Pass 
    	{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
						
			#include "UnityCG.cginc"
			
			struct a2v
			{
                fixed4 vertex : POSITION;
                fixed4 texcoord : TEXCOORD0;
            };
		
			struct v2f 
			{
    			fixed4 position : SV_POSITION;
    			fixed2 uv : TEXCOORD0;
			};
			
			fixed4 _Color;
			sampler2D _MainTex;

			v2f vert(a2v v)
			{
				fixed xScale = length(mul(_Object2World, fixed4(1, 0, 0, 0)));
				fixed yScale = length(mul(_Object2World, fixed4(0, 1, 0, 0)	));		
			
    			v2f f;
    			f.position = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1)) + float4(v.vertex.x * xScale, v.vertex.y * yScale, 0, 0));
    			f.uv = v.texcoord;
    			return f;
			}			
			
			half4 frag(v2f f) : COLOR
			{ 		
 				return tex2D(_MainTex, f.uv) * _Color;
			}
			
			ENDCG

    	}
	}
}