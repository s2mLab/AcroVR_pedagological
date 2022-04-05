using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarModule
{
    public AvatarModule(AvatarManager _avatar)
    {
        // Make sure decimal separator is the point (for instance, on french computers)
        System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        System.Globalization.CultureInfo ci = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        ci.NumberFormat = nfi;
        System.Threading.Thread.CurrentThread.CurrentCulture = ci;

        Avatar = _avatar;
        NbSensorsExpected = _avatar.NbSensors();
    }

    public int NbSensorsExpected { get; protected set; }
    public abstract int NbSensorsConnected();
    public bool IsSensorsConnected { get; protected set; } = false;

    // Preparation methods
    public abstract bool SetupMaterial();
    public abstract bool SetupSensors();
    public virtual bool FinalizeSetup()
    {
        CurrentData = null;
        return true;
    }
    public abstract void Disconnect();

    // Biorbd related methods
    protected AvatarManager Avatar;
    public bool UseKalmanFilter { get; protected set; }

    public void CalibrateKinematicModel()
    {
        // If no data exists, it is not possible to calibrate
        if (CurrentData == null) return;

        Avatar.CalibrateSensorToKinematicModel(CurrentData);
    }

    // Trial related variables
    public bool IsRecording { get; protected set; } = false;
    protected DateTime TrialStartingTime;
    public AvatarData CurrentData { get; protected set; } = null;
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
}
