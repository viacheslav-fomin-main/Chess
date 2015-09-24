using UnityEngine;
using System.Collections;

/*
 * The PieceUI script is attached to all squares of the board
 * Calling the Move method will position the piece graphic of this square at the specified position
 * Calling the Release method will reset the piece graphic back to its original position
 * In order to actually make a move on the board, the ChessUI class must be used.
 * 
 * The objects to which the PieceUI scripts are attached all have the 'Piece' layer assigned.
 * The 'Piece' layer will be disabled while being moves so as not to interfere with input.
 */

public class PieceUI : MonoBehaviour {

	public string algebraicCoordinate { get; private set; }
	Vector3 defaultPosition;

	public void Init(string _algebraicCoordinate, Vector3 position) {
		gameObject.AddComponent<BoxCollider2D> ();

		algebraicCoordinate = _algebraicCoordinate;
		defaultPosition = position;

		gameObject.layer = LayerMask.NameToLayer ("Piece");
	}

	public void Move(Vector2 newPosition) {
		gameObject.layer = 0;
		transform.position = new Vector3(newPosition.x, newPosition.y, defaultPosition.z - .1f);
	}

	public void Release() {
		gameObject.layer = LayerMask.NameToLayer ("Piece");
		transform.position = defaultPosition;
	}
	
}
