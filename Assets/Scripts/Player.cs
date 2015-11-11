using System.Collections;
using System;

/*
 * Base class for human and AI players 
 */

public class Player {
	
	public MoveGenerator moveGenerator;
	public Action<ushort> OnMove;

	protected bool isWhite;
	bool deactivated;

	public virtual void Init(bool white) {

		isWhite = white;
		moveGenerator = new MoveGenerator ();
	}


	protected virtual void MakeMove(ushort move) {

		if (move != 0 && !deactivated) {
			//Board.MakeMove (move, true);
			if (OnMove != null) {
				OnMove (move);
			}
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

	public void Deactivate() {
		deactivated = true;
	}

}
