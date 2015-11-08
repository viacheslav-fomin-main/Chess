using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _Tests : MonoBehaviour {

	void Start() {
		Debug.LogError ("Test");
		Board.SetPositionFromFen (Definitions.startFen,true);

	}

	void Update() {
		Debug.LogError ("Test");
		Board.MakeMove(new MoveGenerator().GetMoves(false,false).GetMove(0),true);
		if (Input.GetKeyDown (KeyCode.Space)) {

			MoveGenerator.Debug();
		}

	}
	

}
