using System;
using UnityEngine;

public class MathUtils
{
    static public double ToDegree(double _radian)
    {
        return _radian * 180 / Math.PI;
    }
    static public AvatarVector ToDegree(AvatarVector _radian)
    {
        AvatarVector _result = new AvatarVector(_radian.Length);
        ComputeToDegree(_radian, ref _result);
        return _result;
    }
    static public AvatarVector3 ToDegree(AvatarVector3 _radian)
    {
        AvatarVector _result = new AvatarVector3();
        ComputeToDegree(_radian, ref _result);
        return (AvatarVector3)_result;
    }
    static public AvatarVector[] ToDegree(AvatarVector[] _radian)
    {
        AvatarVector[] _result = new AvatarVector[_radian.Length];
        for (int i = 0; i < _radian.Length; i++)
        {
            _result[i] = ToDegree(_radian[i]);
        }
        return _result;
    }
    static public AvatarVector3[] ToDegree(AvatarVector3[] _radian)
    {
        AvatarVector3[] _result = new AvatarVector3[_radian.Length];
        for (int i = 0; i < _radian.Length; i++)
        {
            _result[i] = ToDegree(_radian[i]);
        }
        return _result;
    }
    static protected AvatarVector ComputeToDegree(AvatarVector _radian, ref AvatarVector _result)
    {
        for (int i = 0; i < _radian.Length; i++)
        {
            _result.Set(i, ToDegree(_radian.Get(i)));
        }
        return _result;
    }

    static public double ToRadian(double _degree)
    {
        return _degree * Math.PI / 180;
    }
    static public AvatarVector ToRadian(AvatarVector _degree)
    {
        AvatarVector _result = new AvatarVector(_degree.Length);
        ComputeToRadian(_degree, ref _result);
        return _result;
    }
    static public AvatarVector3 ToRadian(AvatarVector3 _degree)
    {
        AvatarVector _result = new AvatarVector3();
        ComputeToRadian(_degree, ref _result);
        return (AvatarVector3)_result;
    }
    static public AvatarVector3 ToRadian(Vector3 _degree)
    {
        AvatarVector3 _result = new AvatarVector3();
        for (int i = 0; i < 3; i++)
        {
            _result.Set(i, ToRadian(_degree[i]));
        }
        return _result;
    }
    static protected AvatarVector ComputeToRadian(AvatarVector _degree, ref AvatarVector _result)
    {
        for (int i = 0; i < _degree.Length; i++)
        {
            _result.Set(i, ToRadian(_degree.Get(i)));
        }
        return _result;
    }
}