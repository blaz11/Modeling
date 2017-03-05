truct VS_IN
{
	float4 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

cbuffer cbWorld : register(b0)
{
	float4x4 worldViewProj;
};

cbuffer cColor : register(b1)
{
	float4 color;
};


PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(input.pos, worldViewProj);
	output.col = color;
	
	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return input.col;
}