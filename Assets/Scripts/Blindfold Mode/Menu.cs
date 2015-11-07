using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	[Multiline]
	public string[] infos;
	public Text infoUI;

	public GameObject mainMenu;
	public GameObject aboutScreen;

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
		GameManager.gameModeIndex = index;
		GameManager.gameModeIndexSet = true;
		Application.LoadLevel ("Chess");
	}

	public void ShowAboutScreen(bool show) {
		aboutScreen.SetActive(show);
		mainMenu.SetActive(!show);

	}
}
