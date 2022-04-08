using System;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUnityGUI : MonoBehaviour
{
    AvatarManager Avatar;

	// Unity related variables
	[SerializeField] protected GameObject XSensPanelConnexion;
	[SerializeField] protected Button InitalizeButton;
	[SerializeField] protected Button CalibrateButton;
	[SerializeField] protected Text TextConnection;
	String InitialConnectingText; 

	void Start()
    {
		Avatar = GetComponent<AvatarManager>();
		InitialConnectingText = TextConnection.text;

		ButtonUtils.EnableButton(InitalizeButton);
		ButtonUtils.DisableButton(CalibrateButton);
	}

	public void ClickInitializeModule(AvatarSensorType _sensorType)
	{
		if (_sensorType.type == SensorType.XSens)
		{
			XSensPanelConnexion.SetActive(true);
		}
		else if (_sensorType.type == SensorType.XSensFake)
		{
			XSensPanelConnexion.SetActive(true);
		}
		else
        {
			Debug.Log("Wrong type of module. Nothing is done.");
			return;
        }

		// Do not allow for reconnecting
		ButtonUtils.DisableButton(InitalizeButton);
		ButtonUtils.DisableButton(CalibrateButton);

		StartCoroutine(
			Avatar.InitializeController(
				_sensorType.type,
				Avatar.FilterKinematicsData,
				IsConnectingCallback,
				IsReadyCallback,
				ConnectionFailed
			)
		);
		return;
	}
	public void ClickCalibrate()
	{
		Avatar.SetCalibrationPositionMatrices();
	}

		public void IsReadyCallback()
	{
		Debug.Log("System connected and ready");
		XSensPanelConnexion.SetActive(false);

		ButtonUtils.DisableButton(InitalizeButton);
		ButtonUtils.EnableButton(CalibrateButton);
	}

	public void IsConnectingCallback(
		int _connected, 
		int _expecting
	)
    {
		TextConnection.text = 
			string.Format(InitialConnectingText, _connected, _expecting);
	}

	public void ConnectionFailed()
    {
		Debug.Log("Error in initializing");

		XSensPanelConnexion.SetActive(false);
		ButtonUtils.EnableButton(InitalizeButton);
		ButtonUtils.DisableButton(CalibrateButton);
	}
}
