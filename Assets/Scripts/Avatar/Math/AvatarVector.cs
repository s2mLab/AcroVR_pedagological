using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarVector : AvatarMatrix
{

    public AvatarVector(AvatarVector _other)
        : base(_other.Length, 1)
    {
        for (int i = 0; i < _other.Length; i++)
        {
            Value[i, 0] = _other.Value[i, 0];
        }
    }

    public AvatarVector(double[] _other)
        : base(_other.Length, 1)
    {
        for (int i = 0; i < _other.Length; i++)
        {
            Value[i, 0] = _other[i];
        }
    }

    public virtual double Get(int row)
    {
        return Value[row, 0];
    }
    public new AvatarVector Get(int _row, int _nbRows)
    {
        AvatarVector _result = new AvatarVector(_nbRows);
        for (int i = 0; i < _nbRows; i++)
        {
            _result.Value[i, 0] = Value[_row + i, 0];
        }
        return _result;
    }
    public virtual void Set(int row, double val)
    {
        Value[row, 0] = val;
    }
    public new void Set(int _row, int _nbRows, double _val)
    {
        for (int i = _row; i < _row + _nbRows; i++)
        {
            Value[i, 0] = _val;
        }
    }

    static public AvatarVector Zero(int _nbRows)
    {
        return new AvatarVector(_nbRows);
    }

    public int Length { get { return NbRows; } }

    public AvatarVector(int _nbElements) : base(_nbElements, 1)
    {

    }
    public new double[] ToDouble()
    {
        double[] _result = new double[NbRows];
        for (int i = 0; i < NbRows; i++)
        {
            _result[i] = Value[i, 0];
        }
        return _result;
    }

    static public AvatarVector operator *(AvatarMatrix _first, AvatarVector _second)
    {
        AvatarMatrix _result = new AvatarVector(_first.NbRows);
        Multiply(_first, _second, ref _result);
        return (AvatarVector)_result;
    }
}
