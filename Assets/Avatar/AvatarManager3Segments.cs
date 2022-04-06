using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager3Segments : AvatarManager
{
	public override string BiomodPath() { 
		return @"Assets/Avatar/Biorbd/model3Segments.bioMod";
	}
	public override void CalibrateSensorToKinematicModel(AvatarData _data)
    {
		KinematicModel = new BiorbdModel(@"Assets/Avatar/Biorbd/model3SegmentsWithImu.bioMod");
		return;
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
		AvatarVector[] _anglesAvatar = MapToAvatar(_data);
		ApplyRotation(hips, _anglesAvatar[0]);
		ApplyRotation(leftUpperLimb, _anglesAvatar[1]);
		ApplyRotation(rightUpperLimb, _anglesAvatar[2]);
	}
	protected AvatarVector3[] MapToAvatar(AvatarMatrixRotation[] _data)
	{
		AvatarVector3[] DispatchToAngleVector()
		{
			AvatarVector3[] _angles = new AvatarVector3[_data.Length];
			for (int i = 0; i < _data.Length; ++i)
			{
				_angles[i] = _data[i].ToEulerYXZ();
			}
			return MathUtils.ToDegree(_angles);
		}
		AvatarVector3[] _anglesDegree = DispatchToAngleVector();

		AvatarVector3[] _result = new AvatarVector3[3];
		// Hips
		_result[0] = new AvatarVector3(-_anglesDegree[0].Get(1), -_anglesDegree[0].Get(0), -_anglesDegree[0].Get(2));

		// Left arm
		_result[1] = new AvatarVector3(-_anglesDegree[1].Get(2), _anglesDegree[1].Get(0), -_anglesDegree[1].Get(1));

		// Right arm
		_result[2] = new AvatarVector3(_anglesDegree[2].Get(2), _anglesDegree[2].Get(0), _anglesDegree[2].Get(1));

		return _result;
	}
}
