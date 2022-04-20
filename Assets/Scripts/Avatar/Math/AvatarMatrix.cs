using System;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMatrix
{
    protected double[,] Value;
    public virtual double Get(int row, int col)
    {
        return Value[row, col];
    }
    public virtual void Set(int row, int col, double val)
    {
        Value[row, col] = val;
    }
    public int NbRows { get; protected set; }
    public int NbColumns { get; protected set; }

    public AvatarMatrix(int _nbRows, int _nbColumns)
    {
        NbRows = _nbRows;
        NbColumns = _nbColumns;
        Value = new double[NbRows, NbColumns];
    }

    public AvatarMatrix(AvatarMatrix _other)
    {
        NbRows = _other.NbRows;
        NbColumns = _other.NbColumns;
        Value = new double[NbRows, NbColumns];

        for (int j = 0; j < _other.NbColumns; j++)
        {
            for (int i = 0; i < _other.NbRows; i++)
            {
                Value[i, j] = _other.Value[i, j];
            }
        }
    }

    public AvatarMatrix(double[,] _other)
    {
        NbRows = _other.GetLength(0);
        NbColumns = _other.GetLength(1);
        Value = new double[NbRows, NbColumns];

        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                Value[i, j] = _other[i, j];
            }
        }
    }

    public double[,] ToDouble()
    {
        double[,] _result = new double[NbRows, NbColumns];
        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                _result[i, j] = Value[i, j];
            }
        }
        return _result;
    }
    public double[] ToDoubleVector()
    {
        double[] _result = new double[NbRows * NbColumns];
        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                _result[i + j * NbRows] = Value[i, j];
            }
        }
        return _result;
    }

    static public AvatarMatrix Identity(int _nbRows, int _nbCols)
    {
        AvatarMatrix _out = new AvatarMatrix(_nbRows, _nbCols);
        for (int i = 0; i < _out.NbRows; i++)
        {
            // Test for non square matrix
            if (i >= _out.NbRows || i >= _out.NbColumns) break;

            _out.Value[i, i] = 1;
        }
        return _out;
    }
    static public AvatarMatrix Zero(int _nbRows, int _nbCols)
    {
        return new AvatarMatrix(_nbRows, _nbCols);
    }
    public virtual void SetZero()
    {
        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                Value[i, j] = 0;
            }
        }
    }

    static public AvatarMatrix operator *(AvatarMatrix _first, AvatarMatrix _second)
    {
        return _first.Multiply(_second);
    }

    static protected void Multiply(
        AvatarMatrix _first, AvatarMatrix _second, ref AvatarMatrix _result
    )
    {
        if (_first.NbColumns != _second.NbRows)
        {
            Debug.Log("Invalid matrices size");
            _result = null;
            return;
        }

        _result.SetZero();
        for (int j = 0; j < _second.NbColumns; j++)
        {
            for (int i = 0; i < _first.NbRows; i++)
            {
                for (int k = 0; k < _first.NbColumns; k++)
                {
                    _result.Value[i, j] += _first.Value[i, k] * _second.Value[k, j];
                }
            }
        }
    }

    public AvatarMatrix Multiply(AvatarMatrix _other)
    {
        AvatarMatrix _result = new AvatarMatrix(NbRows, _other.NbColumns);
        Multiply(this, _other, ref _result);
        return _result;
    }

    static public AvatarMatrix operator *(AvatarMatrix _matrix, double _scalar)
    {
        AvatarMatrix _result = new AvatarMatrix(_matrix.NbRows, _matrix.NbColumns);
        _matrix.Multiply(_scalar, ref _result);
        return _result;
    }
    static public AvatarMatrix operator *(double _scalar, AvatarMatrix _matrix)
    {
        AvatarMatrix _result = new AvatarMatrix(_matrix.NbRows, _matrix.NbColumns);
        _matrix.Multiply(_scalar, ref _result);
        return _result;
    }
    static public AvatarMatrix operator *(AvatarMatrix _matrix, int _scalar)
    {
        AvatarMatrix _result = new AvatarMatrix(_matrix.NbRows, _matrix.NbColumns);
        _matrix.Multiply(_scalar, ref _result);
        return _result;
    }
    static public AvatarMatrix operator *(int _scalar, AvatarMatrix _matrix)
    {
        AvatarMatrix _result = new AvatarMatrix(_matrix.NbRows, _matrix.NbColumns);
        _matrix.Multiply(_scalar, ref _result);
        return _result;
    }
    public AvatarMatrix Multiply(double scalar, ref AvatarMatrix _result)
    {
        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                _result.Value[i, j] = scalar * Value[i, j];
            }
        }
        return _result;
    }

    protected void Transpose(ref AvatarMatrix _result)
    {
        for (int j = 0; j < _result.NbColumns; j++)
        {
            for (int i = 0; i < _result.NbRows; i++)
            {
                _result.Value[i, j] = Value[j, i];
            }
        }
    }

    public AvatarMatrix Transpose()
    {
        AvatarMatrix _result = new AvatarMatrix(NbColumns, NbRows);
        Transpose(ref _result);
        return _result;
    }

    new public string ToString()
    {
        string _result = "";
        for (int i = 0; i < NbRows; i++)
        {
            for (int j = 0; j < NbColumns; j++)
            {
                _result += Value[i, j] + " ";
            }
            _result += "\n";
        }
        return _result;
    }
    
    public static double[,] Cholesky(double[,] a)
    {
        // Input matrix must be square, symmetric and positive definite
        int n = (int)Math.Sqrt(a.Length);

        double[,] ret = new double[n, n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c <= r; c++)
            {
                if (c == r)
                {
                    double sum = 0;
                    for (int j = 0; j < c; j++)
                    {
                        sum += ret[c, j] * ret[c, j];
                    }
                    ret[c, c] = Math.Sqrt(a[c, c] - sum);
                }
                else
                {
                    double sum = 0;
                    for (int j = 0; j < c; j++)
                        sum += ret[r, j] * ret[c, j];
                    ret[r, c] = 1.0 / ret[c, c] * (a[r, c] - sum);
                }
            }

        return ret;
    }

}
