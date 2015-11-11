using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	[Multiline]
	public string[] infos;
	public Text infoUI;

	public GameObject mainMenu;
	public GameObject aboutScreen;
	public GameObject optionsScreen;

	static bool initialScreenSizeSet;

	void Start() {
		infoUI.text = "";

		if (!initialScreenSizeSet) {
			initialScreenSizeSet = true;
			SetScreenSize (900);
		}
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

	public void ShowOptionScreen(bool show) {
		optionsScreen.SetActive(show);
		mainMenu.SetActive(!show);
	}

	public void SetScreenSize(int x) {
		float ratio = 2 / 3f;
		int y = (int)(x * ratio);

		Screen.SetResolution (x, y, false);
	}
}
