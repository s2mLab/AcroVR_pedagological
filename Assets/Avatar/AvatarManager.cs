using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// Biorbd related variables
	public BiorbdModel Model { get; protected set; }
	public abstract void SetSegmentsRotations(AvatarMatrixRotation[] _data);

	protected AvatarMatrixRotation[] AvatarOffset;
	protected AvatarMatrixRotation[] CalibrationPositionMatrices;
	bool IsZeroSet = false;

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
		CalibrationPositionMatrices = new AvatarMatrixRotation[Model.NbSegments];
	}

	virtual protected bool SetCalibrationPositionMatrices()
	{
		if (!CurrentData.AllSensorsSet)
        {
			return false;
        }

		// The first Sensor is the reference sensor to which all the others report wrt
		CalibrationPositionMatrices[0] = new AvatarMatrixRotation(CurrentData.OrientationMatrix[0]);
        AvatarMatrixRotation ReferenceTransposed = CalibrationPositionMatrices[0].Transpose();
        for (int i = 1; i < Model.NbSegments; i++)
        {
			CalibrationPositionMatrices[i] = ReferenceTransposed * CurrentData.OrientationMatrix[i];
        }
        return true;
	}
	virtual protected AvatarMatrixRotation[] ProjectWrtToCalibrationPosition()
	{
		if (!IsZeroSet)
		{
			if (!SetCalibrationPositionMatrices())
			{
				return null;
			}
			IsZeroSet = true;
		}

		AvatarMatrixRotation[] output = new AvatarMatrixRotation[Model.NbSegments];
		output[0] = CalibrationPositionMatrices[0].Transpose() * CurrentData.OrientationMatrix[0];
		//AvatarMatrixRotation ReferenceTransposed = ZeroPositionMatrices[0].Transpose();
		for (int i = 1; i < Model.NbSegments; i++)
		{
			//output[i] = ReferenceTransposed * ZeroPositionMatrices[i] * _sensors.OrientationMatrix[i];
			AvatarMatrixRotation Ref = new AvatarMatrixRotation(CurrentData.OrientationMatrix[0]);
			output[i] = Ref.Transpose() * CurrentData.OrientationMatrix[i];
		}
		return output;
	}

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
