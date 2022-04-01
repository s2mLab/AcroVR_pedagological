using System;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager3Segments : AvatarManager
{
	// Moving Joints
	public GameObject hips;
	public GameObject leftUpperLimb;
	public GameObject rightUpperLimb;

	protected override string BiomodPath()
	{
		return @"Assets/Avatar/Biorbd/model3Segments.bioMod";
	}

	protected override bool SetCalibrationPositionMatrices()
	{
		if (!CurrentData.AllSensorsSet)
		{
			return false;
		}

		// The first Sensor is the reference sensor to which all the others report wrt
		// With more segments, one should define a "parent" vector and Reference should be 
		// from that parent
		CalibrationMatrices_CalibInParent[0] = new AvatarMatrixRotation(CurrentData.OrientationMatrix[0]);
		for (int i = 1; i < Model.NbSegments; i++)
		{
			CalibrationMatrices_CalibInParent[i] =
				(CalibrationMatrices_CalibInParent[0] * AvatarOffset[i]).Transpose() * CurrentData.OrientationMatrix[i];
		}
		return true;
	}
	protected override AvatarMatrixRotation[] ProjectWrtToCalibrationPosition()
	{
		if (!IsZeroSet)
		{
			if (!SetCalibrationPositionMatrices())
			{
				return null;
			}
			IsZeroSet = true;
		}

		// output[0] is the reference for both Left and Right arm that is why we can
		// do this shortcut
		AvatarMatrixRotation[] _currentInCalib = new AvatarMatrixRotation[Model.NbSegments];
		_currentInCalib[0] = AvatarOffset[0]
			* CalibrationMatrices_CalibInParent[0].Transpose()
			* CurrentData.OrientationMatrix[0];
		for (int i = 1; i < Model.NbSegments; i++)
		{
			_currentInCalib[i] = AvatarOffset[i] * CalibrationMatrices_CalibInParent[i]
				* (
					(CalibrationMatrices_CalibInParent[0] * _currentInCalib[0] * CalibrationMatrices_CalibInParent[i]).Transpose()
					* CurrentData.OrientationMatrix[i]
				);
		}
		return _currentInCalib;
	}

	public override void SetSegmentsRotations(AvatarMatrixRotation[] _data)
	{
		double[][] _angles = DispatchToAngleVector(_data);
		double[][] _anglesAvatar = MapToAvatar(_angles);
		ApplyRotation(hips, _anglesAvatar[0]);
		ApplyRotation(leftUpperLimb, _anglesAvatar[1]);
		ApplyRotation(rightUpperLimb, _anglesAvatar[2]);
	}

	protected override AvatarMatrixRotation[] GetOrientationFromAvatar()
    {
		double[] _hipsOrientation = {
            hips.transform.localEulerAngles[0],
            hips.transform.localEulerAngles[1],
            hips.transform.localEulerAngles[2]
        };
		double[] _leftUpperLimbOrientation = {
            leftUpperLimb.transform.localEulerAngles[0],
            leftUpperLimb.transform.localEulerAngles[1],
            leftUpperLimb.transform.localEulerAngles[2]
        };
		double[] _rightUpperLimbOrientation = {
            rightUpperLimb.transform.localEulerAngles[0],
            rightUpperLimb.transform.localEulerAngles[1],
            rightUpperLimb.transform.localEulerAngles[2]
        };
		
		double[][] _result = { _hipsOrientation, _leftUpperLimbOrientation, _rightUpperLimbOrientation };
		return MapAvatarToInternal(_result);
	}

	protected double[][] DispatchToAngleVector(AvatarMatrixRotation[] _data)
	{
		double[][] _angles = new double[_data.Length][];
		for (int i = 0; i < _data.Length; ++i)
		{
			_angles[i] = _data[i].ToEulerYXZ();
		}
		return _angles;
	}

	protected AvatarMatrixRotation[] MapAvatarToInternal(double[][] _angles)
    {
		AvatarMatrixRotation[] _result = new AvatarMatrixRotation[3];

		// Hips
		{
			double[] tp = { -_angles[0][1], -_angles[0][0], -_angles[0][2] };
			_result[0] = AvatarMatrixRotation.FromEulerYXZ(MathUtils.ToRadian(tp));
		}

		// Left arm
		{
			double[] tp = { _angles[1][1], -_angles[1][2], -_angles[1][0] };
			_result[1] = AvatarMatrixRotation.FromEulerYXZ(MathUtils.ToRadian(tp));
		}

		// Right arm
		{
			double[] tp = { _angles[2][1], _angles[2][2], _angles[2][0] };
			_result[2] = AvatarMatrixRotation.FromEulerYXZ(MathUtils.ToRadian(tp));
		}

		return _result;
    }

	protected double[][] MapToAvatar(double[][] _angles)
	{
		double[][] _anglesDegree = MathUtils.ToDegree(_angles);

		double[][] _result = new double[3][];
		// Hips
		_result[0] = new double[3];
		_result[0][0] = -_anglesDegree[0][1];
		_result[0][1] = -_anglesDegree[0][0];
		_result[0][2] = -_anglesDegree[0][2];

		// Left arm
		_result[1] = new double[3];
		_result[1][0] = -_anglesDegree[1][2];
		_result[1][1] = _anglesDegree[1][0];
		_result[1][2] = -_anglesDegree[1][1];

		// Right arm
		_result[2] = new double[3];
		_result[2][0] = _anglesDegree[2][2];
		_result[2][1] = _anglesDegree[2][0];
		_result[2][2] = _anglesDegree[2][1];
		
		return _result;
    }
}
