using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarKinematicModel
{
    public bool IsInitialized = false;
    public bool IsCalibrated = false;
    protected abstract void Initialize();

    protected KinematicModelInfo ModelInfo;

    public AvatarKinematicModel(KinematicModelInfo _modelInfo, int _nbSegments, int _nbSensors)
    {
        ModelInfo = _modelInfo;
        NbSegments = _nbSegments;
        NbSensors = _nbSensors;
    }

    public abstract bool CalibrateModel(AvatarControllerData _currentData);
    public abstract AvatarCoordinates InverseKinematics(AvatarControllerData _currentData);

    public abstract AvatarVector FreeFloatingBaseDynamics(AvatarCoordinates _generalized);

    public virtual void SetFrameRate(double _newFrameRate)
    {
        FrameRate = _newFrameRate;
    }
    protected double FrameRate;

    public int NbSegments { get; protected set; }
    public int NbSensors { get; protected set; }

}
