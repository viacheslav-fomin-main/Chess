using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChessInput : MonoBehaviour {

	public LayerMask pieceMask;
	Camera viewCamera;

	PieceUI pieceHeld;
	bool holdingPiece;

	List<HumanPlayer> players = new List<HumanPlayer>();
	bool active;


	void Start() {
		viewCamera = Camera.main;
		FindObjectOfType<MoveManager>().OnGameOver += HandleOnGameOver;
	}

	void HandleOnGameOver (int result, Definitions.ResultType type)
	{
		active = false;
	}

	public void AddPlayer(HumanPlayer player) {
		players.Add (player);
		active = true;
	}

	void Update() {
		if (!active) { // input is active if one or more players have been assigned to it (or game is over)
			return;
		}

		Vector2 mousePosition = (Vector2)viewCamera.ScreenToWorldPoint (Input.mousePosition);

		// Pick up piece
		if (Input.GetKeyDown(KeyCode.Mouse0) && !holdingPiece) {
			bool isPlayerMove = false;
			for (int i =0; i < players.Count; i ++) {
				if (players[i].isMyMove) {
					isPlayerMove = true;
					break;
				}
			}

			holdingPiece = TryGetPieceUIAtPoint(mousePosition, out pieceHeld);
			if (holdingPiece && isPlayerMove) {
				// highlight legal moves for held piece
				Heap legalMoveHeap = HumanPlayer.legalMoves;
				for (int i =0; i < legalMoveHeap.Count; i ++) {
					HighlightSquare(legalMoveHeap.GetMove(i), pieceHeld.algebraicCoordinate);

				}

			}
		}
		// Let go of piece
		else if (Input.GetKeyUp (KeyCode.Mouse0) && holdingPiece) {
			PieceUI dropSquare;
			ChessUI.instance.ResetHighlights();
			if (TryGetPieceUIAtPoint(mousePosition, out dropSquare)) {
				string algebraicMove = pieceHeld.algebraicCoordinate + dropSquare.algebraicCoordinate;
				for (int i = 0; i < players.Count; i ++) {
					players[i].TryMakeMove(algebraicMove);
				}
			}


			pieceHeld.Release();
			holdingPiece = false;
		}
		// Drag piece
		else if (Input.GetKey (KeyCode.Mouse0) && holdingPiece) {
			pieceHeld.Move(mousePosition);
		}
	}

	void HighlightSquare(ushort move, string pieceAlgebraic) {
		int moveFromIndex = move & 127;
		int moveToIndex = (move >> 7) & 127;

		int moveFromX = Board.Convert128to64(moveFromIndex) % 8;
		int moveFromY = Board.Convert128to64(moveFromIndex) / 8;
    	int moveToX = Board.Convert128to64(moveToIndex) % 8;
  		int moveToY = Board.Convert128to64(moveToIndex) / 8;


		string fromAlgebraic = Definitions.fileNames[moveFromX].ToString() + Definitions.rankNames[moveFromY].ToString();
		string toAlgebraic = Definitions.fileNames[moveToX].ToString() + Definitions.rankNames[moveToY].ToString();

		if (fromAlgebraic == pieceHeld.algebraicCoordinate) {
			ChessUI.instance.HighlightSquare (toAlgebraic);
		}
	}
	
	bool TryGetPieceUIAtPoint(Vector2 point, out PieceUI piece) {
		Collider2D pieceCollider = Physics2D.OverlapPoint(point, pieceMask);
		if (pieceCollider != null) {
			if (pieceCollider.GetComponent<PieceUI>() != null) {
				piece = pieceCollider.GetComponent<PieceUI>();
				return true;
			}
		}
		piece = null;
		return false;
	}
	

}
