struct InBoidsUnitData
{
    float3 Position;
    float3 Direction;
};

struct OutBoidsUnitData
{
    int NeighborCount;
    float3 CohesionVector;
    float3 AlignmentVector;
    float3 SeperationVector;
    float3 ForcerVector;
};

struct InForcerData
{
    float Weight;
    float3 Position;
    float3 Direction;
    float3 LocalHalfExtents;
    float4x4 WorldToLocalMatrix;
};

bool Contains(float3 InWorldPoint, InForcerData InputForcerData)
{
    float3 LocalPoint = mul(float4(InWorldPoint.xyz, 1), InputForcerData.WorldToLocalMatrix).xyz;
    float3 AbsLocalPoint = abs(LocalPoint);
    return 
        AbsLocalPoint.x < InputForcerData.LocalHalfExtents.x &&
        AbsLocalPoint.y < InputForcerData.LocalHalfExtents.y &&
        AbsLocalPoint.z < InputForcerData.LocalHalfExtents.z;
}

float3 Project(float3 InFrom, float3 InTo)
{
    float ADotB = dot(InFrom, InTo);
    float BDotB = dot(InTo, InTo);
    return ADotB / BDotB * InTo;
}