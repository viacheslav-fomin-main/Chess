using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	public GameObject clocks;
	public GameObject pgnViewer;
	public GameObject notationInputSmall;
	public GameObject fullBlindfoldUI;
	public GameObject reviewUI;
	public Text messageUI;

	bool unhideBoardUIEveryXMoves;
	int movesRemainingToBoardReveal;
	int movesBetweenBoardReveals;
	bool resetUINextMove;

	int currentMessagePriority;
	bool currentMessageIsMoveOld;
	bool autoClearMessage;
	int messageCreatedOnMovePly;

	bool gameOver;
	bool inReviewMode;

	void Awake() {
		messageUI.text = "";
	}

	void Start() {
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMoveMade;
		FindObjectOfType<MoveManager> ().OnGameOver += OnGameOver;
		Reset ();
	}

	public void SetMessage(string message, int priority, bool autoClearOnMove) {
		if (currentMessageIsMoveOld || priority > currentMessagePriority) {
			currentMessagePriority = priority;
			currentMessageIsMoveOld = false;
			messageUI.text = message;

			autoClearMessage = autoClearOnMove;
			messageCreatedOnMovePly = Board.halfmoveCount;

		}
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
			ChessUI.instance.SetHightlightLegalMoves(true);
			ChessUI.instance.SetBoardVisibility(true);
			ChessUI.instance.SetPieceVisiblity(true);
			break;
		case GameManager.GameMode.BlindfoldWithBoard01:
			ChessUI.instance.SetPieceVisiblity(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 5;
			SetVisibilityUIElements(true, pgnViewer);

			break;
		case GameManager.GameMode.BlindfoldWithBoard02:
			ChessUI.instance.SetPieceVisiblity(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 10;
			ChessUI.instance.SetHightlightLegalMoves(false);
			SetVisibilityUIElements(true, pgnViewer);
			break;
		case GameManager.GameMode.BlindfoldSansBoard01:
			ChessUI.instance.SetBoardVisibility(false);
			unhideBoardUIEveryXMoves = true;
			movesBetweenBoardReveals = 10;
			ChessUI.instance.SetHightlightLegalMoves(false);
			SetVisibilityUIElements(true, pgnViewer, notationInputSmall);
			break;
		case GameManager.GameMode.BlindfoldSansBoard02:
			ChessUI.instance.SetBoardVisibility(false);
			SetVisibilityUIElements(true, notationInputSmall);
			//SetVisibilityUIElements(true, fullBlindfoldUI);
			break;
		}
		
		if (unhideBoardUIEveryXMoves) {
			movesRemainingToBoardReveal = movesBetweenBoardReveals;
			SetMessage("showing pieces in " + movesRemainingToBoardReveal + " move" + ((movesRemainingToBoardReveal>1)?"s":""), 0, true);
		}
	}

	void HideAllUIElements() {
		SetVisibilityUIElements (false, clocks, pgnViewer, notationInputSmall, fullBlindfoldUI, reviewUI);
	}

	void SetVisibilityUIElements(bool visibility, params GameObject[] objects) {
		for (int i =0; i <objects.Length; i ++) {
			objects[i].SetActive(visibility);
		}
	}

	public void EnterReviewMode() {
		if (gameOver) {
			if (!inReviewMode) {
				GameManager.instance.gameMode = GameManager.GameMode.Regular; // switch to regular mode for game review
				Reset();

				inReviewMode = true;
				SetVisibilityUIElements (true, reviewUI);
				SetVisibilityUIElements (false, clocks, notationInputSmall);
				FindObjectOfType<GameReviewer>().Init();
			}
		} else {
			SetMessage("Cannot review while game is in progress", 5, true);
		}
	}


	void OnMoveMade(bool white, ushort move) {
		currentMessageIsMoveOld = true;
		if (autoClearMessage) {
			if (Board.halfmoveCount > messageCreatedOnMovePly) {
				messageUI.text = "";
			}
		}

		if (unhideBoardUIEveryXMoves && !white) {
			movesRemainingToBoardReveal --;
			messageUI.GetComponent<Text>().text = "showing board in " + movesRemainingToBoardReveal + " move" + ((movesRemainingToBoardReveal>1)?"s":"");
			if (movesRemainingToBoardReveal == 0) {
				messageUI.GetComponent<Text>().text = "showing board";
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
		gameOver = true;
		SetVisibilityUIElements (false, notationInputSmall, clocks);
		string resultString = "Game over";
		if (result == -1) {
			resultString = "Black wins";
		} else if (result == 0) {
			resultString = "Game drawn";
		}
		else if (result == 1) {
			resultString = "White wins";
		}
		SetMessage (resultString, 10, false);
	}



}
