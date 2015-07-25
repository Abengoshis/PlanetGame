Shader "Custom/Planet"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_GlowColour ("Glow Colour", Color) = (1,1,1,1)
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Opaque" "LightMode"="Vertex" }
			LOD 200
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			
			sampler2D _MainTex;
			fixed4 _GlowColour;
			
			struct v2f
			{
				fixed4 position : SV_POSITION;
				fixed4 light : COLOR;
				fixed2 uv : TEXCOORD0;
				
				// For rim lighting.
				fixed3 normal : NORMAL;
				fixed3 viewDir : TEXCOORD2;
			};
			
			float calculateRim(fixed3 viewDir, fixed3 normal, float power, float strength)
			{
				return max((1.0 - pow(dot(viewDir, normal), power)) * strength, 0);
			}

			v2f vert (appdata_base v)
			{			
				v2f f;
				f.position = mul(UNITY_MATRIX_MVP, v.vertex);
				f.uv = v.texcoord.xy;
				f.light = fixed4(0,0,0,0);
				
				f.normal = normalize(mul(_Object2World, v.normal));
				f.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				
				int numLights = 4;	// Increase if there are gonna be more lights.
				fixed3 lightDir;
				fixed lightAtten;
				for (int i = 0; i < numLights; ++i)
				{
					// Skip lights with no alpha (might speed up when looping through more lights than are in scene?).
					if (unity_LightColor[i].a == 0)
						continue;
				
					if (unity_LightPosition[i].w == 0)
					{
						lightDir = normalize(mul(unity_LightPosition[i], UNITY_MATRIX_IT_MV).xyz);
						lightAtten = 2;
					}
					else
					{
						fixed3 delta = mul(unity_LightPosition[i], UNITY_MATRIX_IT_MV).xyz - v.vertex.xyz;
						lightDir = normalize(delta);
						lightAtten = 0.5 / length(delta);
					}
					
					f.light.rgb += unity_LightColor[i].rgb * max(dot(v.normal, lightDir), 0) * lightAtten;
				}
				
				return f;
			}
			
			fixed4 frag (v2f f) : SV_TARGET
			{
				// Get the colour of the texture at the fragment's uv.
				fixed4 c = tex2D(_MainTex, f.uv) * f.light;
				
				// Get the colour of the rim lighting at this position.
				fixed4 rim = calculateRim(f.viewDir, f.normal * 2, 1, 1) * (f.light * 0.8 + 0.2) * _GlowColour;
				
				return c + rim;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
}
