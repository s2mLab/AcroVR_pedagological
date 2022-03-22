using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using XDA;

public class BiorbdModel
{
	//Librairie et fonctions biorbd
#if UNITY_EDITOR
	const string dllpath = @"Assets\Biorbd\bin\biorbd_c.dll";
#else
#if UNITY_STANDALONE_OSX
	const string dllpath = @"AcroVR/Contents/Resources/Data/Biorbd/libbiorbd.dylib";	// Fonctionne pas
	//static System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dllpath);
	//string fileInfo = info.FullName;
#else
	const string dllpath = @"..\Biorbd\bin\biorbd_c.dll";
#endif
#endif
	[DllImport(dllpath)] public static extern IntPtr c_biorbdModel(StringBuilder pathToModel);
	[DllImport(dllpath)] public static extern IntPtr c_deleteBiorbdModel(IntPtr model);
	[DllImport(dllpath)] public static extern IntPtr c_writeBiorbdModel(IntPtr model, StringBuilder path);
	// [DllImport(dllpath)] public static extern int c_nRoot(IntPtr model);
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
	[DllImport(dllpath)] public static extern void c_solveLinearSystem(IntPtr matA, int nCol, int nRow, IntPtr vecB, IntPtr solX);
	[DllImport(dllpath)] public static extern void c_alignSpecificAxisWithParentVertical(IntPtr r1, IntPtr r2, int idxAxe, IntPtr rot_out);
	[DllImport(dllpath)] public static extern void c_rotation(double v00, double v01, double v02, double v10, double v11, double v12, double v20, double v21, double v22, IntPtr rot_out);
	[DllImport(dllpath)] public static extern void c_rotationToEulerAngles(IntPtr rot, StringBuilder seq, IntPtr euler_out);
	[DllImport(dllpath)] public static extern void c_getGravity(IntPtr model, IntPtr gravity);
	[DllImport(dllpath)] public static extern void c_setGravity(IntPtr model, IntPtr newGravity);

	/// <summary> Pointeur qui désigne le modèle BioRBD utilisé pour AcroVR Offline. </summary>
	public IntPtr _ptr_model;
	bool _initialized = false;
	public int nRoot;
	public int nQ;
	public int nQDot;
	public int nQDDot;
	public int nTau;
	public int nMarkers;
	public int nIMUs; 

	// Temporary vectors that are allocated for dll calls
	IntPtr _ptr_q;
	IntPtr _ptr_qdot;
	IntPtr _ptr_qddot;
	IntPtr _ptr_tau;
	IntPtr _ptr_massMatrixVector;
	IntPtr _ptr_massMatrixRootVector;
	IntPtr _ptr_linearSolutionForRoot;
	double[] _q;
	double[] _qdot;
	double[] _qddot;
	double[] _massMatrixVector;
	double[,] _massMatrix;
	double[] _linearSolutionForRoot;

	public BiorbdModel(string path)
    {
		Initialize(path);
    }

	void Initialize(string path)
    {
		if (_initialized || !File.Exists(path))
        {
			Debug.Log(String.Format("Biorbd model not found ({0}), Initialization skipped", path));
			return;
		}

		_ptr_model = c_biorbdModel(new StringBuilder(path));
		_initialized = true;

		// Precompute some values to prevent unnecessary DLL calls
		nRoot = 6; //  c_nRoot(_ptr_model);
		nQ = c_nQ(_ptr_model);
		nQDot = c_nQDot(_ptr_model);
		nQDDot = c_nQDDot(_ptr_model);
		nTau = c_nGeneralizedTorque(_ptr_model);
		nMarkers = c_nMarkers(_ptr_model);
		nIMUs = c_nIMUs(_ptr_model);

		// Preallocate the vectors for communications with the DLL
		_ptr_q = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_qdot = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_qddot = Marshal.AllocCoTaskMem(sizeof(double) * nQ);
		_ptr_massMatrixVector = Marshal.AllocCoTaskMem(sizeof(double) * nQ * nQ);
		_ptr_massMatrixRootVector = Marshal.AllocCoTaskMem(sizeof(double) * nRoot * nRoot);
		_ptr_linearSolutionForRoot = Marshal.AllocCoTaskMem(sizeof(double) * nRoot);
		_ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * nTau);

