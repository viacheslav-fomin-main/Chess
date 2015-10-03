using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	PlayerManager playerManager;
	
	void Start () {
		Evaluation.Init ();

		playerManager = GetComponent<PlayerManager> ();

		Board.SetPositionFromFen (Definitions.startFen);

		playerManager.CreatePlayers ();
	}

}
