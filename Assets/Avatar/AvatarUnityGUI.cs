using System;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUnityGUI : MonoBehaviour
{
    AvatarManager Avatar;

    // Unity related variables
    public GameObject XSensPanelConnexion;
	public Text TextConnection;
	String InitialConnectingText; 

	void Start()
    {
		Avatar = GetComponent<AvatarManager>();
		InitialConnectingText = TextConnection.text;

	}

	public void ClickInitializeModule(AvatarSensorType _sensorType)
	{
		if (_sensorType.type == SensorType.XSens)
		{
			XSensPanelConnexion.SetActive(true);
		}
		else
        {
			Debug.Log("Wrong type of module. Nothing is done.");
        }

		StartCoroutine(
			Avatar.InitializeController(
				_sensorType.type,
				IsConnectingCallback,
				IsReadyCallback,
				ConnectionFailed
			)
		);
		return;
	}

	public void IsReadyCallback()
	{
		Debug.Log("System connected and ready");
		XSensPanelConnexion.SetActive(false);
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
	}
}
