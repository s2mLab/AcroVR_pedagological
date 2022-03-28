using XDA;
using UnityEngine;

public class XSensData
{
	public int TimeIndex { get; protected set; }
	protected int NbExpectedSensors = -1;
	public XsMatrix[] OrientationMatrix { get; protected set; }
	public XsEuler[] OrientationEuler { get; protected set; }
	public double[][] OrientationEulerVector { get; protected set; }
	public XsQuaternion[] OrientationQuaternion { get; protected set; }
	public XsVector[] Acceleration { get; protected set; }

	public bool[] IsSensorsSet;
	public int NbSensorsSet = 0;
	public bool AllSensorsSet { get { return NbExpectedSensors == NbSensorsSet ? true : false; } }

	public XSensData(
		int _timeStamps,
		int _nbSensors
	)
    {
		TimeIndex = _timeStamps;
		NbExpectedSensors = _nbSensors;
		IsSensorsSet = new bool[NbExpectedSensors];


		OrientationMatrix = new XsMatrix[NbExpectedSensors];
		OrientationEuler = new XsEuler[NbExpectedSensors];
		OrientationQuaternion = new XsQuaternion[NbExpectedSensors];
		Acceleration = new XsVector[NbExpectedSensors];
    }

	public bool AddData(
		uint _sensorIndex,
		XsMatrix _orientationMatrix,
		XsEuler _orientationEuler,
		XsQuaternion _orientationQuaternion,
		XsVector _acceleration
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
		OrientationEuler[_sensorIndex] = _orientationEuler;
		OrientationQuaternion[_sensorIndex] = _orientationQuaternion;
		Acceleration[_sensorIndex] = _acceleration;

		IsSensorsSet[_sensorIndex] = true;
		NbSensorsSet++;
		return true;
	}
}
