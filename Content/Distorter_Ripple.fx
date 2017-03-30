
//http://gamedev.stackexchange.com/questions/18201/wave-ripple-effect

/* Variables */

matrix	MatrixTransform;


float4 _Params1;    // [ aspect, 1, scale, 0 ]
float4 _Params2;    // [ 1, 1/aspect, refraction, reflection ]

float3 _Drop1;
float3 _Drop2;
float3 _Drop3;

float4 _Reflection;	//color

texture TextureMap; // texture 0 => screen render
sampler2D textureMapSampler = sampler_state
{
	Texture = (TextureMap);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture GradTexture; // texture 1 => Gradiant texture
sampler2D GradTextureSampler = sampler_state
{
	Texture = (GradTexture);
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


float wave(float2 position, float2 origin, float time)
{
	float d = length(position - origin);
	float t = time - d * _Params1.z;

	return (tex2D(GradTextureSampler, float2(t, 0)).a - 0.5f) * 2;
}



float allwave(float2 position)
{
	//return
	//	wave(position, _Drop1.xy, _Drop1.z) +
	//	wave(position, _Drop2.xy, _Drop2.z) +
	//	wave(position, _Drop3.xy, _Drop3.z);

	return
		wave(position, _Drop1.xy, _Drop1.z);

}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	//Vertex coord to screen coord to map UV
	float2 screen = input.Screen.xy / input.Screen.w;
	screen = (screen + 1.0) / 2;
	screen.y = 1.0 - screen.y;

	//Compute Displacement from gradiant
	const float2 dx = float2(0.01f, 0);
	const float2 dy = float2(0, 0.01f);

	float2 p = input.TexCoord * _Params1.xy;

	float w = allwave(p);

	float2 dw = float2(allwave(p + dx) - w, allwave(p + dy) - w);



	//Attenuation
	float distance = length(p - _Drop1.xy) * 1.5f;
	float multiplier = (distance < 1.0) ? ((distance - 1.0)*(distance - 1.0)) : 0.0;
	dw *= multiplier;


	//UV
	float2 duv = dw * _Params2.xy * 0.2f * _Params2.z;

	

	//Reflexion
	float fr = pow(length(dw) * 3 * _Params2.w, 3);

	//Displacement 
	float4 c = tex2D(textureMapSampler, screen + duv);

	float4 texColor = lerp(c, _Reflection, fr);

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
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif

	}
}