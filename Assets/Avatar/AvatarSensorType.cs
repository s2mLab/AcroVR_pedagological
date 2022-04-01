using System;
using UnityEngine;

public class AvatarSensorType : MonoBehaviour
{
	public SensorType type;
}


[Serializable]
public enum SensorType
{
	XSens,
	None
};
