using UnityEngine;
using System.Timers;
using System.Text;
using System.Runtime.InteropServices;

public class AvatarBiorbd : KalmanInterface
{
    public AvatarVector CurrentQ { get; protected set; }
    Timer TimerHolder;

    public AvatarBiorbd(string _path, AvatarControllerModule _controllerModule) : 
        base(_path, _controllerModule)
    {
        
	}

    public override bool CalibrateModel(AvatarData _currentData)
    {
        throw new System.NotImplementedException();
    }

    protected override void Initialize()
    {
        Debug.Log("Initialize must be call with path for Biorbd");
    }

    protected override void Initialize(string _path)
    {
        base.Initialize(_path);
        CurrentQ = new AvatarVector(NbQ);

        TimerHolder = new Timer();
        TimerHolder.Interval = 500; // In milliseconds
        TimerHolder.AutoReset = true;
        TimerHolder.Elapsed += new ElapsedEventHandler(ChangeQ);
        TimerHolder.Start();

        void ChangeQ(object sender, ElapsedEventArgs e)
        {
            CurrentQ.Set(0, CurrentQ.Get(0) + 0.01);
        }
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
            Marshal.Copy(_rt.ToDoubleVector(), 0, _ptr_imu, 16);
            c_addIMU(_ptr_model, _ptr_imu, new StringBuilder(_imuInfo[i].Name), new StringBuilder(_imuInfo[i].ParentName));
        }

        NbImus = c_nIMUs(_ptr_model);
        return true;
    }

    protected AvatarMatrixRotation[] ProjectDataInLocalReferenceFrame(AvatarMatrixRotation[] _statiqueTrial)
    {
        AvatarVector _zeroPosition = new AvatarVector(NbQ);
        AvatarMatrixHomogenous[] _jcs = GlobalJcs(_zeroPosition);
        AvatarMatrixRotation[] _dataInLocal = new AvatarMatrixRotation[NbSegments];
        for (int i = 0; i < NbSegments; i++)
        {
            _dataInLocal[i] = _jcs[i].Rotation.Transpose() * _statiqueTrial[i];
        }
        return _dataInLocal;
    }

    public double[] RootDynamics(
        double t, double[] x, double[] q_measured, double[] qdot_measured, double[] qddot_measured
    )
    {
        //// Dispatch entry values
        //for (int i = 0; i < NbQ; i++)
        //{
        //	_q[i] = x[i];
        //	_qdot[i] = x[i + NbQ];
        //}
        //for (int i = 0; i < NbQDDot; i++)
        //{
        //	_qddot[i] = 
        //		i < NbRoot ? 
        //		0 : 
        //		qddot_measured[i] + 10 * (q_measured[i] - _q[i]) + 3 * (qdot_measured[i] - _qdot[i]);
        //}
        //Marshal.Copy(_q, 0, _ptr_q, NbQ);
        //Marshal.Copy(_qdot, 0, _ptr_qdot, NbQDot);
        //Marshal.Copy(_qddot, 0, _ptr_qddot, NbQDDot);

        //// Compute the inverse dynamics
        ////c_inverseDynamics(_ptr_model, _ptr_q, _ptr_qdot, _ptr_qddot, _ptr_tau);

        //// Compute root dynamics
        ////c_massMatrix(_ptr_model, _ptr_q, _ptr_massMatrixVector);
        //Marshal.Copy(_ptr_massMatrixVector, _massMatrixVector, 0, NbQ * NbQ);
        //_massMatrix = Vector.ToSquareMatrix(_massMatrixVector);
        //double[,] massMatriceRoot = Matrix.Get(_massMatrix, 0, 0, NbRoot, NbRoot);
        //double[] massMatriceRootVector = Matrix.toVector(massMatriceRoot);
        //Marshal.Copy(massMatriceRootVector, 0, _ptr_massMatrixRootVector, NbRoot * NbRoot);

        //// Compute the Tau for the root
        ////c_solveLinearSystem(_ptr_massMatrixRootVector, NbRoot, NbRoot, _ptr_tau, _ptr_linearSolutionForRoot);
        //Marshal.Copy(_ptr_linearSolutionForRoot, _linearSolutionForRoot, 0, NbRoot);

        double[] xdot = new double[NbQ + NbQDot];
        //for (int i = 0; i < NbQ; i++)
        //{
        //	xdot[i] = _qdot[i];
        //	xdot[i + NbQ] = i < NbRoot ? -_linearSolutionForRoot[i] : _qddot[i];
        //}

        return xdot;
    }
}
