using System.Collections;
using System;

/*
 * Base class for human and AI players 
 */

public class Player {
	
	public IMoveGenerator moveGenerator;
	public Action OnMove;

	protected bool isWhite;

	public virtual void Init(bool white) {

		isWhite = white;
	}


	protected virtual void MakeMove() {
		//ChessUI.instance.MakeMove (move.algebraicMove);
		//currentPosition.MakeMove (move);
		if (OnMove != null) {
			OnMove();
		}
	}

	public virtual void RequestMove() {

	}

	public virtual void Update() {
	}


}
