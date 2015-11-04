using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlindfoldMode : MonoBehaviour {

	public static int mode;
	public Text moveInputWhite;
	public Text moveInputBlack;

	NotationInput input;

	void Start() {
		input = FindObjectOfType<NotationInput> ();
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMoveMade;

		moveInputWhite.text = "";
		moveInputBlack.text = "";
		input.SetInputUI (moveInputWhite);
	}

	void OnMoveMade(bool whiteMoved, ushort move) {
		if (whiteMoved) {
			moveInputBlack.text = "...";
			input.Freeze();
			moveInputWhite.text = PGNReader.NotationFromMove(move);
		} else {
			input.UnFreeze();
			input.Clear();
			moveInputBlack.text = PGNReader.NotationFromMove(move);
		}

	}

}
