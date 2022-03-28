using XDA;

public class XSensData
{
	public double TimeStamps { get; protected set; }
	public long TimeOfArrival { get; protected set; }
	public XsMatrix OrientationMatrix { get; protected set; }
	public XsEuler OrientationEuler { get; protected set; }
	public double[] OrientationEulerVector { get; protected set; }
	public XsQuaternion OrientationQuaternion { get; protected set; }
	public XsVector Acceleration { get; protected set; }

	public double[] EulerToDoubleArray()
    {
		return new double[3] {
			OrientationEuler.x(),
			OrientationEuler.y(),
			OrientationEuler.z()
		};
	}

	public XSensData(
		double _timeStamps,
		int _timeOfArrival,
		XsMatrix _orientationMatrix, 
		XsEuler _orientationEuler,
		XsQuaternion _orientationQuaternion,
		XsVector _acceleration
	)
    {
		TimeStamps = _timeStamps;
		TimeOfArrival = _timeOfArrival;
		OrientationMatrix = _orientationMatrix;
		OrientationEuler = _orientationEuler;
		OrientationQuaternion = _orientationQuaternion;
		Acceleration = _acceleration;
    }
}
