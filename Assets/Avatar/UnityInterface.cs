using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnityInterface : MonoBehaviour
{
    public GameObject PanelConnexion;
    public AvatarManager Avatar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

		PanelConnexion.SetActive(true);
		Text textToUpdate = PanelConnexion.GetComponentInChildren<Text>();

        //XsDevicePtrArray deviceIds;
        string stringToUse = textToUpdate.text;
        //textToUpdate.text = string.Format(stringToUse, deviceIds.size(), nIMUtoConnect);


		PanelConnexion.SetActive(false);
	}
}
