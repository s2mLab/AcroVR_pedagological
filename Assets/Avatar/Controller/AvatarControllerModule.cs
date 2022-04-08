using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarControllerModule
{
    public AvatarControllerModule(AvatarManager _avatar)
    {
        // Make sure decimal separator is the point (for instance, on french computers)
        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        ci.NumberFormat = nfi;
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;

        AvatarHandler = _avatar;
        NbSensorsExpected = _avatar.NbSensors();
    }

    public double AcquisitionFrequency { get; protected set; }
    public int NbSensorsExpected { get; protected set; }
    public abstract int NbSensorsConnected();
    public bool IsSensorsConnected { get; protected set; } = false;

    // Preparation methods
    public abstract bool SetupMaterial();
    public abstract bool SetupSensors();
    public virtual bool FinalizeSetup()
    {
        _currentData = null;
        _currentFilteredData = null;
        return true;
    }
    public abstract void Disconnect();

    // Biorbd related methods
    protected AvatarManager AvatarHandler;

    public void CalibrateKinematicModel()
    {
        // If no data exists, it is not possible to calibrate
        if (CurrentData == null) return;

        AvatarHandler.CalibrateSensorToKinematicModel(CurrentData);
    }

    // Trial related variables
    public bool IsRecording { get; protected set; } = false;
    protected DateTime TrialStartingTime;
    protected AvatarData _currentData = null;
    protected AvatarData _currentFilteredData = null;
    public AvatarData CurrentData
    {
        get
        {
            return AvatarHandler.FilterKinematicsData ? _currentFilteredData : _currentData;
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

    protected virtual void SetNewFrame(int FrameNumber)
    {
        if (IsRecording && CurrentData != null)
        {
            TrialData.Add(CurrentData);
        }

        // Filters the previous data if requested
        if (AvatarHandler.FilterKinematicsData && _currentData != null)
        {
            if (AvatarHandler.KinematicModel == null)
            {
                _currentFilteredData = null;
            }
            else 
            {
                // There is a segfault here after recalibrating
                _currentFilteredData = _currentData;
                //_currentFilteredData = AvatarHandler.KinematicModel.ApplyFilter(_currentData); 
            }
        }

        _currentData = new AvatarData(FrameNumber, NbSensorsConnected());
    }
}
