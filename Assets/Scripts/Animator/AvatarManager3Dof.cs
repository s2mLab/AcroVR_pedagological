using System;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager3Dof : MonoBehaviour
{
	// Moving Joints
	public GameObject hips;
	public GameObject rightUpperArm;
	public GameObject leftUpperArm;

	float[] _anglesAvatar;

	[SerializeField] public string modelPath;
    BiorbdModel _biorbdModel;

    void Start()
	{
		_anglesAvatar = new float[8];
        _biorbdModel = new BiorbdModel(@"Assets/" + modelPath);
    }

	public void SetSegmentsRotations(double[] q)
	{
		FillAngles(q);

		PerformRotation(hips, new Vector3(_anglesAvatar[0], _anglesAvatar[1], _anglesAvatar[2]));
		PerformRotation(rightUpperArm, new Vector3(_anglesAvatar[3], _anglesAvatar[4], _anglesAvatar[5]));
		PerformRotation(leftUpperArm, new Vector3(_anglesAvatar[6], _anglesAvatar[7], _anglesAvatar[8]));
	}

	void FillAngles(double[] q)
	{
		// qAvatar = [q0, -q1, -q2, q3, -q4, -q5, q6, -q7, -q8].
		_anglesAvatar[0] = (float)MathUtils.ToDegree(q[0]);
		_anglesAvatar[1] = (float)MathUtils.ToDegree(q[1]);
		_anglesAvatar[2] = (float)MathUtils.ToDegree(q[2]);
		_anglesAvatar[3] = (float)MathUtils.ToDegree(q[3]);
		_anglesAvatar[4] = (float)MathUtils.ToDegree(q[4]);
		_anglesAvatar[5] = (float)MathUtils.ToDegree(q[5]);
		_anglesAvatar[6] = (float)MathUtils.ToDegree(q[6]);
		_anglesAvatar[7] = (float)MathUtils.ToDegree(q[7]);
		_anglesAvatar[8] = (float)MathUtils.ToDegree(q[8]);
    }

	void PerformRotation(GameObject Segment, Vector3 target)
	{
		if (Segment.transform.localEulerAngles != target)
			Segment.transform.localEulerAngles = target;
	}
}
