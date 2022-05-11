using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMatrixRotation : AvatarMatrix
{

    public AvatarMatrixRotation() : base(AvatarMatrixRotation.Identity())
    {

    }

    public AvatarMatrixRotation(
        double r0c0, double r0c1, double r0c2,
        double r1c0, double r1c1, double r1c2,
        double r2c0, double r2c1, double r2c2
    ) : base(3, 3)
    {
        Value[0, 0] = r0c0;
        Value[1, 0] = r1c0;
        Value[2, 0] = r2c0;
        Value[0, 1] = r0c1;
        Value[1, 1] = r1c1;
        Value[2, 1] = r2c1;
        Value[0, 2] = r0c2;
        Value[1, 2] = r1c2;
        Value[2, 2] = r2c2;
    }

    public AvatarMatrixRotation(AvatarMatrixRotation _other) : base(_other)
    {

    }
    public AvatarMatrixRotation(XDA.XsEuler _xyz) : base(3, 3)
    {

        // Convert to a right-hand side matrix
        AvatarVector3 _tpVector = MathUtils.ToRadian(
            new AvatarVector3(_xyz.x(), -_xyz.z(), -_xyz.y())
        );
        AvatarMatrixRotation _tp = AvatarMatrixRotation.FromEulerXYZ(_tpVector);

        Value[0, 0] = _tp.Value[0, 0];
        Value[1, 0] = _tp.Value[0, 1];
        Value[2, 0] = _tp.Value[0, 2];
        Value[0, 1] = _tp.Value[1, 0];
        Value[1, 1] = _tp.Value[1, 1];
        Value[2, 1] = _tp.Value[1, 2];
        Value[0, 2] = _tp.Value[2, 0];
        Value[1, 2] = _tp.Value[2, 1];
        Value[2, 2] = _tp.Value[2, 2];
    }

    static public AvatarMatrixRotation Identity()
    {
        return new AvatarMatrixRotation(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        );
    }

    static protected AvatarMatrixRotation BaseRotationX(double x)
    {
        return new AvatarMatrixRotation(
            1, 0, 0,
            0, Math.Cos(x), -Math.Sin(x),
            0, Math.Sin(x), Math.Cos(x)
        );
    }
    static protected AvatarMatrixRotation BaseRotationY(double y)
    {
        return new AvatarMatrixRotation(
            Math.Cos(y), 0, Math.Sin(y),
            0, 1, 0,
            -Math.Sin(y), 0, Math.Cos(y)
        );
    }
    static protected AvatarMatrixRotation BaseRotationZ(double z)
    {
        return new AvatarMatrixRotation(
            Math.Cos(z), -Math.Sin(z), 0,
            Math.Sin(z), Math.Cos(z), 0,
            0, 0, 1
        );
    }
    static public AvatarMatrixRotation operator *(AvatarMatrixRotation _first, AvatarMatrixRotation _second)
    {
        return _first.Multiply(_second);
    }

    public AvatarMatrixRotation Multiply(AvatarMatrixRotation _other)
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        Multiply(this, _other, ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarMatrixRotation operator *(AvatarMatrixRotation _matrix, double _scalar)
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        _matrix.Multiply(_scalar, ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarMatrixRotation operator *(double _scalar, AvatarMatrixRotation _matrix)
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        _matrix.Multiply(_scalar, ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarMatrixRotation operator *(AvatarMatrixRotation _matrix, int _scalar)
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        _matrix.Multiply(_scalar, ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarMatrixRotation operator *(int _scalar, AvatarMatrixRotation _matrix)
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        _matrix.Multiply(_scalar, ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarVector3 operator *(AvatarMatrixRotation _matrix, AvatarVector3 _vector)
    {
        AvatarMatrix _result = new AvatarVector3();
        Multiply(_matrix, _vector, ref _result);
        return (AvatarVector3)_result;
    }

    public new AvatarMatrixRotation Transpose()
    {
        AvatarMatrix _result = new AvatarMatrixRotation();
        Transpose(ref _result);
        return (AvatarMatrixRotation)_result;
    }
    static public AvatarMatrixRotation FromEulerXYZ(double[] _xyz)
    {
        return FromEulerXYZ(_xyz[0], _xyz[1], _xyz[2]);
    }
    static public AvatarMatrixRotation FromEulerXYZ(AvatarVector3 _xyz)
    {
        return FromEulerXYZ(_xyz.Get(0), _xyz.Get(1), _xyz.Get(2));
    }

    static public AvatarMatrixRotation FromEulerXYZ(double x, double y, double z)
    {

        AvatarMatrixRotation BaseX = BaseRotationX(x);
        AvatarMatrixRotation BaseY = BaseRotationY(y);
        AvatarMatrixRotation BaseZ = BaseRotationZ(z);
        return BaseX * BaseY * BaseZ;
    }
    static public AvatarMatrixRotation FromEulerYXZ(AvatarVector3 _xyz)
    {
        return FromEulerYXZ(_xyz.Get(0), _xyz.Get(1), _xyz.Get(2));
    }

    static public AvatarMatrixRotation FromEulerYXZ(double x, double y, double z)
    {

        AvatarMatrixRotation BaseX = BaseRotationX(x);
        AvatarMatrixRotation BaseY = BaseRotationY(y);
        AvatarMatrixRotation BaseZ = BaseRotationZ(z);
        return BaseY * BaseX * BaseZ;
    }

    public AvatarVector3 ToEulerXYZ()
    {
        return new AvatarVector3(
            Math.Atan2(-Value[1, 2], Value[2, 2]),
            Math.Asin(Value[0, 2]),
            Math.Atan2(-Value[0, 1], Value[0, 0])
        );
    }
    public AvatarVector3 ToEulerYXZ()
    {
        double a = Math.Atan2(Value[1, 0], Value[1, 1]);
        return new AvatarVector3(
            Math.Asin(-Value[1, 2]),
            Math.Atan2(Value[0, 2], Value[2, 2]),
            Math.Atan2(Value[1, 0], Value[1, 1])
        );
    }

}
