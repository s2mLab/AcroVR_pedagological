using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XSensUnityGUI : MonoBehaviour
{
    AvatarManager Avatar;

    // Unity related variables
    public GameObject PanelConnexion;
	public Text TextConnection;
	String InitialConnectingText; 

	void Start()
    {
		Avatar = GetComponent<AvatarManager>();
		InitialConnectingText = TextConnection.text;

	}

	public void ClickInitializeXSens()
	{
		PanelConnexion.SetActive(true);
		StartCoroutine(
			Avatar.InitializeXSens(
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
		PanelConnexion.SetActive(false);
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
