using System;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Fonctions utilisées pour les différents calculs mathématiques. </summary>

public class Quintic
{
	// =================================================================================================================================================================
	/// <summary> Déterminer la valeur, à un temps T, en utilisant une interpolation de forme Quintic. </summary>

	public static void Eval(double t, double ti, double tj, double qi, double qj, out double p, out double v, out double a)
	{
		if (t < ti)
			t = ti;
		else if (t > tj)
			t = tj;
		double tp0 = tj - ti;
		double tp1 = t - ti;
		double tp2 = tp1 / tp0;
		double tp3 = tp2 * tp2;
		double tp4 = tp3 * tp2 * (6 * tp3 - 15 * tp2 + 10);
		double tp5 = qj - qi;
		double tp6 = tj - t;
		double tp7 = Math.Pow(tp0, 5);
		p = qi + tp5 * tp4;
		v = 30 * tp5 * tp1 * tp1 * tp6 * tp6 / tp7;
		a = 60 * tp5 * tp1 * tp6 * (tj + ti - 2 * t) / tp7;
	}

	// =================================================================================================================================================================
	/// <summary> Interpolation linéaire sur des données d'une dimension et t = 3 valeurs seulement (similaire à la fonction MatLab Interp1(x, v, xq, 'linear')). </summary>

	public static double[] Interp1(double ti, double[] t, double[,] x)
	{
		double[] y = new double[x.GetUpperBound(0) + 1];
		for (int i = 0; i < y.Length; i++)
		{
			if (ti <= t[1])
				y[i] = x[i, 0] + (ti - t[0]) * (x[i, 1] - x[i, 0]) / (t[1] - t[0]);
			else
				y[i] = x[i, 1] + (ti - t[1]) * (x[i, 2] - x[i, 1]) / (t[2] - t[1]);
		}
		return y;
	}
}
