
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	float4 g_vColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flArc < Attribute( "Arc" ); Default1( 0.27199197 ); >;
	float g_flStartLength < Attribute( "StartLength" ); Default1( 0.671946 ); >;
	float g_flEndLength < Attribute( "EndLength" ); Default1( 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float4 l_0 = g_vColor;
		float4 l_1 = i.vTintColor;
		float4 l_2 = l_0 * l_1;
		float l_3 = l_2.w;
		float l_4 = g_flArc;
		float2 l_5 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_6 = PolarCoordinates( ( l_5 ) - ( float2( 0.5, 0.5 ) ), 1, 1 );
		float l_7 = l_6.y;
		float l_8 = l_7 + 0.5;
		float l_9 = step( l_4, l_8 );
		float l_10 = g_flStartLength;
		float l_11 = l_6.x;
		float l_12 = step( l_10, l_11 );
		float l_13 = g_flEndLength;
		float l_14 = step( l_11, l_13 );
		float l_15 = l_12 * l_14;
		float l_16 = l_9 * l_15;
		float l_17 = l_3 * l_16;
		

		return float4( l_2.xyz, l_17 );
	}
}
