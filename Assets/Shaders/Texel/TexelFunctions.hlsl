float3 CalculateSnappedWorldPos(float2 originalUV, float3 originalWorldPos, float4 texelSize)
{
    // 1.) Calculate how much the texture UV coords need to
    //     shift to be at the center of the nearest texel.
    float2 centerUV = floor(originalUV * (texelSize.zw))/texelSize.zw + (texelSize.xy/2.0);
    float2 dUV = (centerUV - originalUV);
 
    // 2b.) Calculate how much the texture coords vary over fragment space.
    //      This essentially defines a 2x2 matrix that gets
    //      texture space (UV) deltas from fragment space (ST) deltas
    // Note: I call fragment space (S,T) to disambiguate.
    float2 dUVdS = ddx(originalUV);
    float2 dUVdT = ddy(originalUV);
 
    // 2c.) Invert the fragment from texture matrix
    float2x2 dSTdUV = float2x2(dUVdT[1], -dUVdT[0], -dUVdS[1], dUVdS[0])*(1.0f/(dUVdS[0]*dUVdT[1]-dUVdT[0]*dUVdS[1]));
 
 
    // 2d.) Convert the UV delta to a fragment space delta
    float2 dST = mul(dSTdUV , dUV);
 
    // 2e.) Calculate how much the world coords vary over fragment space.
    float3 dXYZdS = ddx(originalWorldPos);
    float3 dXYZdT = ddy(originalWorldPos);
 
    // 2f.) Finally, convert our fragment space delta to a world space delta
    // And be sure to clamp it to SOMETHING in case the derivative calc went insane
    // Here I clamp it to -1 to 1 unit in unity, which should be orders of magnitude greater
    // than the size of any texel.
    float3 dXYZ = clamp(dXYZdS * dST[0] + dXYZdT * dST[1], -1, 1);
 
    // 3.) Transform the snapped UV back to world space
    return originalWorldPos + dXYZ;
}

float3 CalculateSnappedWorldViewDir(float3 viewDirectionWS, float3 positionWS) {
    //Recalculate view direction according to recalculated texelized inputData.positionWS
    float3 viewDirectionWSCorrected = GetCameraPositionWS() - positionWS;
    //This needs to be done to make corrected viewDirectionWSCorrected same length as the original. If omitted, specular will not be displayed properly
    float viewPositionMult = length(viewDirectionWS) / length(viewDirectionWSCorrected);

    return viewDirectionWSCorrected * viewPositionMult;
}

half PosterizeHalf(half color, float steps) {
    return floor(color / (1 / steps)) * (1 / steps);
}

half4 Posterize4(half4 color, float4 steps) {
    half4 result = color;
    if (steps.x > 0) result.x = PosterizeHalf(color.x, steps.x);
    if (steps.y > 0) result.y = PosterizeHalf(color.y, steps.y);
    if (steps.z > 0) result.z = PosterizeHalf(color.z, steps.z);
    if (steps.w > 0) result.w = PosterizeHalf(color.w, steps.w);
    return result;
}

half3 Posterize3(half3 color, float4 steps) {
    half3 result = color;
    if (steps.x > 0) result.x = PosterizeHalf(color.x, steps.x);
    if (steps.y > 0) result.y = PosterizeHalf(color.y, steps.y);
    if (steps.z > 0) result.z = PosterizeHalf(color.z, steps.z);
    return result;
}