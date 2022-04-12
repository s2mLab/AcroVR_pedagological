using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class BiorbdInterface : AvatarKinematicModel
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
    [DllImport(dllname)] protected static extern int c_nIMUs(IntPtr model);
    [DllImport(dllname)] protected static extern void c_addIMU(IntPtr model, IntPtr imuRT, StringBuilder name, StringBuilder parentName, bool technical = true, bool anatomical = true);
    [DllImport(dllname)] protected static extern void c_IMU(IntPtr model, IntPtr Q, IntPtr output, bool updateKin = true);


    protected IntPtr _ptr_model = IntPtr.Zero;
    protected string OriginalPath;
    public int NbRoot { get; protected set; }
    public int NbQ { get; protected set; }
    public int NbQDot { get; protected set; }
    public int NbQDDot { get; protected set; }
    public int NbTau { get; protected set; }
    public int NbMarkers { get; protected set; }
    public int NbImus { get; protected set; }

    public BiorbdInterface(KinematicModelInfo _kinematicModelInfo) : 
        base(_kinematicModelInfo, 0, ((BiorbdKinematicModelInfo)_kinematicModelInfo).SensorNodes.Length)  // Initialize takes care NbSegments
    {
        Initialize(((BiorbdKinematicModelInfo)_kinematicModelInfo).BiorbdPath);
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
        IsCalibrated = false;

        Marshal.FreeCoTaskMem(_ptr_q);
        Marshal.FreeCoTaskMem(_ptr_qdot);
        Marshal.FreeCoTaskMem(_ptr_qddot);
        Marshal.FreeCoTaskMem(_ptr_tau);
        Marshal.FreeCoTaskMem(_ptr_massMatrixVector);
        Marshal.FreeCoTaskMem(_ptr_massMatrixRootVector);
        Marshal.FreeCoTaskMem(_ptr_allJcs);
        Marshal.FreeCoTaskMem(_ptr_imuRT);
        Marshal.FreeCoTaskMem(_ptr_allImus);
        Marshal.FreeCoTaskMem(_ptr_allImusRT);
        Marshal.FreeCoTaskMem(_ptr_linearSolutionForRoot);
    }
    public override bool CalibrateModel(AvatarControllerData _currentData)
    {

        if (_currentData == null || !_currentData.AllSensorsReceived) return false;

        // Make sure previous calibration are not interfering
        ReloadModel();

        AddImusFromGlobal(((BiorbdKinematicModelInfo)ModelInfo).SensorNodes, _currentData.OrientationMatrix);

        IsCalibrated = true;
        return true;
    }
    public virtual void ReloadModel()
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
    protected IntPtr _ptr_imuRT;
    protected IntPtr _ptr_allImus;
    protected IntPtr _ptr_allImusRT;
    protected IntPtr _ptr_linearSolutionForRoot;

    protected double[] _q;
    protected double[] _qdot;
    protected double[] _qddot;
    protected double[] _massMatrixVector;
    protected double[,] _massMatrix;
    protected double[] _allJcs;
    protected double[] _imuRT;
    protected double[] _allImus;
    protected double[] _allImusRT;
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
        _ptr_imuRT = Marshal.AllocCoTaskMem(sizeof(double) * 4 * 4);
        _ptr_allImus = Marshal.AllocCoTaskMem(sizeof(double) * NbImus * 3 * 3);
        _ptr_allImusRT = Marshal.AllocCoTaskMem(sizeof(double) * NbImus * 4 * 4);
        _ptr_linearSolutionForRoot = Marshal.AllocCoTaskMem(sizeof(double) * NbRoot);

        _q = new double[NbQ];
        _qdot = new double[NbQ];
        _qddot = new double[NbQ];
        _massMatrixVector = new double[NbQ * NbQ];
        _massMatrix = new double[NbQ, NbQ];
        _allJcs = new double[NbSegments * 16];
        _imuRT = new double[16];
        _allImus = new double[NbImus * 9];
        _allImusRT = new double[NbImus * 16];
        _linearSolutionForRoot = new double[NbRoot];

        IsInitialized = true;
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
            AvatarMatrixHomogenous[] _jcs = new AvatarMatrixHomogenous[NbSegments];
            for (int i = 0; i < _jcs.Length; i++)
            {
                _jcs[i] = AvatarMatrixHomogenous.Identity();
            }
            return _jcs;
        }
        c_globalJCS(_ptr_model, VectorToPtrQ(_generalizedCoordinates), _ptr_allJcs);
        return PtrJcsToJcs();
    }
    public AvatarMatrixHomogenous[] IMU(AvatarVector _generalizedCoordinates)
    {
        if (!IsInitialized || _ptr_model == null || _ptr_allJcs == null || _ptr_q == null)
        {
            AvatarMatrixHomogenous[] _jcs = new AvatarMatrixHomogenous[NbSegments];
            for (int i = 0; i < _jcs.Length; i++)
            {
                _jcs[i] = AvatarMatrixHomogenous.Identity();
            }
            return _jcs;
        }
        c_IMU(_ptr_model, VectorToPtrQ(_generalizedCoordinates), _ptr_allImusRT);
        return PtrAllImuRtToAllImuRt();
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
                _allJcs[i * 16 + 0], _allJcs[i * 16 + 4], _allJcs[i * 16 + 8 ], _allJcs[i * 16 + 12],
                _allJcs[i * 16 + 1], _allJcs[i * 16 + 5], _allJcs[i * 16 + 9 ], _allJcs[i * 16 + 13],
                _allJcs[i * 16 + 2], _allJcs[i * 16 + 6], _allJcs[i * 16 + 10], _allJcs[i * 16 + 14],
                _allJcs[i * 16 + 3], _allJcs[i * 16 + 7], _allJcs[i * 16 + 11], _allJcs[i * 16 + 15]
            );
        }
        return _result;
    }
    protected AvatarMatrixHomogenous[] PtrAllImuRtToAllImuRt()
    {
        Marshal.Copy(_ptr_allImusRT, _allImusRT, 0, NbImus * 16);
        AvatarMatrixHomogenous[] _result = new AvatarMatrixHomogenous[NbImus];
        for (int i = 0; i < NbImus; i++)
        {
            _result[i] = new AvatarMatrixHomogenous(
                _allImusRT[i * 16 + 0], _allImusRT[i * 16 + 4], _allImusRT[i * 16 + 8], _allImusRT[i * 16 + 12],
                _allImusRT[i * 16 + 1], _allImusRT[i * 16 + 5], _allImusRT[i * 16 + 9], _allImusRT[i * 16 + 13],
                _allImusRT[i * 16 + 2], _allImusRT[i * 16 + 6], _allImusRT[i * 16 + 10], _allImusRT[i * 16 + 14],
                _allImusRT[i * 16 + 3], _allImusRT[i * 16 + 7], _allImusRT[i * 16 + 11], _allImusRT[i * 16 + 15]
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
        Marshal.Copy(_matricesInVector, 0, _ptr_allImus, NbImus * 9);
        return _ptr_allImus;
    }

    protected bool AddImusFromGlobal(
        BiorbdNode[] _imuInfo,
        AvatarMatrixRotation[] _dataInGlobal
    )
    {
        AvatarMatrixRotation[] _dataInLocal = ProjectDataInLocalReferenceFrame(_dataInGlobal);
        return AddImusFromLocal(_imuInfo, _dataInLocal);
    }

    protected bool AddImusFromLocal(BiorbdNode[] _imuInfo, AvatarMatrixRotation[] _imuInLocal)
    {
        int _nbNewImus = _imuInLocal.Length;
        if (_imuInfo.Length != _nbNewImus)
        {
            Debug.Log("Wrong number of IMU information. Stopping calibration process");
            return false;
        }

        // Add to the model
        for (int i = 0; i < _nbNewImus; i++)
        {
            AvatarMatrixHomogenous _rt = new AvatarMatrixHomogenous(_imuInLocal[i], new AvatarVector3());
            Marshal.Copy(_rt.ToDoubleVector(), 0, _ptr_imuRT, 16);
            c_addIMU(_ptr_model, _ptr_imuRT, new StringBuilder(_imuInfo[i].Name), new StringBuilder(_imuInfo[i].ParentName));
        }

        MemoryManagementAfterAddingImu();
        return true;
    }

    protected virtual bool MemoryManagementAfterAddingImu()
    {
        NbImus = c_nIMUs(_ptr_model);
        Marshal.FreeCoTaskMem(_ptr_allImus);
        Marshal.FreeCoTaskMem(_ptr_allImusRT);
        _ptr_allImus = Marshal.AllocCoTaskMem(sizeof(double) * NbImus * 3 * 3);
        _ptr_allImusRT = Marshal.AllocCoTaskMem(sizeof(double) * NbImus * 4 * 4);
        _allImus = new double[NbImus * 9];
        _allImusRT = new double[NbImus * 16];
        return true;
    }

    protected AvatarMatrixRotation[] ProjectDataInLocalReferenceFrame(AvatarMatrixRotation[] _statiqueTrial)
    {
        AvatarVector _zeroPosition = new AvatarVector(NbQ);
        AvatarMatrixHomogenous[] _jcs = GlobalJcs(_zeroPosition);
        AvatarMatrixRotation[] _dataInLocal = new AvatarMatrixRotation[NbSegments];
        for (int i = 0; i < NbSegments; i++)
        {
            // This assumes one unit per joint
            _dataInLocal[i] = _jcs[i].Rotation.Transpose() * _statiqueTrial[i];
        }
        return _dataInLocal;
    }
}
