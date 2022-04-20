using System;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMatrix
{
    protected double[,] Value;
    public virtual double Get(int _row, int _col)
    {
        return Value[_row, _col];
    }
    public virtual AvatarMatrix Get(int _row, int _col, int _nbRows, int _nbColums)
    {
        AvatarMatrix _result = new AvatarMatrix(_nbRows, _nbColums);
        for (int j = 0; j < _nbColums; j++)
        {
            for (int i = 0; i < _nbRows; i++)
            {
                _result.Value[i, j] = Value[_row + i, _col + j];
            }
        }
        return _result;
    }

    public virtual void Set(int _row, int _col, double _val)
    {
        Value[_row, _col] = _val;
    }

    public virtual void Set(int _row, int _col, int _nbRows, int _nbCols, double _val)
    {
        for (int j = _col; j < _col + _nbCols; j++)
        {
            for (int i = _row; i < _row + _nbRows; i++)
            {
                Value[i, j] = _val;
            }
        }
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
    
    public AvatarMatrix Cholesky()
    {
        // Input matrix must be square, symmetric and positive definite, but this is not tested!


        //// Function was tested with the following matrix
        //Value = new double[6, 6] {
        //    { 65.3028 , 0.0 , 0.0 , 0.0 , 2.79774665, 1.39167295},
        //    { 0.0 , 65.3028 , 0.0 , 0.65577323, 0.95882407, 0.88445335},
        //    { 0.0 , 0.0 , 65.3028 , -3.74589558, -0.61565385, -2.63400759},
        //    { 0.0 , 0.65577323, -3.74589558, 3.94619281, -2.70013724,4.82985174},
        //    { 2.79774665, 0.95882407, -0.61565385, -2.70013724, 10.27543497, -1.87896653},
        //    { 1.39167295, 0.88445335, -2.63400759, 4.82985174, -1.87896653, 8.31802549}
        //};
        //AvatarMatrix ExpectedResult = new AvatarMatrix(new double[6, 6]
        //    {
        //        { 8.08101478776026,    0,                   0,                   0,                  0,                   0},
        //        { 0,                   8.08101478776026,    0,                   0,                  0,                   0},
        //        { 0,                   0,                   8.08101478776026,    0,                  0,                   0},
        //        { 0,                   0.0811498614002357, -0.463542720609006,   1.92995742340735,   0,                   0},
        //        { 0.346212291832153,   0.118651443560314,  -0.0761852151208147, -1.42235308240152,   2.84826286987442,    0},
        //        { 0.172215122302198,   0.109448302376531,  -0.325950101463686,   2.41967940431127,   0.514429400036013,   1.43201222580103}
        //    }
        //); 


        int _nbRowsAndCols = (int)Math.Sqrt(NbRows * NbColumns);

        AvatarMatrix _result = new AvatarMatrix(_nbRowsAndCols, _nbRowsAndCols);
        for (int _row = 0; _row < _nbRowsAndCols; _row++)
            for (int _col = 0; _col <= _row; _col++)
            {
                if (_col == _row)
                {
                    double _sum = 0;
                    for (int j = 0; j < _col; j++)
                    {
                        _sum += _result.Get(_col, j) * _result.Get(_col, j);
                    }
                    _result.Set(_col, _col, Math.Sqrt(Get(_col, _col) - _sum));
                }
                else
                {
                    double _sum = 0;
                    for (int j = 0; j < _col; j++)
                        _sum += _result.Get(_row, j) * _result.Get(_col, j);
                    _result.Set(_row, _col, 1.0 / _result.Get(_col, _col) * (Get(_row, _col) - _sum));
                }
            }

        return _result;
    }

}
