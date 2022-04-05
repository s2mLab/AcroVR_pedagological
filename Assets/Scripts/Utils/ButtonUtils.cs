using UnityEngine;
using UnityEngine.UI;

public class ButtonUtils
{
	static public void DisableButton(Button _buttonToDisable)
	{
		_buttonToDisable.interactable = false;
		_buttonToDisable.GetComponent<Image>().color = Color.gray;
	}
	static public void EnableButton(Button _buttonToEnable)
	{
		_buttonToEnable.interactable = true;
		_buttonToEnable.GetComponent<Image>().color = Color.white;
	}
}
