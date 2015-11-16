using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PGNDisplay : MonoBehaviour {

	public Text moveNumberUI;
	public Text moveNotationWhiteUI;
	public Text moveNotationBlackUI;
	public RectTransform contentBounds;
	public Scrollbar scroller;
	bool userControllingScrollbar;
	bool relinquishUserScrollbarControlNextMove;
	static string fullGamePGN;

	void Start() {
		fullGamePGN = "";
		moveNumberUI.text = "";
		moveNotationWhiteUI.text = "";
		moveNotationBlackUI.text = "";
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMove;
	}


	void OnMove(bool whiteMoved, ushort move) {

		if (relinquishUserScrollbarControlNextMove) {
			relinquishUserScrollbarControlNextMove = false;
			userControllingScrollbar = false;
		}

		if (whiteMoved) {
			fullGamePGN += (Board.GetFullMoveCount () + 1) + ". " +  PGNReader.NotationFromMove (move);
			moveNumberUI.text += (Board.GetFullMoveCount () + 1) + ". \n";
			moveNotationWhiteUI.text += PGNReader.NotationFromMove (move) + "\n";
		} else {
			fullGamePGN += " "  + PGNReader.NotationFromMove (move) + " ";
			moveNotationBlackUI.text += PGNReader.NotationFromMove (move) + "\n";

			if (Board.GetFullMoveCount () > 14) {
				int size = -30 * (Board.GetFullMoveCount ()-14);
				contentBounds.offsetMin = new Vector2 (contentBounds.offsetMin.x, size);
				contentBounds.offsetMax = new Vector2 (contentBounds.offsetMax.x, 0);
			}
		}
	}

	void LateUpdate() {
		if (!userControllingScrollbar) {
			scroller.value = 0;
		}
	}

	public void OnUseScrollbar() {
		relinquishUserScrollbarControlNextMove = false;
		userControllingScrollbar = true;
	}

	public void OnStopUsingScrollbar() {
		relinquishUserScrollbarControlNextMove = true;
	}

	public static string GetGamePGN() {
		return fullGamePGN;
	}


}
