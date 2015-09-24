using System.Collections;
using System;

/*
 * Base class for human and AI players 
 */

public class Player {

	public static Position currentPosition;
	public MoveGenerator moveGenerator;

	protected bool isWhite;

	public virtual void Init(bool white) {
		currentPosition = new Position ();
		currentPosition.SetPositionFromFen (Definitions.startFen);
		moveGenerator = new MoveGenerator ();

		isWhite = white;
	}


	protected virtual void MakeMove(Move move) {
		ChessUI.instance.MakeMove (move.algebraicMove);
		currentPosition.MakeMove (move);
	}





}
