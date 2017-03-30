
//http://gamedev.stackexchange.com/questions/18201/wave-ripple-effect

/* Variables */

float OffsetPower;
matrix	MatrixTransform;

texture TextureMap; // texture 0 => screen render
sampler2D textureMapSampler = sampler_state
{
	Texture = (TextureMap);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture DisplacementMap; // texture 1 => Displacement normal map
sampler2D displacementMapSampler = sampler_state
{
	Texture = (DisplacementMap);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Screen : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(float4 position:POSITION, float2 texcoord : TEXCOORD0, float4 color : COLOR0)
{
	VertexShaderOutput output;

	output.Position = mul(position, MatrixTransform);
	output.Screen = output.Position;
	output.TexCoord = texcoord;

	return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{

	//Displacement coords from texture
	float2 coords = input.TexCoord;
	float4 displacementColor = tex2D(displacementMapSampler, coords);
	float mul = (displacementColor.b * 0.1);// 0.1 seems to work nicely.

	//Vertex coord to screen coord to map UV
	float2 screen = input.Screen.xy / input.Screen.w;
	screen = (screen + 1.0) / 2;
	screen.y = 1.0 - screen.y;

	screen.x += (displacementColor.r * mul) - mul / 2;
	screen.y += (displacementColor.g * mul) - mul / 2;
	
	float4 texColor = tex2D(textureMapSampler, screen);

	//texColor.x += 0.5;	//Help to show sprite zone

	return texColor;
}


technique Technique1
{
	pass Pass1
	{
#if SM4
		VertexShader = compile vs_4_0_level_9_1 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
#else
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
#endif

	}
}