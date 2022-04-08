using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarKinematicModel
{
    public bool IsInitialized;
    protected abstract void Initialize();
    public abstract void ReloadModel();

    public AvatarKinematicModel(int _nbSegments, int _nbSensors)
    {
        NbSegments = _nbSegments;
        NbSensors = _nbSensors;
    }

    public abstract bool CalibrateModel(AvatarData _currentData);
    public abstract AvatarCoordinates InverseKinematics(AvatarData _currentData);

    public int NbSegments { get; protected set; }
    public int NbSensors { get; protected set; }

}
