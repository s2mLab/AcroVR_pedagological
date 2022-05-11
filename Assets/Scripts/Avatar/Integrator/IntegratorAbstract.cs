using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IntegratorAbstract
{
    public delegate AvatarCoordinates DynamicFunction(AvatarCoordinates _x, AvatarVector _tau);
    public abstract AvatarCoordinates[] Perform(AvatarCoordinates _initialCondition);
    public abstract AvatarCoordinates DxDt(DynamicFunction _dynamicFunction, AvatarCoordinates _x, AvatarVector _u);
}
