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

	// =================================================================================================================================================================
	/// <summary>
	/// <para> Vérification du signe d'un vecteur. </para>
	/// Retourne: -1 si vect inférieur à 0, 0 si vect égal 0, 1 si vect supérieur à 0
	/// </summary>

	public static int[] Sign(int[] vect)
	{
		int[] vectS = new int[vect.Length];
		for (int i = 0; i < vect.Length; i++)
		{
			if (vect[i] < 0) vectS[i] = -1;
			else if (vect[i] == 0) vectS[i] = 0;
			else vectS[i] = 1;
		}
		return vectS;
	}

	// =================================================================================================================================================================
	/// <summary> This is C source code for an implementation of one-dimensional phase unwrapping. It should produce results identical to the function in Matlab. </summary>
	// Ref: http://homepages.cae.wisc.edu/~brodskye/mr/phaseunwrap/

	public static double[] unwrap(double[] p)         // ported from matlab (Dec 2002)
	{
		int n = p.Length;
		double[] dp = new double[n];
		double[] dps = new double[n];
		double[] dp_corr = new double[n];
		double[] cumsum = new double[n];
		double[] pp = new double[n];
		double cutoff = Mathf.PI;               /* default value in matlab */
		int j;

		// incremental phase variation 
		// MATLAB: dp = diff(p, 1, 1);
		for (j = 0; j < n - 1; j++)
			dp[j] = p[j + 1] - p[j];

		// equivalent phase variation in [-pi, pi]
		// MATLAB: dps = mod(dp+dp,2*pi) - pi;
		for (j = 0; j < n - 1; j++)
			dps[j] = (dp[j] + Math.PI) - Math.Floor((dp[j] + Math.PI) / (2 * Math.PI)) * (2 * Math.PI) - Math.PI;

		// preserve variation sign for +pi vs. -pi
		// MATLAB: dps(dps==pi & dp>0,:) = pi;
		for (j = 0; j < n - 1; j++)
			if ((dps[j] == -Mathf.PI) && (dp[j] > 0))
				dps[j] = Mathf.PI;

		// incremental phase correction
		// MATLAB: dp_corr = dps - dp;
		for (j = 0; j < n - 1; j++)
			dp_corr[j] = dps[j] - dp[j];

		// Ignore correction when incremental variation is smaller than cutoff
		// MATLAB: dp_corr(abs(dp)<cutoff,:) = 0;
		for (j = 0; j < n - 1; j++)
			if (Math.Abs(dp[j]) < cutoff)
				dp_corr[j] = 0;

		// Find cumulative sum of deltas
		// MATLAB: cumsum = cumsum(dp_corr, 1);
		cumsum[0] = dp_corr[0];
		for (j = 1; j < n - 1; j++)
			cumsum[j] = cumsum[j - 1] + dp_corr[j];

		// Integrate corrections and add to P to produce smoothed phase values
		// MATLAB: p(2:m,:) = p(2:m,:) + cumsum(dp_corr,1);
		pp[0] = p[0];                       // Ajouter par Marcel Beaulieu, pour que la fonction soit complètement équivalente à la fonction Matlab
		for (j = 1; j < n; j++)
			pp[j] = p[j] + cumsum[j - 1];

		return pp;
	}

}
