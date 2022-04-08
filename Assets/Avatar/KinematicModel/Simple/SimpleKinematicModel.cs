using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKinematicModel : AvatarKinematicModel
{
    protected AvatarMatrixRotation[] CalibrationMatrices;
    protected bool IsCalibrated = false;
    protected int ParentIndex(int _segment)
    {
        return _segment == 0 ? -1 : 0;  // Parent of all segments is the root
    }

    protected override void Initialize()
    {
        IsInitialized = true;
    }

    public override void ReloadModel()
    {
        return;
    }

    public SimpleKinematicModel(int _nbSegments, int _nbSensors) 
        : base(_nbSegments, _nbSensors)
    {
        CalibrationMatrices = new AvatarMatrixRotation[NbSegments];
        if (NbSegments != NbSensors)
        {
            Debug.Log(
                "SimpleKinematicModel is supposed to have a one-to-one" +
                "NbSegments/NbSensors relationship as the orientation of the sensors " +
                "are simply copied to the orientation of the segments");
        }
    }

    public override bool CalibrateModel(AvatarData _currentData)
    {
        if (_currentData == null || !_currentData.AllSensorsReceived) return false;

        for (int i = 0; i < NbSegments; i++)
        {
            int _parentIndex = ParentIndex(i);
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

        AvatarMatrixHomogenous[] _segmentsOrientation = new AvatarMatrixHomogenous[NbSegments];
        for (int i = 0; i < NbSegments; i++)
        {
            int _parentIndex = ParentIndex(i);
            // Root is the reference for all the segments, that is why we can do this shortcut
            AvatarMatrixRotation _orientationParentTransposed =
                _parentIndex < 0 ?
                AvatarMatrixRotation.Identity() : _currentData.OrientationMatrix[_parentIndex].Transpose();
            _segmentsOrientation[i] = new AvatarMatrixHomogenous(
                CalibrationMatrices[i].Transpose()
                * _orientationParentTransposed
                * _currentData.OrientationMatrix[i],
                new AvatarVector3()
            );
        }
        return new AvatarCoordinates(_currentData.TimeIndex, _segmentsOrientation);
    }
}
