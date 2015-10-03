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
		UnityEngine.Debug.Log ("Making move: I am white - " + isWhite);
		Board.MakeMove (move,true);
		if (OnMove != null) {
			OnMove();
		}
	}

	public virtual void RequestMove() {

	}

	public virtual void Update() {
	}


}
