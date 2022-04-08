using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class KalmanInterface : BiorbdInterface
{
    [DllImport(dllname)] protected static extern IntPtr c_BiorbdKalmanReconsIMU(IntPtr model, IntPtr QinitialGuess, double freq = 100, double noiseF = 5e-3, double errorF = 1e-10);
    [DllImport(dllname)] protected static extern void c_deleteBiorbdKalmanReconsIMU(IntPtr kalman);
    [DllImport(dllname)] protected static extern void c_BiorbdKalmanReconsIMUstep(IntPtr model, IntPtr kalman, IntPtr imu, IntPtr Q, IntPtr QDot, IntPtr QDDot);

    protected IntPtr _ptr_kalman_model = IntPtr.Zero;
    public bool IsKalmanInitialized;
    protected AvatarControllerModule ControllerModuleHandler ;

    public KalmanInterface(string _path, AvatarControllerModule _controllerModule) : base(_path)
    {
        ControllerModuleHandler = _controllerModule;
    }

    protected override void Initialize(string _path)
    {
        base.Initialize(_path);
        if (IsKalmanInitialized)
        {
            Debug.Log(String.Format("Kalman already initialized, Initialization skipped", _path));
            return;
        }

        _ptr_kalman_model = c_BiorbdKalmanReconsIMU(
            _ptr_model,
            VectorToPtrQ(AvatarVector.Zero(NbQ))
        );
    }
    protected override void CloseModel()
    {
        if (_ptr_kalman_model != IntPtr.Zero)
        {
            IsKalmanInitialized = false;
            c_deleteBiorbdKalmanReconsIMU(_ptr_kalman_model);
        }
        _ptr_kalman_model = IntPtr.Zero;
        base.CloseModel();
    }

    public override AvatarData ApplyFilter(AvatarData _currentData)
    {
        //// Apply the filter
        //c_BiorbdKalmanReconsIMUstep(
        //    _ptr_model,
        //    _ptr_kalman_model,
        //    ImuToImuPtr(_currentData.OrientationMatrix), 
        //    _ptr_q, 
        //    _ptr_qdot,
        //    _ptr_qddot
        //);
        //if (_ptr_q == IntPtr.Zero) return null;

        //// Convert to avatars angles
        //c_globalJCS(_ptr_model, _ptr_q, _ptr_allJcs);
        //AvatarMatrixHomogenous[] _jcs = PtrJcsToJcs();

        AvatarData _filteredData = _currentData;  // new AvatarData(_currentData.TimeIndex, 3); // _jcs.Length);
        //for (int i = 0; i < _jcs.Length; i++)
        //{
        //    _filteredData.AddData(i, _jcs[i].Rotation);
        //}
        return _filteredData;
    }
}
