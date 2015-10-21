using System.Collections;
using System;

/*
 * Base class for human and AI players 
 */

public class Player {
	
	public MoveGenerator moveGenerator;
	public Action OnMove;

	protected bool isWhite;

	public virtual void Init(bool white) {

		isWhite = white;
		moveGenerator = new MoveGenerator ();
	}


	protected virtual void MakeMove(ushort move) {
		Board.MakeMove (move,true);
		if (OnMove != null) {
			OnMove();
		}
	}

	public virtual void RequestMove() {

	}

	public virtual void Update() {
	}

	public bool isMyMove {
		get {
			return isWhite == Board.IsWhiteToPlay ();
		}
	}

}
