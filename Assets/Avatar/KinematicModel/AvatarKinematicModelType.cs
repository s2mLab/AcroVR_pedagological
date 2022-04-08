using System;
using UnityEngine;

public class AvatarKinematicModelType : MonoBehaviour
{
	public KinematicModelType type;
}


[Serializable]
public enum KinematicModelType
{
	Biorbd,
	Simple,
	None
};