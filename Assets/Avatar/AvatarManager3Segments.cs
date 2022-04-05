using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager3Segments : AvatarManager
{
	public override string BiomodPath() { 
		return @"Assets/Avatar/Biorbd/model3Segments.bioMod"; 
	}

	// Moving Joints
	public GameObject hips;
	public GameObject leftUpperLimb;
	public GameObject rightUpperLimb;

	public override int NbSegments()
	{
		return 3;
	}
	public override int NbSensors()
	{
		return 3;
	}

	protected override int ParentIndex(int _segment)
    {
		return _segment == 0 ? -1 : 0;  // Parent of all segments is hips
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
