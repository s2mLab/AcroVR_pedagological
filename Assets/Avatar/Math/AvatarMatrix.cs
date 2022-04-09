using System.Collections;
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

    public AvatarMatrix(XDA.XsMatrix _other)
    {
        NbRows = (int)_other.rows();
        NbColumns = (int)_other.cols();
        Value = new double[NbRows, NbColumns];

        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                Value[i, j] = _other.value((uint)i, (uint)j);
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
                _result[j * NbColumns + i] = Value[i, j];
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
    static public AvatarMatrix operator *(AvatarMatrix _first, XDA.XsMatrix _second)
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

    static protected void Multiply(
        AvatarMatrix _first, XDA.XsMatrix _second, ref AvatarMatrix _result
    )
    {
        if (_first.NbColumns != _second.rows())
        {
            Debug.Log("Invalid matrices size");
            _result = null;
            return;
        }

        _result.SetZero();
        for (int j = 0; j < _second.cols(); j++)
        {
            for (int i = 0; i < _first.NbRows; i++)
            {
                for (int k = 0; k < _first.NbColumns; k++)
                {
                    _result.Value[i, j] += _first.Value[i, k] * _second.value((uint)k, (uint)j);
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
    public AvatarMatrix Multiply(XDA.XsMatrix _other)
    {
        AvatarMatrix _result = new AvatarMatrix(NbRows, (int)_other.cols());
        Multiply(this, _other, ref _result);
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
        for (int j = 0; j < NbColumns; j++)
        {
            for (int i = 0; i < NbRows; i++)
            {
                _result += Value[i, j] + " ";
            }
            _result += "\n";
        }
        return _result;
    }
}
