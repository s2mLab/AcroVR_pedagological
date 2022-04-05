using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using XDA;

public class BiorbdModel
{
	const string dllname = "biorbd_eigen_c.dll";
#if UNITY_EDITOR
	const string dllfolder = @"Assets\Avatar\Biorbd\bin";
#else
#if UNITY_STANDALONE_OSX
	const string dllpath = @"AcroVR/Contents/Resources/Data/Biorbd/libbiorbd.dylib";	// Fonctionne pas
	//static System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(dllpath);
	//string fileInfo = info.FullName;
#else
	const string dllpath = @"..\Biorbd\bin\biorbd_c.dll";
#endif
#endif
	protected IntPtr rbdlHandlesDLL = DllManagement.LoadLib($"{dllfolder}\\rbdl.dll");
	protected IntPtr tinyXmlHandlesDLL = DllManagement.LoadLib($"{dllfolder}\\tinyxml.dll");
	protected IntPtr biorbdHandlesDLL = DllManagement.LoadLib($"{dllfolder}\\{dllname}");

    [DllImport(dllname)] public static extern IntPtr c_biorbdModel(StringBuilder pathToModel);
    [DllImport(dllname)] public static extern IntPtr c_deleteBiorbdModel(IntPtr model);
    [DllImport(dllname)] public static extern IntPtr c_writeBiorbdModel(IntPtr model, StringBuilder path);
    [DllImport(dllname)] public static extern int c_nSegments(IntPtr model);
    [DllImport(dllname)] public static extern int c_nRoot(IntPtr model);
    [DllImport(dllname)] public static extern int c_nQ(IntPtr model);
    [DllImport(dllname)] public static extern int c_nQDot(IntPtr model);
    [DllImport(dllname)] public static extern int c_nQDDot(IntPtr model);
    [DllImport(dllname)] public static extern int c_nGeneralizedTorque(IntPtr model);
    [DllImport(dllname)] public static extern int c_nMarkers(IntPtr model);

    //[DllImport(dllname)] public static extern void c_globalJCS(IntPtr model, IntPtr Q, IntPtr jcs);
    //[DllImport(dllname)] public static extern void c_inverseDynamics(IntPtr model, IntPtr q, IntPtr qdot, IntPtr qddot, IntPtr tau);
    //[DllImport(dllname)] public static extern void c_NonlinearEffects(IntPtr model, IntPtr q, IntPtr qdot, IntPtr tau);
    //[DllImport(dllname)] public static extern void c_massMatrix(IntPtr model, IntPtr q, IntPtr massMatrix);
    //[DllImport(dllname)] public static extern int c_markersInLocal(IntPtr model, IntPtr markPos);
    //[DllImport(dllname)] public static extern void c_markers(IntPtr model, IntPtr q, IntPtr markPos, bool removeAxis, bool updateKin);
    [DllImport(dllname)] public static extern int c_nIMUs(IntPtr model);
    //[DllImport(dllname)] public static extern void c_addIMU(IntPtr model, IntPtr imuRT, StringBuilder name, StringBuilder parentName, bool technical = true, bool anatomical = true);
    //[DllImport(dllname)] public static extern IntPtr c_BiorbdKalmanReconsIMU(IntPtr model, IntPtr QinitialGuess, double freq = 100, double noiseF = 5e-3, double errorF = 1e-10);
    //[DllImport(dllname)] public static extern void c_deleteBiorbdKalmanReconsIMU(IntPtr kalman);
    //[DllImport(dllname)] public static extern void c_BiorbdKalmanReconsIMUstep(IntPtr model, IntPtr kalman, IntPtr imu, IntPtr Q, IntPtr QDo, IntPtr QDDot);
    ////[DllImport(dllname)] public static extern void c_getA(IntPtr kalman, IntPtr Mout);
    ////[DllImport(dllname)] public static extern void c_getJacobian();
    //[DllImport(dllname)] public static extern void c_matrixMultiplication(IntPtr M1, IntPtr M2, IntPtr Mout);
    //[DllImport(dllname)] public static extern void c_meanRT(IntPtr imuRT, int nFrame, IntPtr imuRT_mean);
    //[DllImport(dllname)] public static extern void c_projectJCSinParentBaseCoordinate(IntPtr parent, IntPtr jcs, IntPtr out1);
    //[DllImport(dllname)] public static extern void c_solveLinearSystem(IntPtr matA, int nCol, int nRow, IntPtr vecB, IntPtr solX);
    //[DllImport(dllname)] public static extern void c_alignSpecificAxisWithParentVertical(IntPtr r1, IntPtr r2, int idxAxe, IntPtr rot_out);
    //[DllImport(dllname)] public static extern void c_rotation(double v00, double v01, double v02, double v10, double v11, double v12, double v20, double v21, double v22, IntPtr rot_out);
    //[DllImport(dllname)] public static extern void c_rotationToEulerAngles(IntPtr rot, StringBuilder seq, IntPtr euler_out);
    //[DllImport(dllname)] public static extern void c_getGravity(IntPtr model, IntPtr gravity);
    //[DllImport(dllname)] public static extern void c_setGravity(IntPtr model, IntPtr newGravity);

