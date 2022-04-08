using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class BiorbdInterface
{
    protected const string dllname = "biorbd_eigen_c.dll";
#if UNITY_EDITOR
    protected const string dllfolder = @"Assets\Avatar\KinematicModel\Biorbd\bin";
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

    [DllImport(dllname)] protected static extern IntPtr c_biorbdModel(StringBuilder pathToModel);
    [DllImport(dllname)] protected static extern IntPtr c_deleteBiorbdModel(IntPtr model);
    [DllImport(dllname)] protected static extern IntPtr c_writeBiorbdModel(IntPtr model, StringBuilder path);
    [DllImport(dllname)] protected static extern int c_nSegments(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nRoot(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nQ(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nQDot(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nQDDot(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nGeneralizedTorque(IntPtr model);
    [DllImport(dllname)] protected static extern int c_nMarkers(IntPtr model);

    [DllImport(dllname)] protected static extern void c_globalJCS(IntPtr model, IntPtr Q, IntPtr jcs);
    //[DllImport(dllname)] protected static extern void c_inverseDynamics(IntPtr model, IntPtr q, IntPtr qdot, IntPtr qddot, IntPtr tau);
    //[DllImport(dllname)] protected static extern void c_NonlinearEffects(IntPtr model, IntPtr q, IntPtr qdot, IntPtr tau);
    //[DllImport(dllname)] protected static extern void c_massMatrix(IntPtr model, IntPtr q, IntPtr massMatrix);
    //[DllImport(dllname)] protected static extern int c_markersInLocal(IntPtr model, IntPtr markPos);
    //[DllImport(dllname)] protected static extern void c_markers(IntPtr model, IntPtr q, IntPtr markPos, bool removeAxis, bool updateKin);
    [DllImport(dllname)] protected static extern int c_nIMUs(IntPtr model);
    [DllImport(dllname)] protected static extern void c_addIMU(IntPtr model, IntPtr imuRT, StringBuilder name, StringBuilder parentName, bool technical = true, bool anatomical = true);
    ////[DllImport(dllname)] protected static extern void c_getJacobian();
    //[DllImport(dllname)] protected static extern void c_matrixMultiplication(IntPtr M1, IntPtr M2, IntPtr Mout);
    //[DllImport(dllname)] protected static extern void c_meanRT(IntPtr imuRT, int nFrame, IntPtr imuRT_mean);
    //[DllImport(dllname)] protected static extern void c_projectJCSinParentBaseCoordinate(IntPtr parent, IntPtr jcs, IntPtr out1);
    //[DllImport(dllname)] protected static extern void c_solveLinearSystem(IntPtr matA, int nCol, int nRow, IntPtr vecB, IntPtr solX);
    //[DllImport(dllname)] protected static extern void c_alignSpecificAxisWithParentVertical(IntPtr r1, IntPtr r2, int idxAxe, IntPtr rot_out);
    //[DllImport(dllname)] protected static extern void c_rotation(double v00, double v01, double v02, double v10, double v11, double v12, double v20, double v21, double v22, IntPtr rot_out);
    //[DllImport(dllname)] protected static extern void c_rotationToEulerAngles(IntPtr rot, StringBuilder seq, IntPtr euler_out);
    //[DllImport(dllname)] protected static extern void c_getGravity(IntPtr model, IntPtr gravity);
    //[DllImport(dllname)] protected static extern void c_setGravity(IntPtr model, IntPtr newGravity);



    protected IntPtr _ptr_model = IntPtr.Zero;
    protected string OriginalPath;
    public bool IsInitialized;
    public int NbSegments { get; protected set; }
    public int NbRoot { get; protected set; }
    public int NbQ { get; protected set; }
    public int NbQDot { get; protected set; }
    public int NbQDDot { get; protected set; }
    public int NbTau { get; protected set; }
    public int NbMarkers { get; protected set; }
    public int NbImus { get; protected set; }
    public abstract AvatarData ApplyFilter(AvatarData _currentData);

    public BiorbdInterface(string _path)
    {
        Initialize(_path);
    }
    ~BiorbdInterface()
    {
        CloseModel();
    }
    protected virtual void CloseModel()
    {
        if (IsInitialized)
        {
            IsInitialized = false;
            c_deleteBiorbdModel(_ptr_model);
        }
        _ptr_model = IntPtr.Zero;

        Marshal.FreeCoTaskMem(_ptr_q);
        Marshal.FreeCoTaskMem(_ptr_qdot);
        Marshal.FreeCoTaskMem(_ptr_qddot);
        Marshal.FreeCoTaskMem(_ptr_tau);
        Marshal.FreeCoTaskMem(_ptr_massMatrixVector);
        Marshal.FreeCoTaskMem(_ptr_massMatrixRootVector);
        Marshal.FreeCoTaskMem(_ptr_allJcs);
        Marshal.FreeCoTaskMem(_ptr_imu);
        Marshal.FreeCoTaskMem(_ptr_linearSolutionForRoot);
    }
    public void ReloadModel()
    {
        CloseModel();
        Initialize(OriginalPath);
    }

    // Temporary vectors that are allocated for dll calls
    protected IntPtr _ptr_q;
    protected IntPtr _ptr_qdot;
    protected IntPtr _ptr_qddot;
    protected IntPtr _ptr_tau;
    protected IntPtr _ptr_massMatrixVector;
    protected IntPtr _ptr_massMatrixRootVector;
    protected IntPtr _ptr_allJcs;
    protected IntPtr _ptr_imu;
    protected IntPtr _ptr_linearSolutionForRoot;

    protected double[] _q;
    protected double[] _qdot;
    protected double[] _qddot;
    protected double[] _massMatrixVector;
    protected double[,] _massMatrix;
    protected double[] _allJcs;
    protected double[] _imu;
    protected double[] _linearSolutionForRoot;


    protected virtual void Initialize(string _path)
    {
        if (IsInitialized || !File.Exists(_path))
        {
            Debug.Log(String.Format("Biorbd model not found ({0}), Initialization skipped", _path));
            return;
        }

        OriginalPath = _path;
        _ptr_model = c_biorbdModel(new StringBuilder(_path));

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
        _ptr_tau = Marshal.AllocCoTaskMem(sizeof(double) * NbTau);
        _ptr_massMatrixVector = Marshal.AllocCoTaskMem(sizeof(double) * NbQ * NbQ);
        _ptr_massMatrixRootVector = Marshal.AllocCoTaskMem(sizeof(double) * NbRoot * NbRoot);
        _ptr_allJcs = Marshal.AllocCoTaskMem(sizeof(double) * NbSegments * 4 * 4);
        _ptr_imu = Marshal.AllocCoTaskMem(sizeof(double) * 3 * 3);
        _ptr_linearSolutionForRoot = Marshal.AllocCoTaskMem(sizeof(double) * NbRoot);

        _q = new double[NbQ];
        _qdot = new double[NbQ];
        _qddot = new double[NbQ];
        _massMatrixVector = new double[NbQ * NbQ];
        _massMatrix = new double[NbQ, NbQ];
        _allJcs = new double[NbSegments * 16];
        _imu = new double[16];
        _linearSolutionForRoot = new double[NbRoot];
    }




    // Interface to c functions
    public void write(string path)
    {
        c_writeBiorbdModel(_ptr_model, new StringBuilder(path));
    }

    public AvatarMatrixHomogenous[] GlobalJcs(AvatarVector _generalizedCoordinates)
    {
        if (!IsInitialized || _ptr_model == null || _ptr_allJcs == null || _ptr_q == null)
        {
            Debug.Log("coucou!");
            AvatarMatrixHomogenous[] tata = new AvatarMatrixHomogenous[3];
            for (int i = 0; i < tata.Length; i++)
            {
                tata[i] = AvatarMatrixHomogenous.Identity();
            }
            return tata;
        }
        c_globalJCS(_ptr_model, VectorToPtrQ(_generalizedCoordinates), _ptr_allJcs);
        return PtrJcsToJcs();
    }

    // Dispatch functions
    protected IntPtr VectorToPtrQ(AvatarVector _generalizedCoordinates)
    {
        return VectorToPtrCoordinates(_generalizedCoordinates, ref _ptr_q, NbQ);
    }
    protected IntPtr VectorToPtrCoordinates(AvatarVector _generalizedCoordinates, ref IntPtr _ptr, int _nbElements)
    {
        Marshal.Copy(_generalizedCoordinates.ToDouble(), 0, _ptr, _nbElements);
        return _ptr;
    }
    protected AvatarVector PtrCoordinatesToVector(IntPtr _ptr, int _nbElements)
    {
        double[] _generalizedCoordinates = new double[_nbElements];
        Marshal.Copy(_ptr, _generalizedCoordinates, 0, _nbElements);
        return new AvatarVector(_generalizedCoordinates);
    }

    protected AvatarMatrixHomogenous[] PtrJcsToJcs()
    {
        Marshal.Copy(_ptr_allJcs, _allJcs, 0, NbSegments * 16);
        int _nbRt = _allJcs.Length / 16;
        AvatarMatrixHomogenous[] _result = new AvatarMatrixHomogenous[_nbRt];
        for (int i = 0; i < _nbRt; i++)
        {
            _result[i] = new AvatarMatrixHomogenous(
                _allJcs[i * 16 + 0], _allJcs[i * 16 + 4], _allJcs[i * 16 + 8], _allJcs[i * 16 + 12],
                _allJcs[i * 16 + 1], _allJcs[i * 16 + 5], _allJcs[i * 16 + 9], _allJcs[i * 16 + 13],
                _allJcs[i * 16 + 2], _allJcs[i * 16 + 6], _allJcs[i * 16 + 10], _allJcs[i * 16 + 14],
                _allJcs[i * 16 + 3], _allJcs[i * 16 + 7], _allJcs[i * 16 + 11], _allJcs[i * 16 + 15]
            );
        }
        return _result;
    }

    protected IntPtr ImuToImuPtr(AvatarMatrixRotation[] _orientationMatrix)
    {
        double[] _matricesInVector = new double[_orientationMatrix.Length * 9];
        for (int i = 0; i < _orientationMatrix.Length; i++)
        {
            _matricesInVector[i * 9 + 0] = _orientationMatrix[i].Get(0, 0);
            _matricesInVector[i * 9 + 1] = _orientationMatrix[i].Get(1, 0);
            _matricesInVector[i * 9 + 2] = _orientationMatrix[i].Get(2, 0);
            _matricesInVector[i * 9 + 3] = _orientationMatrix[i].Get(0, 1);
            _matricesInVector[i * 9 + 4] = _orientationMatrix[i].Get(1, 1);
            _matricesInVector[i * 9 + 5] = _orientationMatrix[i].Get(2, 1);
            _matricesInVector[i * 9 + 6] = _orientationMatrix[i].Get(0, 2);
            _matricesInVector[i * 9 + 7] = _orientationMatrix[i].Get(1, 2);
            _matricesInVector[i * 9 + 8] = _orientationMatrix[i].Get(2, 2);
        }
        Marshal.Copy(_matricesInVector, 0, _ptr_imu, _orientationMatrix.Length * 9);
        return _ptr_imu;
    }
}
