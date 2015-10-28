using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _Tests : MonoBehaviour {



	void Update() {

		if (Input.GetKeyDown (KeyCode.Space)) {

			MoveGenerator.Debug();
		}

	}
	

}
