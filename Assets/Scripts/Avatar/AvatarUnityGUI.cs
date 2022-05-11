using System;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUnityGUI : MonoBehaviour
{
	[SerializeField] protected AvatarManager Avatar;
	[SerializeField] protected AvatarSensorType ControllerSensorType;
	[SerializeField] protected AvatarKinematicModelType KinematicType;

	// Unity related variables
	[SerializeField] protected Button InitalizeButton;
	[SerializeField] protected Button CalibrateButton;
	[SerializeField] protected GameObject XSensPanelConnexion;
	[SerializeField] protected Text TextConnection;
	String InitialConnectingText; 

	void Start()
    {
		InitialConnectingText = TextConnection.text;

		ButtonUtils.EnableButton(InitalizeButton);
		ButtonUtils.DisableButton(CalibrateButton);
	}

	public void ClickInitializeAvatar()
	{
		if (ControllerSensorType.type == SensorType.XSens)
		{
			XSensPanelConnexion.SetActive(true);
		}
		else if (ControllerSensorType.type == SensorType.XSensFake)
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

        Avatar.InitializeKinematicModel(KinematicType.type);

        StartCoroutine(
			Avatar.InitializeController(
				ControllerSensorType.type,
				IsConnectingCallback,
				IsReadyCallback,
				ConnectionFailed
			)
		);
		return;
	}
	public void ClickCalibrate()
	{
		Avatar.CalibrateKinematicModel();
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
