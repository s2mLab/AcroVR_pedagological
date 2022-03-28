using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// Biorbd related variables
	public BiorbdModel Model { get; protected set; }
	public abstract void SetSegmentsRotations(double[] q);


    // XSens related variables
    public XSensModule Module { get; protected set; }
    protected XSensData CurrentData = null;
	protected bool CurrentDataHasChanged = false;
	protected int PreviousDataIndex = -1;

	protected abstract string BiomodPath();

	protected void Start()
    {
		Model = new BiorbdModel(BiomodPath());

		Module = new XSensModule();
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


	protected void PerformRotation(GameObject segment, Vector3 target)
	{
		if (segment.transform.localEulerAngles != target)
        {
			segment.transform.localEulerAngles = target;
		}
	}
}
