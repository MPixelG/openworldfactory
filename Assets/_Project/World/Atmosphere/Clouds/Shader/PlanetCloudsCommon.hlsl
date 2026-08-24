#ifndef PLANET_CLOUDS_COMMON_INCLUDED
#define PLANET_CLOUDS_COMMON_INCLUDED

// Intersect a normalised ray with the shell between two origin-centred
// spheres. Returns float2(tEnter, tExit) in world units along the ray.
// If tExit <= tEnter there is no segment: skip the pixel because it is not visible
float2 RayShell(float3 ro, float3 rd, float rInner, float rOuter)
{
    float b      = dot(ro, rd);
    float rr     = dot(ro, ro);

    // --- outer sphere: gives us the base interval ---
    float cOuter = rr - rOuter * rOuter;
    float dOuter = b * b - cOuter;
    if (dOuter < 0.0) return float2(0.0, -1.0);   // ray misses the deck

    float sOuter = sqrt(dOuter);
    float tNear  = -b - sOuter;
    float tFar   = -b + sOuter;
    if (tFar < 0.0) return float2(0.0, -1.0);     // deck is entirely behind us

    float tEnter = max(tNear, 0.0);               // never march backwards
    float tExit  = tFar;

    // --- inner sphere: trims the interval ---
    float cInner = rr - rInner * rInner;
    float dInner = b * b - cInner;

    if (dInner > 0.0)
    {
        float sInner = sqrt(dInner);

        if (cInner > 0.0)
        {
            // Camera is ABOVE the inner sphere. The planet blocks
            // everything past the near inner hit.
            float iNear = -b - sInner;
            if (iNear > 0.0) tExit = min(tExit, iNear);
        }
        else
        {
            // Camera is BELOW the inner sphere (on the ground).
            // The deck only starts once we punch back out through it.
            float iFar = -b + sInner;
            tEnter = max(tEnter, iFar);
        }
    }

    return float2(tEnter, tExit);
}

#endif