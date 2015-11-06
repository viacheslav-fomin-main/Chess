using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	public GameObject clocks;
	public GameObject pgnViewer;
	public GameObject notationInputSmall;
	public GameObject fullBlindfoldUI;
	public GameObject resultUI;
	public GameObject boardRevealCounterUI;

	bool unhideBoardUIEveryXMoves;
	int movesRemainingToBoardReveal;
	int movesBetweenBoardReveals;
	bool resetUINextMove;

	void Start() {
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMoveMade;
		FindObjectOfType<MoveManager> ().OnGameOver += OnGameOver;
		Reset ();

	}

	[ContextMenu("Set UI")]
	void Initialize() {
		ChessUI.instance.CreateBoardUI ();
		Board.SetPositionFromFen (Definitions.startFen);

		Reset ();
	}

	void Reset() {
		HideAllUIElements ();

		switch (GameManager.instance.gameMode) {
		case GameManager.GameMode.Regular:
			SetVisibilityUIElements(notationInputSmall);
			SetVisibilityUIElements(true, clocks, pgnViewer);
			break;
		case GameManager.GameMode.BlindfoldWithBoard01:
			ChessUI.instance.SetPieceVisiblity(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 5;
			SetVisibilityUIElements(true, pgnViewer, boardRevealCounterUI);

			break;
		case GameManager.GameMode.BlindfoldWithBoard02:
			ChessUI.instance.SetPieceVisiblity(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 10;
			ChessUI.instance.SetHightlightLegalMoves(false);
			SetVisibilityUIElements(true, pgnViewer, boardRevealCounterUI);
			break;
		case GameManager.GameMode.BlindfoldSansBoard01:
			ChessUI.instance.SetBoardVisibility(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 10;
			ChessUI.instance.SetHightlightLegalMoves(false);
			SetVisibilityUIElements(true, pgnViewer, notationInputSmall, boardRevealCounterUI);
			break;
		case GameManager.GameMode.BlindfoldSansBoard02:
			ChessUI.instance.SetBoardVisibility(false);
			SetVisibilityUIElements(clocks, pgnViewer, notationInputSmall);
			SetVisibilityUIElements(true, fullBlindfoldUI);
			break;
		}
		
		if (unhideBoardUIEveryXMoves) {
			movesRemainingToBoardReveal = movesBetweenBoardReveals;
			boardRevealCounterUI.GetComponent<Text>().text = "showing board in " + movesRemainingToBoardReveal + " move" + ((movesRemainingToBoardReveal>1)?"s":"");
		}
	}

	void HideAllUIElements() {
		SetVisibilityUIElements (false, clocks, pgnViewer, notationInputSmall, fullBlindfoldUI, resultUI, boardRevealCounterUI);
	}

	void SetVisibilityUIElements(bool visibility, params GameObject[] objects) {
		for (int i =0; i <objects.Length; i ++) {
			objects[i].SetActive(visibility);
		}
	}


	void OnMoveMade(bool white, ushort move) {
		if (unhideBoardUIEveryXMoves && !white) {
			movesRemainingToBoardReveal --;
			boardRevealCounterUI.GetComponent<Text>().text = "showing board in " + movesRemainingToBoardReveal + " move" + ((movesRemainingToBoardReveal>1)?"s":"");
			if (movesRemainingToBoardReveal == 0) {
				boardRevealCounterUI.GetComponent<Text>().text = "showing board";
				movesRemainingToBoardReveal = movesBetweenBoardReveals;
				ChessUI.instance.SetBoardVisibility(true);
				ChessUI.instance.SetPieceVisiblity(true);
				resetUINextMove = true;
			}
		}

		if (unhideBoardUIEveryXMoves && white && resetUINextMove) {
			resetUINextMove = false;
			Reset();
		}
	}

	void OnGameOver(int result) {
		SetVisibilityUIElements (false, notationInputSmall, clocks);
		SetVisibilityUIElements (true, resultUI);
		string resultString = "Game over";
		if (result == -1) {
			resultString = "Black wins";
		} else if (result == 0) {
			resultString = "Game drawn";
		}
		else if (result == 1) {
			resultString = "White wins";
		}
		resultUI.GetComponent<Text> ().text = resultString;
	}

}
