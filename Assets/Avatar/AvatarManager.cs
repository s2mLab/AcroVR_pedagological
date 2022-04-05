using System;
using System.Collections;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
    // Biorbd related variables
    public BiorbdModel Model { get; protected set; }

	protected AvatarMatrixRotation[] CalibrationMatrices;
	protected bool IsCalibrated = false;


	// Controller related variables
	public AvatarModule ControllerModule { get; protected set; } = null;
    protected AvatarData CurrentData = null;
	protected bool CurrentDataHasChanged = false;
	protected int PreviousDataIndex = -1;

	protected abstract string BiomodPath();
	protected abstract int ParentIndex(int _segment);

	protected void Start()
	{
		Model = new BiorbdModel(BiomodPath());
		CalibrationMatrices = new AvatarMatrixRotation[Model.NbSegments];
	}

	void Update()
	{
		if (ControllerModule == null) return;

		GetCurrentData();
		if (CurrentData == null || !CurrentData.AllSensorsConnected) return;

		AvatarMatrixRotation[] _data = ProjectWrtToCalibrationPosition();
		if (_data is null) return;

		SetSegmentsRotations(_data);
	}

	public abstract void SetSegmentsRotations(AvatarMatrixRotation[] _data);
	public virtual bool SetCalibrationPositionMatrices()
	{
		if (!CurrentData.AllSensorsConnected) return false;

		for (int i = 0; i < Model.NbSegments; i++)
		{
			int _parentIndex = ParentIndex(i);
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				_parentIndex < 0 ?
				AvatarMatrixRotation.Identity() : CurrentData.OrientationMatrix[_parentIndex].Transpose();
			CalibrationMatrices[i] = _orientationParentTransposed * CurrentData.OrientationMatrix[i];
		}
		return true;
	}
	protected virtual AvatarMatrixRotation[] ProjectWrtToCalibrationPosition()
	{
		if (!IsCalibrated)
		{
			if (!SetCalibrationPositionMatrices()) return null;
			IsCalibrated = true;
		}

		AvatarMatrixRotation[] _currentInAvatar = new AvatarMatrixRotation[Model.NbSegments];
		for (int i = 0; i < Model.NbSegments; i++)
		{
			int _parentIndex = ParentIndex(i);
			// Hips are the reference for both Left and Right arm that is why we can do this shortcut
			AvatarMatrixRotation _orientationParentTransposed =
				_parentIndex < 0 ?
				AvatarMatrixRotation.Identity() : CurrentData.OrientationMatrix[_parentIndex].Transpose();
			_currentInAvatar[i] =
				CalibrationMatrices[i].Transpose() * _orientationParentTransposed
				* CurrentData.OrientationMatrix[i];
		}
		return _currentInAvatar;
	}

	public IEnumerator InitializeController(
		SensorType _sensorType,
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
			ControllerModule = new XSensModule(Model.NbImus);
		}
		else if (_sensorType == SensorType.XSensFake)
        {
			ControllerModule = new XSensFakeModule(Model.NbImus);
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

	protected void GetCurrentData()
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

	void OnDestroy()
    {
		if (ControllerModule != null) ControllerModule.Disconnect();
	}


	protected void ApplyRotation(GameObject segment, double[] target)
	{
		Vector3 targetVector = new Vector3((float)target[0], (float)target[1], (float)target[2]);
		if (segment.transform.localEulerAngles != targetVector)
        {
			segment.transform.localEulerAngles = targetVector;
		}
	}
}
