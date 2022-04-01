using System;
using System.Collections.Generic;

public abstract class AvatarModule
{
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
