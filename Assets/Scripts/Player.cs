using System.Collections;
using System;

/*
 * Base class for human and AI players 
 */

public class Player {

	public static Position currentPosition;
	public IMoveGenerator moveGenerator;
	public Action OnMove;

	protected bool isWhite;

	public virtual void Init(bool white) {
		currentPosition = new Position ();
		currentPosition.SetPositionFromFen (Definitions.startFen);
		moveGenerator = new MoveGenerator3 ();

		isWhite = white;
	}


	protected virtual void MakeMove(Move move) {
		ChessUI.instance.MakeMove (move.algebraicMove);
		currentPosition.MakeMove (move);
		if (OnMove != null) {
			OnMove();
		}
	}

	public virtual void RequestMove() {

	}



}
