using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMatrixRotation
{
    public double[,] Value = new double[3, 3];

    public AvatarMatrixRotation()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Value[i, j] = 0;
            }
        }
    }

    public AvatarMatrixRotation(
        double r0c0, double r0c1, double r0c2,
        double r1c0, double r1c1, double r1c2,
        double r2c0, double r2c1, double r2c2
    )
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

    public AvatarMatrixRotation(XDA.XsMatrix other)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Value[i, j] = other.value((uint)i, (uint)j);
            }
        }
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

    static public AvatarMatrixRotation operator *(AvatarMatrixRotation first, AvatarMatrixRotation second)
    {
        return first.Multiply(second);
    }
    static public AvatarMatrixRotation operator *(AvatarMatrixRotation first, XDA.XsMatrix second)
    {
        return first.Multiply(second);
    }

    public AvatarMatrixRotation Multiply(AvatarMatrixRotation other)
    {
        AvatarMatrixRotation _result = new AvatarMatrixRotation();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    _result.Value[i, j] += Value[i, k] * other.Value[k, j];
                }
            }
        }
        return _result;
    }
    public AvatarMatrixRotation Multiply(XDA.XsMatrix other)
    {
        AvatarMatrixRotation _result = new AvatarMatrixRotation();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    _result.Value[i, j] += Value[i, k] * other.value((uint)k, (uint)j);
                }
            }
        }
        return _result;
    }

    public AvatarMatrixRotation Transpose()
    {
        AvatarMatrixRotation _result = new AvatarMatrixRotation();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _result.Value[i, j] = Value[j, i];
            }
        }
        return _result;
    }
    static public AvatarMatrixRotation FromEulerXYZ(double[] _xyz)
    {
        return FromEulerXYZ(_xyz[0], _xyz[1], _xyz[2]);
    }

    static public AvatarMatrixRotation FromEulerXYZ(double x, double y, double z)
    {

        AvatarMatrixRotation BaseX = BaseRotationX(x);
        AvatarMatrixRotation BaseY = BaseRotationY(y);
        AvatarMatrixRotation BaseZ = BaseRotationZ(z);
        return BaseX * BaseY * BaseZ;
    }
    static public AvatarMatrixRotation FromEulerYXZ(double[] _xyz)
    {
        return FromEulerYXZ(_xyz[0], _xyz[1], _xyz[2]);
    }

    static public AvatarMatrixRotation FromEulerYXZ(double x, double y, double z)
    {

        AvatarMatrixRotation BaseX = BaseRotationX(x);
        AvatarMatrixRotation BaseY = BaseRotationY(y);
        AvatarMatrixRotation BaseZ = BaseRotationZ(z);
        return BaseY * BaseX * BaseZ;
    }

    public double[] ToEulerXYZ()
    {
        double[] _result = new double[3];
        _result[0] = Math.Atan2(-Value[1, 2], Value[2, 2]);
        _result[1] = Math.Asin(Value[0, 2]);
        _result[2] = Math.Atan2(-Value[0, 1], Value[0, 0]);
        return _result;
    }
    public double[] ToEulerYXZ()
    {
        double[] _result = new double[3];
        _result[0] = Math.Asin(-Value[1, 2]);
        _result[1] = Math.Atan2(Value[0, 2], Value[2, 2]);
        _result[2] = Math.Atan2(Value[1, 0], Value[1, 1]);
        return _result;
    }

    new public string ToString()
    {
        return $"{Value[0, 0]} {Value[0, 1]} {Value[0, 2]}\n" +
               $"{Value[1, 0]} {Value[1, 1]} {Value[1, 2]}\n" +
               $"{Value[2, 0]} {Value[2, 1]} {Value[2, 2]}";
    }
}
