#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

float4x4 projectionMatrix;
float4x4 viewMatrix;
float4x4 modelMatrix;
float2 angles;
float radius;
int triangles;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(uint vertexId: SV_VertexID)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = float4(0,0,0,0);
    
    // Compute vertices based on index if not a midpoint vertex.
    int id = vertexId % 3;
    if(0 < id){
        int tri = vertexId / 3;
        int curve = tri+(id-1);
        float ease = ((float)curve)/triangles;
        
        // Now that we have an ease factor in [0..1] we can get the real angle.
        float angle = lerp(angles.x, angles.y, ease);
        
        // Turn polar coordinates into cartesian.
        pos.x = radius * cos(angle);
        pos.y = radius * sin(angle);
    }

    output.Position = mul(mul(mul(pos, modelMatrix), viewMatrix), projectionMatrix);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    return float4(1, 0, 0, 0.5);
}

technique Cone
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};