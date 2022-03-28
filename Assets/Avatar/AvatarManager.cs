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
    protected double[] _data;

	protected abstract string BiomodPath();

	protected void Start()
    {
		Model = new BiorbdModel(BiomodPath());

		Module = new XSensModule();
		_data = new double[Model.NbQ];
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

	protected void FillData()
    {
        //double[] data = Module.CurrentData.EulerToDoubleArray();
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
