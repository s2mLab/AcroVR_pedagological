using System;
using System.Collections;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// Biorbd related variables
	public BiorbdModel KinematicModel { get; protected set; }
	public abstract string BiomodPath();
	[SerializeField] public bool UseKalmanFilter { get; protected set; }
	public abstract void CalibrateSensorToKinematicModel(AvatarData _data);


	// Model related variables
	protected abstract int ParentIndex(int _segment);
	public abstract int NbSegments();
	public abstract int NbSensors();


	// Controller related variables
	public AvatarModule ControllerModule { get; protected set; } = null;
	protected AvatarMatrixRotation[] CalibrationMatrices;
	protected bool IsCalibrated = false;
	protected AvatarData CurrentData = null;
	protected bool CurrentDataHasChanged = false;
	protected int PreviousDataIndex = -1;


	protected virtual void Start()
	{
		CalibrationMatrices = new AvatarMatrixRotation[NbSegments()];
		KinematicModel = new BiorbdModel(BiomodPath()); 
		if (!KinematicModel.IsInitialized)
		{
			Debug.Log("Could not load biorbd, UseKalmanFilter is set to false");
			UseKalmanFilter = false;
		}
	}

	protected void Update()
	{
		if (ControllerModule == null) return;

		FetchCurrentData();
		AvatarData _currentData = GetCurrentData();
		if (_currentData == null || !_currentData.AllSensorsConnected) return;

		AvatarMatrixRotation[] _data = ProjectWrtToCalibrationPosition();
		if (_data is null) return;

		SetSegmentsRotations(_data);
	}

	public abstract void SetSegmentsRotations(AvatarMatrixRotation[] _data);
	public virtual bool SetCalibrationPositionMatrices()
	{
		AvatarData _currentData = GetCurrentData();
		if (!_currentData.AllSensorsConnected) return false;

		for (int i = 0; i < NbSegments(); i++)
		{
			int _parentIndex = ParentIndex(i);
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				_parentIndex < 0 ?
				AvatarMatrixRotation.Identity() : _currentData.OrientationMatrix[_parentIndex].Transpose();
			CalibrationMatrices[i] = _orientationParentTransposed * _currentData.OrientationMatrix[i];
		}

		ControllerModule.CalibrateKinematicModel();
		return true;
	}
	protected virtual AvatarMatrixRotation[] ProjectWrtToCalibrationPosition()
	{
		if (!IsCalibrated)
		{
			if (!SetCalibrationPositionMatrices()) return null;
			IsCalibrated = true;
		}

		AvatarData _currentData = GetCurrentData();

		AvatarMatrixRotation[] _currentCalibrated = new AvatarMatrixRotation[NbSegments()];
		for (int i = 0; i < NbSegments(); i++)
		{
			int _parentIndex = ParentIndex(i);
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				_parentIndex < 0 ?
				AvatarMatrixRotation.Identity() : _currentData.OrientationMatrix[_parentIndex].Transpose();
			_currentCalibrated[i] =
				CalibrationMatrices[i].Transpose() * _orientationParentTransposed
				* _currentData.OrientationMatrix[i];
		}
		return _currentCalibrated;
	}

	public IEnumerator InitializeController(
		SensorType _sensorType,
		bool _useKalmanFilter,
		Action<int, int>  UpdateConnectingStatusCallback,
		Action ConectingIsCompletedCallback,
		Action InitializationFailedCallback
	)
    {
		if (ControllerModule != null)
        {
			Debug.Log("Controller already chosen. Restart the software to change controller.");
			InitializationFailedCallback();
			yield break;
		}

		if (_sensorType == SensorType.None)
		{
			Debug.Log("A controller must be chosen");
			InitializationFailedCallback();
			yield break;
		}
		else if (_sensorType == SensorType.XSens){
			ControllerModule = new XSensModule(this);
		}
		else if (_sensorType == SensorType.XSensFake)
        {
			ControllerModule = new XSensFakeModule(this);
        }
		else
        {
			throw new NotImplementedException("This controller is not implemented");
        }

		if (!ControllerModule.SetupMaterial())
        {
			InitializationFailedCallback();
			yield break;
		}

        while (!ControllerModule.SetupSensors())
        {
            UpdateConnectingStatusCallback(
				ControllerModule.NbSensorsConnected(), ControllerModule.NbSensorsExpected);
            yield return 0;
        }

		ControllerModule.FinalizeSetup();
		ConectingIsCompletedCallback();
		
		yield return 0;
	}

	protected virtual AvatarData GetCurrentData()
	{
		return CurrentData;
	}
	protected virtual void FetchCurrentData()
	{
		CurrentDataHasChanged = false;
		if (ControllerModule.IsSensorsConnected)
		{
			if (CurrentData == null 
				|| CurrentData.TimeIndex != ControllerModule.CurrentData.TimeIndex 
					&& ControllerModule.CurrentData.AllSensorsConnected
			)
			{
				CurrentData = ControllerModule.CurrentData;
				CurrentDataHasChanged = true;
			}
		}
	}

	protected void OnDestroy()
    {
		if (ControllerModule != null) ControllerModule.Disconnect();
	}


	protected void ApplyRotation(GameObject segment, AvatarVector target)
	{
		Vector3 targetVector = new Vector3((float)target.Get(0), (float)target.Get(1), (float)target.Get(2));
		if (segment.transform.localEulerAngles != targetVector)
        {
			segment.transform.localEulerAngles = targetVector;
		}
	}
}
