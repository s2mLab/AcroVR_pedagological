using System;
using UnityEngine;

public class MathUtils
{
    static public double ToDegree(double radian)
    {
        return radian * 180 / Math.PI;
    }
    static public double[] ToDegree(double[] radian)
    {
        double[] result = new double[radian.Length];
        for (int i = 0; i < radian.Length; i++)
        {
            result[i] = ToDegree(radian[i]);
        }
        return result;
    }
    static public double[][] ToDegree(double[][] radian)
    {
        double[][] result = new double[radian.Length][];
        for (int i = 0; i < radian.Length; i++)
        {
            result[i] = ToDegree(radian[i]);
        }
        return result;
    }

    static public double ToRadian(double degree)
    {
        return degree * Math.PI / 180;
    }
    static public double[] ToRadian(double[] degree)
    {
        double[] result = new double[degree.Length];
        for (int i = 0; i < degree.Length; i++)
        {
            result[i] = ToRadian(degree[i]);
        }
        return result;
    }
    static public double[] ToRadian(Vector3 degree)
    {
        double[] result = new double[3];
        for (int i = 0; i < 3; i++)
        {
            result[i] = ToRadian(degree[i]);
        }
        return result;
    }
}