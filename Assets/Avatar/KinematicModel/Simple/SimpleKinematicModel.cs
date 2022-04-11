using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKinematicModel : AvatarKinematicModel
{

    protected AvatarMatrixRotation[] CalibrationMatrices;

    public SimpleKinematicModel(KinematicModelInfo _modelInfo)
        : base(
            _modelInfo,
            ((SimpleKinematicModelInfo)_modelInfo).NbSegments, 
            ((SimpleKinematicModelInfo)_modelInfo).NbSensors
        )
    {
        CalibrationMatrices = new AvatarMatrixRotation[NbSegments];
        if (NbSegments != NbSensors)
        {
            Debug.Log(
                "SimpleKinematicModel is supposed to have a one-to-one" +
                "NbSegments/NbSensors relationship as the orientation of the sensors " +
                "are simply copied to the orientation of the segments");
        }
        Initialize();
    }

    protected override void Initialize()
    {
        IsInitialized = true;
    }

    public override bool CalibrateModel(AvatarData _currentData)
    {
        if (_currentData == null || !_currentData.AllSensorsReceived) return false;

        for (int i = 0; i < NbSegments; i++)
        {
            int _parentIndex = ((SimpleKinematicModelInfo)ModelInfo).ParentIndex[i];
            // Root is the reference for all the segments, that is why we can do this shortcut
            AvatarMatrixRotation _orientationParentTransposed =
                _parentIndex < 0 ?
                AvatarMatrixRotation.Identity() : _currentData.OrientationMatrix[_parentIndex].Transpose();
            CalibrationMatrices[i] = _orientationParentTransposed * _currentData.OrientationMatrix[i];
        }

        IsCalibrated = true;
        return true;
    }

    public override AvatarCoordinates InverseKinematics(AvatarData _currentData)
    {
        if (_currentData == null || !_currentData.AllSensorsReceived || !IsCalibrated) return null;

        AvatarVector _currentQ = AvatarVector.Zero(NbSegments * 3);

        for (int i = 0; i < NbSegments; i++)
        {
            int _parentIndex = ((SimpleKinematicModelInfo)ModelInfo).ParentIndex[i];
            // Root is the reference for all the segments, that is why we can do this shortcut
            AvatarMatrixRotation _orientationParentTransposed =
                _parentIndex < 0 ?
                AvatarMatrixRotation.Identity() : _currentData.OrientationMatrix[_parentIndex].Transpose();
            AvatarVector3 angles = (CalibrationMatrices[i].Transpose() * _orientationParentTransposed * _currentData.OrientationMatrix[i]).ToEulerYXZ();
            for (int j = 0; j < 3; j++)
            {
                _currentQ.Set(i * 3 + j, angles.Get(j));
            }
        }
        return new AvatarCoordinates(_currentData.TimeIndex, _currentQ);
    }
}
