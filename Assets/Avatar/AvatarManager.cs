using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarManager : MonoBehaviour
{
	// Biorbd related variables
	public BiorbdModel Model { get; protected set; }
	public abstract void SetSegmentsRotations(double[][] q);

	protected AvatarMatrixRotation[] ZeroMatrix;
	bool IsZeroSet = false;

	// XSens related variables
	public XSensModule Module { get; protected set; }
    protected XSensData CurrentData = null;
	protected bool CurrentDataHasChanged = false;
	protected int PreviousDataIndex = -1;

	protected abstract string BiomodPath();

	protected void Start()
    {
		///// DEBUG /////
		double[][] orig = new double[3][];
		for (int i = 0; i < orig.Length; i++)
		{
			orig[i] = new double[3];
		}
		orig[0][0] = 1;
		orig[0][1] = 2;
		orig[0][2] = 3;
		orig[1][0] = 4;
		orig[1][1] = 5;
		orig[1][2] = 6;
		orig[2][0] = 7;
		orig[2][1] = 8;
		orig[2][2] = 9;
		double[][] avatar = AvatarManager3Segments.MapToInternal(orig);
		double[][] final = AvatarManager3Segments.MapToAvatar(avatar);
		Debug.Log(
			$"{orig[0][0]}, {orig[0][1]}, {orig[0][2]}\n" +
			$"{orig[1][0]}, {orig[1][1]}, {orig[1][2]}\n" +
			$"{orig[2][0]}, {orig[2][1]}, {orig[2][2]}\n"
		);
		Debug.Log(
			$"{final[0][0]}, {final[0][1]}, {final[0][2]}\n" +
            $"{final[1][0]}, {final[1][1]}, {final[1][2]}\n" +
            $"{final[2][0]}, {final[2][1]}, {final[2][2]}\n"
		);
		/////////////////



		Model = new BiorbdModel(BiomodPath());

		Module = new XSensModule();

		ZeroMatrix = new AvatarMatrixRotation[Model.NbSegments];
	}

	public virtual bool SetZeroMatrix(XSensData _zero)
	{
		if (!_zero.AllSensorsSet)
        {
			return false;
        }

		for (int i = 0; i < Model.NbSegments; i++)
		{
			ZeroMatrix[i] = new AvatarMatrixRotation(_zero.OrientationMatrix[i]);
			ZeroMatrix[i] = ZeroMatrix[i].Transpose();
		}
		return true;
	}
	protected AvatarMatrixRotation[] ApplyZeroMatrix(XSensData _currentData)
	{
		if (!IsZeroSet)
		{
			if (!SetZeroMatrix(CurrentData))
			{
				return null;
			}
			IsZeroSet = true;
		}

		AvatarMatrixRotation[] output = new AvatarMatrixRotation[Model.NbSegments];
		for (int i = 0; i < Model.NbSegments; i++)
		{
			output[i] = ZeroMatrix[i] * _currentData.OrientationMatrix[i];
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
