using System;
using UnityEngine;

// =================================================================================================================================================================
/// <summary> Script utilisé pour faire bouger l'avatar. </summary>

public class AvatarManager : MonoBehaviour
{
	public static AvatarManager Instance;

	// Référence sur les parents

	//public GameObject RightUpperArmParentGameObject;
	//public GameObject LeftUpperArmParentGameObject;
	//public GameObject humerusParentGameObject;

	// Parties du corps à manipuler

	public GameObject pelvisGameObject;
	public GameObject rightUpperArmGameObject;
	public GameObject leftUpperArmGameObject;
	//public GameObject thoraxGameObject;
	//public GameObject RHumerusGameObject;

	//

	Vector3 rightUpperArmInitial;
	Vector3 leftUpperArmInitial;

	// Rotations à appliquer aux segments

	Vector3 pelvisTarget;
	Vector3 rightUpperArmTarget;
	Vector3 leftUpperArmTarget;
	//Vector3 thoraxTarget;
	//Vector3 RHumerusTarget;

	double rad2Deg;

	// =================================================================================================================================================================
	/// <summary> Initialisation du script. </summary>

	void Start()
    {
		Instance = this;
		rad2Deg = 180 / Math.PI;

		// Initialisation des vecteurs de rotation

		pelvisTarget = new Vector3();
		rightUpperArmTarget = new Vector3();
		leftUpperArmTarget = new Vector3();
		//thoraxTarget = new Vector3();
		//RHumerusTarget = new Vector3();

		rightUpperArmInitial = rightUpperArmGameObject.transform.localEulerAngles;
		leftUpperArmInitial = leftUpperArmGameObject.transform.localEulerAngles;
	}

	// =================================================================================================================================================================
	/// <summary> Fixer les nouvelles rotations à appliquer aux segments. </summary>

