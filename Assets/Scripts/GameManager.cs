using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	void Start () {
		Board.SetPositionFromFen (Definitions.startFen);
	}

}
