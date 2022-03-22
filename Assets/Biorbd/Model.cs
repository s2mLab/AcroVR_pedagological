using System;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;

public class BiorbdModel : MonoBehaviour
{
	//Librairie et fonctions biorbd
#if UNITY_EDITOR
	const string dllpath = @"Assets\Biorbd\biorbd_c.dll";
#else
#if UNITY_STANDALONE_OSX
	const string dllpath = @"AcroVR/Contents/Resources/Data/Biorbd/libbiorbd.dylib";	// Fonctionne pas
	//static System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dllpath);
	//string fileInfo = info.FullName;
#else
	const string dllpath = @"..\Biorbd\biorbd_c.dll";
#endif
#endif
	[DllImport(dllpath)] public static extern IntPtr c_biorbdModel(StringBuilder pathToModel);
	[DllImport(dllpath)] public static extern IntPtr c_deleteBiorbdModel(IntPtr model);
	[DllImport(dllpath)] public static extern IntPtr c_writeBiorbdModel(IntPtr model, StringBuilder path);
	[DllImport(dllpath)] public static extern int c_nRoot(IntPtr model);
	[DllImport(dllpath)] public static extern int c_nQ(IntPtr model);
	[DllImport(dllpath)] public static extern int c_nQDot(IntPtr model);
	[DllImport(dllpath)] public static extern int c_nQDDot(IntPtr model);
	[DllImport(dllpath)] public static extern int c_nGeneralizedTorque(IntPtr model);
	[DllImport(dllpath)] public static extern int c_nMarkers(IntPtr model);

	[DllImport(dllpath)] public static extern void c_globalJCS(IntPtr model, IntPtr Q, IntPtr jcs);
	[DllImport(dllpath)] public static extern void c_inverseDynamics(IntPtr model, IntPtr q, IntPtr qdot, IntPtr qddot, IntPtr tau);
	[DllImport(dllpath)] public static extern void c_NonlinearEffects(IntPtr model, IntPtr q, IntPtr qdot, IntPtr tau);
	[DllImport(dllpath)] public static extern void c_massMatrix(IntPtr model, IntPtr q, IntPtr massMatrix);
	[DllImport(dllpath)] public static extern int c_markersInLocal(IntPtr model, IntPtr markPos);
	[DllImport(dllpath)] public static extern void c_markers(IntPtr model, IntPtr q, IntPtr markPos, bool removeAxis, bool updateKin);
	[DllImport(dllpath)] public static extern int c_nIMUs(IntPtr model);
	[DllImport(dllpath)] public static extern void c_addIMU(IntPtr model, IntPtr imuRT, StringBuilder name, StringBuilder parentName, bool technical = true, bool anatomical = true);
	[DllImport(dllpath)] public static extern IntPtr c_BiorbdKalmanReconsIMU(IntPtr model, IntPtr QinitialGuess, double freq = 100, double noiseF = 5e-3, double errorF = 1e-10);
	[DllImport(dllpath)] public static extern void c_deleteBiorbdKalmanReconsIMU(IntPtr kalman);
	[DllImport(dllpath)] public static extern void c_BiorbdKalmanReconsIMUstep(IntPtr model, IntPtr kalman, IntPtr imu, IntPtr Q, IntPtr QDo, IntPtr QDDot);
	//[DllImport(dllpath)] public static extern void c_getA(IntPtr kalman, IntPtr Mout);
	//[DllImport(dllpath)] public static extern void c_getJacobian();
	[DllImport(dllpath)] public static extern void c_matrixMultiplication(IntPtr M1, IntPtr M2, IntPtr Mout);
	[DllImport(dllpath)] public static extern void c_meanRT(IntPtr imuRT, int nFrame, IntPtr imuRT_mean);
	[DllImport(dllpath)] public static extern void c_projectJCSinParentBaseCoordinate(IntPtr parent, IntPtr jcs, IntPtr out1);
	[DllImport(dllpath)] public static extern void c_solveLinearSystem(IntPtr matA, int nbCol, int nbLigne, IntPtr matB, IntPtr solX);
	[DllImport(dllpath)] public static extern void c_alignSpecificAxisWithParentVertical(IntPtr r1, IntPtr r2, int idxAxe, IntPtr rot_out);
	[DllImport(dllpath)] public static extern void c_rotation(double v00, double v01, double v02, double v10, double v11, double v12, double v20, double v21, double v22, IntPtr rot_out);
	[DllImport(dllpath)] public static extern void c_rotationToEulerAngles(IntPtr rot, StringBuilder seq, IntPtr euler_out);
	[DllImport(dllpath)] public static extern void c_getGravity(IntPtr model, IntPtr gravity);
	[DllImport(dllpath)] public static extern void c_setGravity(IntPtr model, IntPtr newGravity);

