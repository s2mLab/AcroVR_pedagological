using UnityEngine;

public class AvatarManager3Segments : AvatarManager
{
	protected KinematicModelType KinModelType;

	public override string BiomodPath() { 
		return @"Assets/Avatar/KinematicModel/Biorbd/model3Segments.bioMod";
	}

	protected override KinematicModelInfo GetModelInfo(KinematicModelType _type)
    {
		KinModelType = _type;
		if (KinModelType == KinematicModelType.Biorbd)
        {
			return new BiorbdKinematicModelInfo(BiomodPath(), SensorsInfo);
		}
		else if (KinModelType == KinematicModelType.Simple)
		{
			int[] _parentIndex = { -1, 0, 0 };
			return new SimpleKinematicModelInfo(_parentIndex, NbSegments(), NbSensors());
		}
        else
        {
			Debug.Log("Wrong choice of Kinematic Model type");
			return null;
        }
    }

	protected BiorbdNode[] SensorsInfo
	{
		get
		{
			BiorbdNode[] _info = new BiorbdNode[3];
			_info[0] = new BiorbdNode("HipsImu", "Hips");
			_info[1] = new BiorbdNode("LeftArmImu", "LeftArm");
			_info[2] = new BiorbdNode("RightArmImu", "RightArm");
			return _info;
		}
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
		AvatarVector3[] _angles = new AvatarVector3[3];
		if (KinModelType == KinematicModelType.Simple)
		{
			// Conversion schema
			// hips		  -ry  -rx  -rz
			// LeftArm	  -ry   rx  -rz
			// RightArm	   ry  -rx   rz
			_angles[0] = new AvatarVector3(-_data.Q.Get(1), -_data.Q.Get(0), -_data.Q.Get(2));
			_angles[1] = new AvatarVector3(-_data.Q.Get(5),  _data.Q.Get(3), -_data.Q.Get(4));
			_angles[2] = new AvatarVector3( _data.Q.Get(8),  _data.Q.Get(6),  _data.Q.Get(7));
		}
		else if (KinModelType == KinematicModelType.Biorbd)
		{
			// Conversion schema
			// hips		 tx ty tz  -ry   rx  -rz
			// LeftArm			    ry   rx   rz
			// RightArm			   -ry  -rx   rz
			_angles[0] = new AvatarVector3(_data.Q.Get(4), -_data.Q.Get(3), -_data.Q.Get(5));
			_angles[1] = new AvatarVector3(-_data.Q.Get(7), _data.Q.Get(6), -_data.Q.Get(8));
			_angles[2] = new AvatarVector3(_data.Q.Get(10), -_data.Q.Get(9), -_data.Q.Get(11));
		}
		else
		{
			Debug.Log("Model not implemented");
			return null;
		}
		return MathUtils.ToDegree(_angles);
	}
}
