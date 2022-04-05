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

	public override bool SetCalibrationPositionMatrices()
	{
		if (!CurrentData.AllSensorsConnected) return false;

		for (int i = 0; i < Model.NbSegments; i++)
		{
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				i == 0 ? AvatarMatrixRotation.Identity() : CurrentData.OrientationMatrix[0].Transpose();
			CalibrationMatrices[i] = _orientationParentTransposed * CurrentData.OrientationMatrix[i];
		}
		return true;
	}
	protected override AvatarMatrixRotation[] ProjectWrtToCalibrationPosition()
	{
		if (!IsCalibrated)
		{
			if (!SetCalibrationPositionMatrices()) return null;
			IsCalibrated = true;
		}

		AvatarMatrixRotation[] _currentInAvatar = new AvatarMatrixRotation[Model.NbSegments];
		for (int i = 0; i < Model.NbSegments; i++)
		{
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				i == 0 ? AvatarMatrixRotation.Identity() : CurrentData.OrientationMatrix[0].Transpose();
			_currentInAvatar[i] =
				CalibrationMatrices[i].Transpose() * _orientationParentTransposed 
				* CurrentData.OrientationMatrix[i];
		}
		return _currentInAvatar;
	}

	public override void SetSegmentsRotations(AvatarMatrixRotation[] _data)
	{
		double[][] _anglesAvatar = MapToAvatar(_data);
		ApplyRotation(hips, _anglesAvatar[0]);
		ApplyRotation(leftUpperLimb, _anglesAvatar[1]);
		ApplyRotation(rightUpperLimb, _anglesAvatar[2]);
	}
	protected double[][] MapToAvatar(AvatarMatrixRotation[] _data)
	{
		double[][] DispatchToAngleVector()
		{
			double[][] _angles = new double[_data.Length][];
			for (int i = 0; i < _data.Length; ++i)
			{
				_angles[i] = _data[i].ToEulerYXZ();
			}
			return MathUtils.ToDegree(_angles);
		}
		double[][] _anglesDegree = DispatchToAngleVector();

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
