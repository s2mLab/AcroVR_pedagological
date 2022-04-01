using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// Biorbd related variables
	public BiorbdModel Model { get; protected set; }

	protected AvatarMatrixRotation[] AvatarOffset;
	protected AvatarMatrixRotation[] CalibrationMatrices_CalibInParent;
	protected bool IsZeroSet = false;

	// XSens related variables
	public XSensModule Module { get; protected set; }
    protected XSensData CurrentData = null;
	protected bool CurrentDataHasChanged = false;
	protected int PreviousDataIndex = -1;

	protected abstract string BiomodPath();

	protected void Start()
	{
		Module = new XSensModule();
		Model = new BiorbdModel(BiomodPath());
		CalibrationMatrices_CalibInParent = new AvatarMatrixRotation[Model.NbSegments];
		AvatarOffset = GetOrientationFromAvatar();
	}

	void Update()
	{
		GetCurrentData();
		if (CurrentData == null || !CurrentData.AllSensorsSet)
		{
			return;
		}

		AvatarMatrixRotation[] _data = ProjectWrtToCalibrationPosition();
		if (_data is null)
		{
			return;
		}

		SetSegmentsRotations(_data);
	}

	public abstract void SetSegmentsRotations(AvatarMatrixRotation[] _data);
	protected abstract AvatarMatrixRotation[] GetOrientationFromAvatar();
	protected abstract bool SetCalibrationPositionMatrices();
	protected abstract AvatarMatrixRotation[] ProjectWrtToCalibrationPosition();

	public IEnumerator InitializeXSens(
		Action<int, int>  UpdateConnectingStatusCallback,
		Action ConectingIsCompletedCallback,
		Action InitializationFailedCallback
	)
    {
		if (!Module.IsStationInitialized)
        {
			if (!Module.InitializeStationsAndDevice())
			{
				InitializationFailedCallback();
				yield break;
			}
		}

        while (!Module.IsSensorsConnected)
        {
            Module.SetupSensors(Model.NbImus);
            UpdateConnectingStatusCallback(
                Module.NbSensors, Model.NbImus);
            yield return 0;
        }

        Module.FinalizeSetup();
        ConectingIsCompletedCallback();
		
		yield return 0;
	}

	protected void GetCurrentData()
	{
		CurrentDataHasChanged = false;
		if (Module.IsSensorsConnected)
		{
			if (CurrentData == null || CurrentData.TimeIndex != Module.CurrentData.TimeIndex && Module.CurrentData.AllSensorsSet)
            {
				CurrentData = Module.CurrentData;
				CurrentDataHasChanged = true;
			}
		}
	}

	void OnDestroy()
    {
        Module.Disconnect();
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