		_q = new double[nQ];
		_qdot = new double[nQ];
		_qddot = new double[nQ];
		_massMatrixVector = new double[nQ * nQ];
		_massMatrix = new double[nQ, nQ];
		_linearSolutionForRoot = new double[nRoot];
	}

	public static void createModelFromStaticXsens(List<XsMatrix[]> statiqueTrial, string pathToModel, string pathToTemplate)
	{
		// Load of a generic model
		if (!System.IO.File.Exists(pathToTemplate))
		{
			Debug.Log("Template not found");
			return;
		}

		IntPtr newModel = c_biorbdModel(new StringBuilder(pathToTemplate));

		//// moyenner les centrales
		//List<double[]> allIMUsMean = getIMUmean(m_s2m, statiqueTrial);
		////List<double[]> allIMUsMeanReoriented = getIMUmean(m_s2m, statiqueTrial);

		//// Trouver l'orientation du tronc (en comparant l'axe vertical de la première centrale avec celle du tronc)
		//List<double[]> allIMUsMeanReoriented = reorientIMUtoBodyVerticalAxisXsens(m_s2m, allIMUsMean);

		//// Remettre les IMUs dans le repère local par segment
		//List<double[]> allIMUsInLocal = computeIMUsInLocal(m_s2m, allIMUsMeanReoriented);

		//// Include translation to IMU
		//List<double[]> allIMUsInLocalWithTrans = addLocalTags(m_s2m, allIMUsInLocal);

		//// Ajouter les imus dans le modèle template
		//addIMUtoModel(m_s2m, allIMUsInLocalWithTrans);

		////System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("createModelFromStaticXsens: c_nTags = {0}{1}", c_nTags(m_s2m), System.Environment.NewLine));     // Debug Marcel
		//System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("createModelFromStaticXsens: allIMUsInLocalWithTrans.count = {0}{1}", allIMUsInLocalWithTrans.Count, System.Environment.NewLine));     // Debug Marcel
		//																																																					//System.IO.File.AppendAllText(@"C:\Devel\AcroVR_S2M_Debug.txt", string.Format("createModelFromStaticXsens: c_nIMUs = {0}{1}", c_nIMUs(m_s2m), System.Environment.NewLine));     // Debug Marcel

		// Generate new bioMod
		c_writeBiorbdModel(newModel, new StringBuilder(pathToModel));

		//calibre = true;

		//// Finaliser
		//m_isStaticDone = true;
	}

	public void write(string path)
    {
		c_writeBiorbdModel(_ptr_model, new StringBuilder(path));
    }

	~BiorbdModel()
    {
		Marshal.FreeCoTaskMem(_ptr_q);
		Marshal.FreeCoTaskMem(_ptr_qdot);
		Marshal.FreeCoTaskMem(_ptr_qddot);
		Marshal.FreeCoTaskMem(_ptr_massMatrixVector);
		Marshal.FreeCoTaskMem(_ptr_massMatrixRootVector);
		Marshal.FreeCoTaskMem(_ptr_linearSolutionForRoot);
		Marshal.FreeCoTaskMem(_ptr_tau);

		c_deleteBiorbdModel(_ptr_model);
	}


	public double[] RootDynamics(
		double t, double[] x, double[] q_measured, double[] qdot_measured, double[] qddot_measured
	)
	{
		// Dispatch entry values
		for (int i = 0; i < nQ; i++)
		{
			_q[i] = x[i];
			_qdot[i] = x[i + nQ];
		}
		for (int i = 0; i < nQDDot; i++)
		{
			_qddot[i] = 
				i < nRoot ? 
				0 : 
				qddot_measured[i] + 10 * (q_measured[i] - _q[i]) + 3 * (qdot_measured[i] - _qdot[i]);
		}
		Marshal.Copy(_q, 0, _ptr_q, nQ);
		Marshal.Copy(_qdot, 0, _ptr_qdot, nQDot);
		Marshal.Copy(_qddot, 0, _ptr_qddot, nQDDot);

		// Compute the inverse dynamics
		c_inverseDynamics(_ptr_model, _ptr_q, _ptr_qdot, _ptr_qddot, _ptr_tau);

		// Compute root dynamics
		c_massMatrix(_ptr_model, _ptr_q, _ptr_massMatrixVector);
		Marshal.Copy(_ptr_massMatrixVector, _massMatrixVector, 0, nQ * nQ);
		_massMatrix = Vector.ToSquareMatrix(_massMatrixVector);
		double[,] massMatriceRoot = Matrix.Get(_massMatrix, 0, 0, nRoot, nRoot);
		double[] massMatriceRootVector = Matrix.toVector(massMatriceRoot);
		Marshal.Copy(massMatriceRootVector, 0, _ptr_massMatrixRootVector, nRoot * nRoot);

		// Compute the Tau for the root
		c_solveLinearSystem(_ptr_massMatrixRootVector, nRoot, nRoot, _ptr_tau, _ptr_linearSolutionForRoot);
		Marshal.Copy(_ptr_linearSolutionForRoot, _linearSolutionForRoot, 0, nRoot);

		double[] xdot = new double[nQ + nQDot];
		for (int i = 0; i < nQ; i++)
		{
			xdot[i] = _qdot[i];
			xdot[i + nQ] = i < nRoot ? -_linearSolutionForRoot[i] : _qddot[i];
		}

		return xdot;
	}
}
