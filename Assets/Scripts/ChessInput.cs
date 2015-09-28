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
	}

	public void AddPlayer(HumanPlayer player) {
		players.Add (player);
		active = true;
	}

	void Update() {
		if (!active) { // input is active if one or more players have been assigned to it
			return;
		}

		Vector2 mousePosition = (Vector2)viewCamera.ScreenToWorldPoint (Input.mousePosition);

		// Pick up piece
		if (Input.GetKeyDown(KeyCode.Mouse0) && !holdingPiece) {
			holdingPiece = TryGetPieceUIAtPoint(mousePosition, out pieceHeld);
			if (holdingPiece) {

				// highlight legal moves for held piece
				MoveOld[] legalMoves = HumanPlayer.legalMovesInPosition;
				for (int i =0; i < legalMoves.Length; i ++) {
					if (legalMoves[i].from.algebraic == pieceHeld.algebraicCoordinate) {
						ChessUI.instance.HighlightSquare(legalMoves[i].to.algebraic);
					}
				}
			}
		}
		// Let go of piece
		else if (Input.GetKeyUp (KeyCode.Mouse0) && holdingPiece) {
			PieceUI dropSquare;
			if (TryGetPieceUIAtPoint(mousePosition, out dropSquare)) {
				string algebraicMove = pieceHeld.algebraicCoordinate + dropSquare.algebraicCoordinate;
				for (int i = 0; i < players.Count; i ++) {
					players[i].TryMakeMove(algebraicMove);
				}
			}

			ChessUI.instance.ResetHighlights();
			pieceHeld.Release();
			holdingPiece = false;
		}
		// Drag piece
		else if (Input.GetKey (KeyCode.Mouse0) && holdingPiece) {
			pieceHeld.Move(mousePosition);
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
