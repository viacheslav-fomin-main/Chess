using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameReviewer : MonoBehaviour {

	List<ushort> moveList = new List<ushort>();
	int index;

	void Start() {
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMove;
	}

	void OnMove(bool white, ushort move) {
		moveList.Add (move);
	}

	public void Init() {
		Board.SetPositionFromFen (Definitions.startFen, true);
	}

	public void Next() {
		if (index < moveList.Count) {
			Board.MakeMove (moveList [index], true);
			index ++;
		}
	}

	public void Previous() {
		if (index > 0) {
			index --;
			Board.UnmakeMove (moveList [index], true);
		}
	}

	public void First() {
		while (index > 0) {
			index --;
			Board.UnmakeMove (moveList [index], true);
		}
	}

	public void Last() {
		while (index < moveList.Count) {
			Board.MakeMove (moveList [index], true);
			index ++;
		}
	}
}
