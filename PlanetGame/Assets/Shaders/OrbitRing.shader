Shader "Custom/OrbitRing"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_SemiMajorAxis ("Semi Major Axis", Float) = 1
		_SemiMinorAxis ("Semi Minor Axis", Float) = 1
		_Thickness ("Thickness", Range(0.0, 0.5)) = 0.005
	}
	SubShader
	{
	    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		
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
            };
		
			struct v2f 
			{
    			fixed4 position : SV_POSITION;
    			fixed4 coord : COLOR;
			};
			
			fixed4 _Color;
			fixed _SemiMajorAxis;
			fixed _SemiMinorAxis;
			fixed _Thickness;

			v2f vert(a2v v)
			{
    			v2f f;
    			f.position = mul(UNITY_MATRIX_MVP, v.vertex);
    			f.coord = v.vertex;
    			return f;
			}			
			
			half4 frag(v2f f) : COLOR
			{
				float semiMajor = 1;
				float semiMinor = 1 * _SemiMinorAxis / _SemiMajorAxis;
			
				float angle = atan2(f.coord.y, f.coord.x);
				
 				fixed2 perimeter;
 				perimeter.x = semiMajor * cos(angle);
 				perimeter.y = semiMinor * sin(angle);
 				
 				fixed dist = distance(f.coord, perimeter);
 				if (dist > 0.5 - _Thickness && dist < 0.5 + _Thickness)
 					return fixed4(1,1,1,1);
 					
 				return fixed4(0,0,0,0);
			}
			
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
