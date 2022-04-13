using UnityEngine;

public class AvatarControllerData
{
    public int TimeIndex { get; protected set; }
    public AvatarMatrixRotation[] OrientationMatrix { get; protected set; }

	protected int NbExpectedSensors = -1;
	public bool[] IsSensorsSet;
	public int NbSensorsSet = 0;
	public bool AllSensorsReceived { get { return NbExpectedSensors == NbSensorsSet ? true : false; } }

	public AvatarControllerData(
		int _timeStamps,
		int _nbSensors
	)
	{
		TimeIndex = _timeStamps;
		NbExpectedSensors = _nbSensors;
		IsSensorsSet = new bool[NbExpectedSensors];

		OrientationMatrix = new AvatarMatrixRotation[NbExpectedSensors];
	}

	public bool AddData(
		int _sensorIndex,
		AvatarMatrixRotation _orientationMatrix
	)
	{
		if (IsSensorsSet[_sensorIndex])
		{
			Debug.Log("Data already set.");
			return false;
		}

		if (_sensorIndex >= NbExpectedSensors)
		{
			Debug.Log("Wrong index of sensor.");
			return false;
		}

		OrientationMatrix[_sensorIndex] = _orientationMatrix;

		IsSensorsSet[_sensorIndex] = true;
		NbSensorsSet++;
		return true;
	}
}
