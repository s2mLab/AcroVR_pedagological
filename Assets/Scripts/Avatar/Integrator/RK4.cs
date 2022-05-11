using System;
using System.Collections.Generic;
using UnityEngine;

public class RK4 : FixedSizeRK
{
    public override AvatarCoordinates DxDt(DynamicFunction _dynamicFunction, AvatarCoordinates _x, AvatarVector _u)
    {
        AvatarCoordinates k1 = _dynamicFunction(_x, _u);
        AvatarCoordinates k2 = _dynamicFunction(_x + TimeStep / 2 * k1, _u);
        AvatarCoordinates k3 = _dynamicFunction(_x + TimeStep / 2 * k2, _u);
        AvatarCoordinates k4 = _dynamicFunction(_x + TimeStep * k3, _u);
        return _x + TimeStep / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
    }
}
