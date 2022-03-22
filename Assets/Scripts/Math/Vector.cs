using System;

public class Vector
{
	public static double[] Initialize(int rows)
	{
		double[] result = new double[rows];
		return result;
	}

	public static double Min(params double[] vector)
	{
		int num = vector.Length;
		if (num == 0)
		{
			return 0f;
		}

		double num2 = vector[0];
		for (int i = 1; i < num; i++)
		{
			if (vector[i] < num2)
			{
				num2 = vector[i];
			}
		}

		return num2;
	}
	public static double Max(params double[] vector)
	{
		int num = vector.Length;
		if (num == 0)
		{
			return 0f;
		}

		double num2 = vector[0];
		for (int i = 1; i < num; i++)
		{
			if (vector[i] > num2)
			{
				num2 = vector[i];
			}
		}

		return num2;
	}


	// =================================================================================================================================================================
	/// <summary> Copié le contenu d'une matrice dans une nouvelle matrice. </summary>

	public static double[] Copy(double[] vector)
	{
		double[] newMatrix = new double[vector.GetUpperBound(0) + 1];
		for (int i = 0; i <= vector.GetUpperBound(0); i++)
			newMatrix[i] = vector[i];
		return newMatrix;
	}

	// =================================================================================================================================================================
	/// <summary> Convertir un vecteur en une matrice carré. </summary>

	public static double[,] ToSquareMatrix(double[] vector)
	{
		int dim = (int)System.Math.Sqrt(vector.Length);
		double[,] newMatrix = new double[dim, dim];
		for (int i = 0; i < newMatrix.GetLength(0); i++)
			for (int j = 0; j < newMatrix.GetLength(1); j++)
				newMatrix[j, i] = vector[j + newMatrix.GetLength(0) * i];
		return newMatrix;
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
		double cutoff = Math.PI;               /* default value in matlab */
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
			if ((dps[j] == -Math.PI) && (dp[j] > 0))
				dps[j] = Math.PI;

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
