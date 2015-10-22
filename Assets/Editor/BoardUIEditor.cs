using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (ChessUI))]
public class BoardUIEditor : Editor {

	bool editorHasReset;

	void OnEnable() {
		EditorApplication.playmodeStateChanged += OnStateChange;
	}

	void OnStateChange() {
		if (!Application.isPlaying) {
			editorHasReset = true;
		}
	}

	public override void OnInspectorGUI() {

		ChessUI chessUI = target as ChessUI;

		if (DrawDefaultInspector ()) {
			chessUI.CreateBoardUI();
		}

		if (GUILayout.Button ("Create Board UI")) {
			chessUI.CreateBoardUI();
		}

		if (GUILayout.Button ("Set Position")) {
			chessUI.SetPosition(chessUI.editorFen);
		}
		
		if (GUILayout.Button ("Make Move")) {
			chessUI.MakeMove(chessUI.editorMove);
		}

		chessUI.editorFen = EditorGUILayout.TextField ("FEN",chessUI.editorFen);
		chessUI.editorMove = EditorGUILayout.TextField ("Move",chessUI.editorMove);


	}

}