	/// <summary> Pointeur qui désigne le modèle BioRBD utilisé pour AcroVR Offline. </summary>
	public IntPtr model;
	public int nRoot;
	public int nQ;
	public int nQDot;
	public int nQDDot;
	public int nTau;

	// Temporary vectors that are allocated for dll calls
	IntPtr _ptr_q;
	IntPtr _ptr_qdot;
	IntPtr _ptr_qddot;
	IntPtr _ptr_tau;
	IntPtr _ptr_massMatrix;
	double[] _q;
	double[] _qdot;
	double[] _qddot;
	double[] _tau;
	double[] _massMatrix;

	public BiorbdModel(string path)
    {
		model = c_biorbdModel(new StringBuilder(path));

		// Precompute some values to prevent unnecessary DLL calls
		nRoot = c_nRoot(model);
		nQ = c_nQ(model);
		nQDot = c_nQDot(model);
		nQDDot = c_nQDDot(model);
		nTau = c_nGeneralizedTorque(model);

		// Preallocate the vectors for communications with the DLL
		_ptr_q = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_qdot = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_qddot = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_massMatrix = Marshal.AllocCoTaskMem(sizeof(double) * nQ * nQ);
		_ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * nTau);

		_q = new double[nQ];
		_qdot = new double[nQ];
		_qddot = new double[nQ];
		_tau = new double[nTau];
		_massMatrix = new double[nQ * nQ];
	}

	public void write(string path)
    {
		c_writeBiorbdModel(model, new StringBuilder(path));
    }

	~BiorbdModel()
    {
		c_deleteBiorbdModel(model);
    }


	public double[] ShortDynamics(double t, double[] q, double[] qdot)
	{
		Marshal.Copy(q, 0, _ptr_q, nQ);
		Marshal.Copy(qdot, 0, _ptr_qdot, nQDot);
		Marshal.Copy(qddot, 0, _ptr_qddot, nQDDot);

		// Génère la matrice de masse

		c_massMatrix(model, ptr_Q, ptr_massMatrix);

		Marshal.Copy(ptr_massMatrix, massMatrix, 0, massMatrix.Length);

		MainParameters.c_inverseDynamics(MainParameters.Instance.modelBioRBDOffline, ptr_Q, ptr_V, ptr_qddot2, ptr_tau);

		Marshal.Copy(ptr_tau, m_taud, 0, m_taud.Length);

		double[,] squareMassMatrix = new double[NDDL, NDDL];
		squareMassMatrix = Vector.ToSquareMatrix(massMatrix);            // La matrice de masse générée est sous forme d'un vecteur de taille NDDL*NDDL

		double[,] matriceA = new double[NROOT, NROOT];
		matriceA = Matrix.ShrinkSquare(squareMassMatrix, NROOT);                // On réduit la matrice de masse

		double[] matAGrandVecteur = new double[NROOT * NROOT];
		matAGrandVecteur = Matrix.FromSquareToVector(matriceA);              // La nouvelle matrice doit être convertie en vecteur pour qu'elle puisse être utilisée dans BioRBD

		ptr_matA = Marshal.AllocCoTaskMem(sizeof(double) * matAGrandVecteur.Length);
		ptr_solX = Marshal.AllocCoTaskMem(sizeof(double) * NROOT);

		Marshal.Copy(matAGrandVecteur, 0, ptr_matA, matAGrandVecteur.Length);

		MainParameters.c_solveLinearSystem(ptr_matA, NROOT, NROOT, ptr_tau, ptr_solX);  // Résouds l'équation Ax=b

		double[] solutionX = new double[NROOT];
		Marshal.Copy(ptr_solX, solutionX, 0, solutionX.Length);

		for (int i = 0; i < NROOT; i++)
			qddot2[i] = -solutionX[i];

		for (int i = 0; i < NDDL; i++)
		{
			qddot1integ[i] = Vintegrateur[i];
			qddot1integ[i + NDDL] = qddot2[i];
		}

		qddot1integHumans = ConvertHumansBioRBD.Biorbd2Humans(qddot1integ);             // Convertir les DDL du modèle BioRBD vers le modèle Humans

		// Désallocation des pointeurs
		Marshal.FreeCoTaskMem(ptr_matA);
		Marshal.FreeCoTaskMem(ptr_solX);

		return new Microsoft.Research.Oslo.Vector(qddot1integHumans);
	}
}
