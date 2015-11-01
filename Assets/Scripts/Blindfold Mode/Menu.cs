using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	public string[] infos;
	public Text infoUI;

	void Start() {
		infoUI.text = "";
	}
	
	public void OnButtonSelected(int index) {
		infoUI.text = infos [index];
	}

	public void OnButtonDeselected() {
		infoUI.text = "";
	}

	public void OnButtonPressed(int index) {
	
	}
}
