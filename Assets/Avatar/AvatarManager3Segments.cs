using UnityEngine;

public class AvatarManager3Segments : AvatarManager
{
	public override string BiomodPath() { 
		return @"Assets/Avatar/KinematicModel/Biorbd/model3Segments.bioMod";
	}

	protected override KinematicModelInfo GetModelInfo()
    {
		if (typeof(BiorbdKinematicModel).IsInstanceOfType(KinematicModel))
        {
			int[] _parentIndex = { -1, 0, 0 };
			return new BiorbdKinematicModelInfo(SensorsInfo());
		}
		else
		{
			int[] _parentIndex = { -1, 0, 0 };
			return new SimpleKinematicModelInfo(_parentIndex, NbSegments(), NbSensors());
		}
    }

	protected BiorbdNode[] SensorsInfo()
    {
		BiorbdNode[] _info = new BiorbdNode[3];
		_info[0] = new BiorbdNode("HipsImu", "Hips");
		_info[1] = new BiorbdNode("LeftArmImu", "LeftArm");
		_info[2] = new BiorbdNode("RightArmImu", "RightArm");
		return _info;
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

	public override void SetSegmentsRotations(AvatarCoordinates _data)
	{
		AvatarVector[] _anglesAvatar = MapToAvatar(_data);
		ApplyRotation(hips, _anglesAvatar[0]);
		ApplyRotation(leftUpperLimb, _anglesAvatar[1]);
		ApplyRotation(rightUpperLimb, _anglesAvatar[2]);
	}
	protected AvatarVector3[] MapToAvatar(AvatarCoordinates _data)
	{
		AvatarVector3[] DispatchToAngleVector()
		{
			AvatarVector3[] _angles = new AvatarVector3[_data.Length];
			for (int i = 0; i < _data.Length; ++i)
			{
				_angles[i] = _data.Jcs[i].Rotation.ToEulerYXZ();
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
