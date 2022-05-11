﻿using System;
using System.Collections;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// KinematicModel related variables
	public AvatarKinematicModel KinematicModel { get; protected set; }
	public abstract string BiomodPath();
	public void CalibrateKinematicModel()
	{
		ControllerModule.PlanKinematicModelCalibration();
	}

	// Model related variables
	public abstract int NbSegments();
	public abstract int NbSensors();


	// Controller related variables
	public AvatarControllerModule ControllerModule { get; protected set; } = null;
	protected AvatarCoordinates CurrentCoordinates = null;
	protected int PreviousDataIndex = -1;

	protected abstract KinematicModelInfo GetModelInfo(KinematicModelType _type);

	protected virtual void Start()
	{
	}

	protected void Update()
	{
		if (ControllerModule == null || ControllerModule.CurrentCoordinates == null) return;
		SetSegmentsRotations(ControllerModule.CurrentCoordinates);
	}

	public abstract void SetSegmentsRotations(AvatarCoordinates _data);
	
	public bool InitializeKinematicModel(KinematicModelType _model_type)
	{
		if (_model_type == KinematicModelType.Biorbd)
        {
			KinematicModel = new BiorbdKinematicModel(GetModelInfo(KinematicModelType.Biorbd));
		} else if (_model_type == KinematicModelType.Simple)
        {
			KinematicModel = new SimpleKinematicModel(GetModelInfo(KinematicModelType.Simple));
        }
        else
		{
			Debug.Log("Wrong choice of Kinematic model");
			return false;
		}

		if (!KinematicModel.IsInitialized)
		{
			Debug.Log("Could not load the kinematic model");
			return false;
		}

		return true;
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
			ControllerModule = new XSensModule(KinematicModel);
		}
		else if (_sensorType == SensorType.XSensFake)
        {
			ControllerModule = new XSensFakeModule(KinematicModel);
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