    /// <summary> Pointeur qui désigne le modèle BioRBD utilisé pour AcroVR Offline. </summary>
    public IntPtr _ptr_model;
	public bool IsInitialized { get; protected set; } = false;
	public int NbSegments { get; protected set; }
	public int NbRoot { get; protected set; }
	public int NbQ { get; protected set; }
	public int NbQDot { get; protected set; }
	public int NbQDDot { get; protected set; }
	public int NbTau { get; protected set; }
	public int NbMarkers { get; protected set; }
	public int NbImus { get; protected set; }

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

	void Initialize(string path)
    {
		if (IsInitialized || !File.Exists(path))
        {
			Debug.Log(String.Format("Biorbd model not found ({0}), Initialization skipped", path));
			return;
		}

        _ptr_model = c_biorbdModel(new StringBuilder(path));
        IsInitialized = true;

		// Precompute some values to prevent unnecessary DLL calls
		NbSegments = c_nSegments(_ptr_model);
		NbRoot = c_nRoot(_ptr_model);
		NbQ = c_nQ(_ptr_model);
		NbQDot = c_nQDot(_ptr_model);
		NbQDDot = c_nQDDot(_ptr_model);
		NbTau = c_nGeneralizedTorque(_ptr_model);
		NbMarkers = c_nMarkers(_ptr_model);
		NbImus = c_nIMUs(_ptr_model);

		// Preallocate the vectors for communications with the DLL
		_ptr_q = Marshal.AllocCoTaskMem(sizeof(double) * NbQ);
		_ptr_qdot = Marshal.AllocCoTaskMem(sizeof(double) * NbQ);
		_ptr_qddot = Marshal.AllocCoTaskMem(sizeof(double) * NbQ);
		_ptr_massMatrixVector = Marshal.AllocCoTaskMem(sizeof(double) * NbQ * NbQ);
		_ptr_massMatrixRootVector = Marshal.AllocCoTaskMem(sizeof(double) * NbRoot * NbRoot);
		_ptr_linearSolutionForRoot = Marshal.AllocCoTaskMem(sizeof(double) * NbRoot);
		_ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * NbTau);

		_q = new double[NbQ];
		_qdot = new double[NbQ];
		_qddot = new double[NbQ];
		_massMatrixVector = new double[NbQ * NbQ];
		_massMatrix = new double[NbQ, NbQ];
		_linearSolutionForRoot = new double[NbRoot];
	}

	public static void createModelFromStaticXsens(
		List<XsMatrix[]> statiqueTrial, 
		string pathToModel, 
		string pathToTemplate
	)
	{
		// Load of a generic model
		if (!System.IO.File.Exists(pathToTemplate))
		{
			Debug.Log("Template not found");
			return;
		}

		//IntPtr newModel = c_biorbdModel(new StringBuilder(pathToTemplate));

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
		//c_writeBiorbdModel(newModel, new StringBuilder(pathToModel));

		//calibre = true;

		//// Finaliser
		//m_isStaticDone = true;
	}

	public void write(string path)
    {
        c_writeBiorbdModel(_ptr_model, new StringBuilder(path));
    }


	public double[] RootDynamics(
		double t, double[] x, double[] q_measured, double[] qdot_measured, double[] qddot_measured
	)
	{
		// Dispatch entry values
		for (int i = 0; i < NbQ; i++)
		{
			_q[i] = x[i];
			_qdot[i] = x[i + NbQ];
		}
		for (int i = 0; i < NbQDDot; i++)
		{
			_qddot[i] = 
				i < NbRoot ? 
				0 : 
				qddot_measured[i] + 10 * (q_measured[i] - _q[i]) + 3 * (qdot_measured[i] - _qdot[i]);
		}
		Marshal.Copy(_q, 0, _ptr_q, NbQ);
		Marshal.Copy(_qdot, 0, _ptr_qdot, NbQDot);
		Marshal.Copy(_qddot, 0, _ptr_qddot, NbQDDot);

		// Compute the inverse dynamics
		//c_inverseDynamics(_ptr_model, _ptr_q, _ptr_qdot, _ptr_qddot, _ptr_tau);

		// Compute root dynamics
		//c_massMatrix(_ptr_model, _ptr_q, _ptr_massMatrixVector);
		Marshal.Copy(_ptr_massMatrixVector, _massMatrixVector, 0, NbQ * NbQ);
		_massMatrix = Vector.ToSquareMatrix(_massMatrixVector);
		double[,] massMatriceRoot = Matrix.Get(_massMatrix, 0, 0, NbRoot, NbRoot);
		double[] massMatriceRootVector = Matrix.toVector(massMatriceRoot);
		Marshal.Copy(massMatriceRootVector, 0, _ptr_massMatrixRootVector, NbRoot * NbRoot);

		// Compute the Tau for the root
		//c_solveLinearSystem(_ptr_massMatrixRootVector, NbRoot, NbRoot, _ptr_tau, _ptr_linearSolutionForRoot);
		Marshal.Copy(_ptr_linearSolutionForRoot, _linearSolutionForRoot, 0, NbRoot);

		double[] xdot = new double[NbQ + NbQDot];
		for (int i = 0; i < NbQ; i++)
		{
			xdot[i] = _qdot[i];
			xdot[i + NbQ] = i < NbRoot ? -_linearSolutionForRoot[i] : _qddot[i];
		}

		return xdot;
	}
}
