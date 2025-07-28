
Shader "DiamondScreenGetURP"
{
	Properties
	{
		//	RenderTexDiamond("RenderTexDiamond", 2D) = "white" {}

	}

	SubShader
	{


		Tags { "RenderType" = "Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0



		Pass
		{

			CGPROGRAM



			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
		//only defining to not throw compilation error over Unity 5.5
		#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
		#endif
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing
		#include "UnityCG.cginc"


		struct appdata
		{
			float4 vertex : POSITION;
			float4 color : COLOR;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
			float3 worldPos : TEXCOORD0;
			#endif
			float4 ase_texcoord1 : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		uniform sampler2D RenderTexDiamond;
		inline float4 ASE_ComputeGrabScreenPos(float4 pos)
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = (pos.y - o.y) * _ProjectionParams.x * scale + o.y;
			return o;
		}



		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			UNITY_TRANSFER_INSTANCE_ID(v, o);

			float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
			float4 screenPos = ComputeScreenPos(ase_clipPos);
			o.ase_texcoord1 = screenPos;

			float3 vertexValue = float3(0, 0, 0);
			#if ASE_ABSOLUTE_VERTEX_POS
			vertexValue = v.vertex.xyz;
			#endif
			vertexValue = vertexValue;
			#if ASE_ABSOLUTE_VERTEX_POS
			v.vertex.xyz = vertexValue;
			#else
			v.vertex.xyz += vertexValue;
			#endif
			o.vertex = UnityObjectToClipPos(v.vertex);

			#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			#endif
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(i);
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
			fixed4 finalColor;
			#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
			float3 WorldPosition = i.worldPos; 
			#endif
			float4 screenPos = i.ase_texcoord1;
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos(screenPos);
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w ;

			ase_grabScreenPosNorm.y =  1 - ase_grabScreenPosNorm.y;

			finalColor = tex2D(RenderTexDiamond, ase_grabScreenPosNorm.xy);
			return finalColor;
		}
		ENDCG
	}
	}



}
/*ASEBEGIN
Version=18912
507;297;1920;1019;1518.462;577.5056;1;True;True
Node;AmplifyShaderEditor.GrabScreenPosition;3;-1140.462,-314.5056;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-1331,-187;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-701,-129;Inherit;True;Property;RenderTexDiamond;RenderTexDiamond;0;0;Create;False;0;0;0;False;0;False;1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;100;1;New Amplify Shader;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;1;1;3;0
WireConnection;0;0;1;0
ASEEND*/
//CHKSM=0658EAAAEC7F2D5AB630C5EBA102D2291346B01F