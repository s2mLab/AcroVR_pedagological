using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FixedSizeRK : IntegratorAbstract
{
    public double TimeStep { get; protected set; }

    public override AvatarCoordinates[] Perform(AvatarCoordinates _initialCondition)
    {
        throw new System.NotImplementedException();
    }

}