	public void SetSegmentsRotations(double[] Q)
	{
		////hipsTarget = new Vector3((float)(Q[0] * 180 / Math.PI), -(float)(Q[1] * 180 / Math.PI), -(float)(Q[2] * 180 / Math.PI));
		//hipsTarget = new Vector3(0, -(float)(Q[2] * rad2Deg), 0);
		////leftUpperArmTarget = new Vector3((float)(Q[3] * rad2Deg), -(float)(Q[4] * rad2Deg), -(float)(Q[5] * rad2Deg));          // Effet miroir à l'écran (gauche <==> droit)
		////leftUpperArmTarget = new Vector3(0, 0, (float)(Q[4] * rad2Deg));
		//leftUpperArmTarget = new Vector3((float)(Q[3] * rad2Deg), 0, (float)(Q[4] * rad2Deg));
		//rightUpperArmTarget = new Vector3((float)(Q[6] * rad2Deg), 0, (float)(Q[7] * rad2Deg));

		//pelvisGameObject.transform.localRotation = Quaternion.AngleAxis((float)(Q[0] * rad2Deg), Vector3.right) * Quaternion.AngleAxis((float)(Q[2] * rad2Deg), Vector3.forward) *
		//											Quaternion.AngleAxis((float)(Q[1] * rad2Deg), Vector3.up);
		//pelvisGameObject.transform.localRotation = Quaternion.AngleAxis(180-(float)(Q[2] * rad2Deg), Vector3.up);
		//pelvisGameObject.transform.localRotation = Quaternion.AngleAxis(45, Vector3.right) * Quaternion.AngleAxis(180 - (float)(Q[2] * rad2Deg), Vector3.up);
		//RightUpperArmGameObject.transform.localRotation = Quaternion.AngleAxis((float)(Q[3] * rad2Deg), Vector3.up) * Quaternion.AngleAxis(-(float)(Q[4] * rad2Deg), Vector3.forward);
		//LeftUpperArmGameObject.transform.localRotation = Quaternion.AngleAxis((float)(Q[6] * rad2Deg), Vector3.up) * Quaternion.AngleAxis(-(float)(Q[7] * rad2Deg), Vector3.forward);

		// [q0, -q1, -q2, q3, -q4, -q5, q6, -q7, -q8].
		pelvisTarget = new Vector3((float)(Q[0] * rad2Deg), -(float)(Q[1] * rad2Deg), -(float)(Q[2] * rad2Deg));
		rightUpperArmTarget = new Vector3((float)(Q[4] * rad2Deg), (float)(Q[5] * rad2Deg), (float)(Q[3] * rad2Deg));
		leftUpperArmTarget = new Vector3((float)(Q[6] * rad2Deg), -(float)(Q[7] * rad2Deg), -(float)(Q[8] * rad2Deg));

		//pelvisTarget = new Vector3((float)(Q[1] * rad2Deg), -(float)(Q[0] * rad2Deg), (float)(Q[2] * rad2Deg));
		//rightUpperArmTarget = new Vector3(0, (float)(Q[5] * rad2Deg), (float)(Q[4] * rad2Deg));
		//leftUpperArmTarget = new Vector3(0, (float)(Q[8] * rad2Deg), -(float)(Q[7] * rad2Deg));

		//RightUpperArmTarget = new Vector3(-(float)(Q[5] * rad2Deg), (float)(Q[3] * rad2Deg), (float)(Q[4] * rad2Deg));
		//LeftUpperArmTarget = new Vector3((float)(Q[8] * rad2Deg), (float)(Q[6] * rad2Deg), -(float)(Q[7] * rad2Deg));
		rotateSegment(pelvisGameObject, pelvisTarget);
		rotateSegment(rightUpperArmGameObject, rightUpperArmTarget + rightUpperArmInitial);
		rotateSegment(leftUpperArmGameObject, leftUpperArmTarget + leftUpperArmInitial);

		//thoraxTarget = new Vector3(-(float)(Q[0] * rad2Deg), -(float)(Q[2] * rad2Deg), -(float)(Q[1] * rad2Deg));
		//RHumerusTarget = new Vector3((float)(Q[6] * rad2Deg), -(float)(Q[8] * rad2Deg), -(float)(Q[7] * rad2Deg));
		//rotateSegment(thoraxGameObject, thoraxTarget);
		//rotateSegment(RHumerusGameObject, RHumerusTarget);

		//if (TestXSens.nMsg < TestXSens.nMsgSize)
		//{
		//	TestXSens.msg[TestXSens.nMsg] = string.Format("SetSegmentsRotations: {0}, {1}, {2}, {3}, {4}, {5}", rootActualRotations.x, rootActualRotations.y, rootActualRotations.z,
		//		rootNewRotations.x, rootNewRotations.y, rootNewRotations.z);
		//	TestXSens.nMsg++;
		//}
	}

	public void UpdateParent(S2M_model s2m)		// Marcel: Jamais trouvé que cette fonction changeait quelques choses au mouvement de l'avatar, j'espère bien l'utiliser...
	{
		// humerus
		//double[] rt_tp = s2m.getCardanTransformationFromParentToSegment(1);
		//RightUpperArmParentGameObject.transform.localEulerAngles = new Vector3((float)(rt_tp[0] * rad2Deg), -(float)(rt_tp[2] * rad2Deg), -(float)(rt_tp[1] * rad2Deg + 90));
		//rt_tp = s2m.getCardanTransformationFromParentToSegment(2);
		//LeftUpperArmParentGameObject.transform.localEulerAngles = new Vector3((float)(rt_tp[3] * rad2Deg), -(float)(rt_tp[5] * rad2Deg), -(float)(rt_tp[4] * rad2Deg - 90));
		//double[] rt_tp = s2m.getCardanTransformationFromParentToSegment(2);
		//humerusParentGameObject.transform.localEulerAngles = new Vector3((float)(rt_tp[3] * rad2Deg), -(float)(rt_tp[4] * rad2Deg), -(float)(rt_tp[5] * rad2Deg) - 90);
	}

	// =================================================================================================================================================================
	/// <summary> Faire une rotation sur une segment. </summary>

	public void rotateSegment(GameObject Segment, Vector3 target)
	{
		if (Segment.transform.localEulerAngles != target)
			Segment.transform.localEulerAngles = target;
	}
}
