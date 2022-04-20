using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class KalmanInterface : BiorbdInterface
{
    [DllImport(dllname)] protected static extern IntPtr c_BiorbdKalmanReconsIMU(IntPtr model, IntPtr QinitialGuess, double freq = 100, double noiseF = 5e-3, double errorF = 1e-10);
    [DllImport(dllname)] protected static extern void c_deleteBiorbdKalmanReconsIMU(IntPtr kalman);
    [DllImport(dllname)] protected static extern void c_BiorbdKalmanReconsIMUstep(IntPtr model, IntPtr kalman, IntPtr imu, IntPtr Q, IntPtr QDot, IntPtr QDDot);

    protected IntPtr _ptr_kalman_model = IntPtr.Zero;
    public bool IsKalmanInitialized;

    public KalmanInterface(KinematicModelInfo _kinematicModelInfo) 
        : base(_kinematicModelInfo)
    {

    }

    protected override void Initialize(string _path)
    {
        base.Initialize(_path);
        if (IsKalmanInitialized)
        {
            Debug.Log(String.Format("Kalman already initialized, Initialization skipped", _path));
            return;
        }
        InitializeKalman();
    }
    protected virtual void InitializeKalman()
    {
        if (_ptr_kalman_model != IntPtr.Zero) c_deleteBiorbdKalmanReconsIMU(_ptr_kalman_model);
        IsKalmanInitialized = false;
        double _noiseF = 1e-10;
        double _errorF = 1e-10;
        _ptr_kalman_model = c_BiorbdKalmanReconsIMU(
            _ptr_model,
            VectorToPtrQ(AvatarVector.Zero(NbQ)),
            FrameRate,
            _noiseF,
            _errorF
        );
        IsKalmanInitialized = true;
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

    public override void SetFrameRate(double _newFrameRate)
    {
        base.SetFrameRate(_newFrameRate);
        InitializeKalman();
    }

    protected override bool MemoryManagementAfterAddingImu()
    {
        base.MemoryManagementAfterAddingImu();
        InitializeKalman();
        return IsKalmanInitialized;
    }

    public override bool CalibrateModel(AvatarControllerData _currentData)
    {
        base.CalibrateModel(_currentData);
        if (_currentData == null || !_currentData.AllSensorsReceived) return false;
        return true;
    }

    public override AvatarCoordinates InverseKinematics(AvatarControllerData _currentData)
    {
        if (
            _currentData == null || 
            !_currentData.AllSensorsReceived || 
            !IsCalibrated
        ) return null;

        // Apply the filter
        c_BiorbdKalmanReconsIMUstep(
            _ptr_model, _ptr_kalman_model,
            ImuToImuPtr(_currentData.OrientationMatrix),
            _ptr_q, _ptr_qdot, _ptr_qddot
        );
        if (_ptr_q == IntPtr.Zero) return null;

        // Dispatch result
        AvatarVector _currentQ = PtrCoordinatesToVector(_ptr_q, NbQ);
        AvatarVector _currentQDot = PtrCoordinatesToVector(_ptr_qdot, NbQ);
        AvatarVector _currentQDDot = PtrCoordinatesToVector(_ptr_qddot, NbQ);

        AvatarCoordinates _filteredData = new AvatarCoordinates(_currentData.TimeIndex, _currentQ);
        return _filteredData;
    }
}