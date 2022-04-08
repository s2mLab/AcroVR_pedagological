using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarControllerModule
{
    protected AvatarKinematicModel KinematicModel;
    public AvatarControllerModule(AvatarKinematicModel _kinematicModel)
    {
        // Make sure decimal separator is the point (for instance, on french computers)
        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        ci.NumberFormat = nfi;
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;

        KinematicModel = _kinematicModel;
    }

    public double AcquisitionFrequency { get; protected set; }
    public int NbSensorsExpected { get { return KinematicModel.NbSensors; } }
    public abstract int NbSensorsConnected();
    public bool IsSensorsConnected { get; protected set; } = false;

    // Preparation methods
    public abstract bool SetupMaterial();
    public abstract bool SetupSensors();
    public virtual bool FinalizeSetup()
    {
        _currentData = null;
        _currentAvatarCoordinates = null;
        return true;
    }
    public abstract void Disconnect();

    // Trial related variables
    public bool IsRecording { get; protected set; } = false;
    protected DateTime TrialStartingTime;
    protected AvatarData _currentData = null;
    protected AvatarCoordinates _currentAvatarCoordinates = null;
    public AvatarCoordinates CurrentCoordinates
    {
        get
        {
            return _currentAvatarCoordinates;
        }
    }
    public List<AvatarData> TrialData { get; protected set; } = new List<AvatarData>();

    public virtual void StartTrial()
    {
        TrialStartingTime = DateTime.Now;
        IsRecording = true;
        TrialData.Clear();
    }
    public virtual void StopTrial()
    {
        IsRecording = false;
    }

    protected bool ShouldCalibrate = true;
    public void PlanKinematicModelCalibration()
    {
        ShouldCalibrate = true;
    }

    protected virtual void SetNewFrame(int FrameNumber)
    {
        if (IsRecording && CurrentCoordinates != null)
        {
            TrialData.Add(_currentData);
        }

        // Filters the previous data if requested
        if (_currentData != null && KinematicModel != null)
        {
            if (ShouldCalibrate)
            {
                KinematicModel.CalibrateModel(_currentData);
                ShouldCalibrate = false;
            }
            _currentAvatarCoordinates = KinematicModel.InverseKinematics(_currentData);
        }

        _currentData = new AvatarData(FrameNumber, NbSensorsConnected());
    }
}
