using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameReviewer : MonoBehaviour {

	List<ushort> moveList = new List<ushort>();

	void Start() {
		FindObjectOfType<MoveManager> ().OnMoveMade += OnMove;
	}

	void OnMove(bool white, ushort move) {
		moveList.Add (move);
	}

	public void Next() {

	}

	public void Previous() {
		
	}

	public void First() {

	}

	public void Last() {

	}
}
