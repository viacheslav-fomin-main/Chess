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
			chessUI.SetBoardVisibility(!chessUI.hide);
		}

		if (GUILayout.Button ("Create Board UI")) {
			chessUI.CreateBoardUI();
			chessUI.SetBoardVisibility(!chessUI.hide);
		}


	}

}
