using UnityEngine;
using System.Timers;
using System.Text;
using System.Runtime.InteropServices;

public class BiorbdKinematicModel : KalmanInterface
{
    public AvatarVector CurrentQ { get; protected set; }
    Timer TimerHolder;

    public BiorbdKinematicModel(KinematicModelInfo _kinematicModelInfo) : 
        base(_kinematicModelInfo)
    {
        
	}

    public override bool CalibrateModel(AvatarData _currentData)
    {
        // Make sure previous calibration are not interfering
        ReloadModel();

        AddImusFromGlobal(((BiorbdKinematicModelInfo)ModelInfo).SensorNodes, _currentData.OrientationMatrix);
        return true;
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
