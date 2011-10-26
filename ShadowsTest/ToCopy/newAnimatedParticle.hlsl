void main_vp(float2 uv : TEXCOORD0,
			 float4 position : POSITION,
			 float4 color : COLOR0,
			 
			 out float2 oUv : TEXCOORD0,			 
			 out float4 oPosition : POSITION,
			 out float4 oColor : COLOR0,
			 
			 uniform int numTilesU,
			 uniform int numTilesV,
			 
			 uniform float4x4 worldViewProj)
{
	int numTilesTotal = numTilesU*numTilesV;
	int selectedTile = (int)(numTilesTotal * color.a);
	if (selectedTile == numTilesTotal)
		selectedTile = numTilesTotal - 1;
	int selTileU = selectedTile % numTilesU;
	int selTileV = selectedTile / numTilesU;
	float offsetU = selTileU;
	float offsetV = selTileV;
	oUv.x = uv.x + offsetU;
	oUv.y = uv.y + offsetV;
	oUv.x /= numTilesU;
	oUv.y /= numTilesV;
	
	oPosition = mul(worldViewProj, position);
	oColor = color;
}

float4 main_fp (float2 uv : TEXCOORD0,
				uniform sampler2D tex : register(s0))
: COLOR
{
	return tex2D(tex, uv.xy);
	//return float4(uv.x, 0, 0, 1);
}