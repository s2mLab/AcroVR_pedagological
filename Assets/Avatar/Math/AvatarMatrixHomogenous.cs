using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMatrixHomogenous : AvatarMatrix
{

    public AvatarMatrixHomogenous() : base(4, 4)
    {

    }

    public AvatarMatrixHomogenous(
        double r0c0, double r0c1, double r0c2, double r0c3,
        double r1c0, double r1c1, double r1c2, double r1c3,
        double r2c0, double r2c1, double r2c2, double r2c3,
        double r3c0, double r3c1, double r3c2, double r3c3
    ) : base(4, 4)
    {
        Value[0, 0] = r0c0;
        Value[1, 0] = r1c0;
        Value[2, 0] = r2c0;
        Value[3, 0] = r3c0;
        Value[0, 1] = r0c1;
        Value[1, 1] = r1c1;
        Value[2, 1] = r2c1;
        Value[3, 1] = r3c1;
        Value[0, 2] = r0c2;
        Value[1, 2] = r1c2;
        Value[2, 2] = r2c2;
        Value[3, 2] = r3c2;
        Value[0, 3] = r0c3;
        Value[1, 3] = r1c3;
        Value[2, 3] = r2c3;
        Value[3, 3] = r3c3;
    }

    public AvatarMatrixHomogenous(AvatarMatrixHomogenous _other) : base(_other)
    {

    }

    static public AvatarMatrixHomogenous Identity()
    {
        return new AvatarMatrixHomogenous(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
    }

    static public AvatarMatrixHomogenous operator *(AvatarMatrixHomogenous _first, AvatarMatrixHomogenous _second)
    {
        return _first.Multiply(_second);
    }

    public AvatarMatrixHomogenous Multiply(AvatarMatrixHomogenous _other)
    {
        AvatarMatrix _result = new AvatarMatrixHomogenous();
        Multiply(this, _other, ref _result);
        return (AvatarMatrixHomogenous)_result;
    }
    public new AvatarMatrixHomogenous Transpose()
    {
        Debug.Log("Transpose not implemented yet for AvatarMatrixHomogenous");
        return null;
    }
}
