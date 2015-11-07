using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotationInput : MonoBehaviour {

	Text inputUI;
	MoveGenerator moveGen = new MoveGenerator();

	string legalInputChars = "abcdefghrnbqk12345678o-x+#=";
	string currentInput = "";

	const float timeBetweenBackspace = .15f;
	float nextBackspaceTime;
	bool inputFrozen;

	HumanPlayer player;
	UIManager uiManager;

	void Start() {
		uiManager = FindObjectOfType<UIManager> ();
	}

	public void SetPlayer(HumanPlayer p) {
		player = p;
	}

	public void SetInputUI(Text ui) {
		inputUI = ui;
	}

	public void Clear() {
		inputUI.text = "";
		currentInput = "";
	}
	
	void Update () {
		if (inputFrozen) {
			return;
		}

		if (Input.inputString.Length > 0) {
			string input = Input.inputString [0].ToString();
			if (legalInputChars.Contains (input.ToLower())) {
				if (currentInput.Length < 7) {
					if ("rnkqo".Contains(input.ToLower())) { // auto capitalise piece names (excluding bishop since could refer to b file)
						input = input.ToUpper();
					}
					else if ("acdefghx".Contains(input.ToLower())) { // auto lowercase files names (excluding b file since could refer to bishop)
						input = input.ToLower();
					}
					currentInput += input;
				}
			}
		}

		// backspace
		if (Input.GetKey(KeyCode.Backspace)) {
			if (currentInput.Length > 0 && Time.time > nextBackspaceTime) {
				nextBackspaceTime = Time.time + timeBetweenBackspace;
				currentInput = currentInput.Remove(currentInput.Length-1);
			}
		}
		if (Input.GetKeyUp (KeyCode.Backspace)) {
			nextBackspaceTime = 0;
		}

		inputUI.text = currentInput;

		// enter
		if (Input.GetKeyDown(KeyCode.Return)) {
			if (!ValidateEntry()) {
				uiManager.SetMessage("move illegal / incorrect format",2,true);
				inputUI.text = "";
				currentInput = "";
			}
		}
	}

	public bool ValidateEntry() {
		if (currentInput.Length > 0) {
			List<ushort> inputtedMoves = PGNReader.MovesFromPGN (currentInput);
			if (inputtedMoves.Count > 0) {
				string cleanedInput = PGNReader.MoveStringsFromPGN (currentInput) [0];
			
				ushort inputMove = inputtedMoves [0];
				int movePieceCode = PieceTypeCodeFromNotation (cleanedInput) + ((Board.IsWhiteToPlay ()) ? 1 : 0);
				ushort[] legalMoves = moveGen.GetMoves (false, false).moves;

				bool ambiguousCaseSpecified = false; // does the inputted string describe an ambiguous case (e.g Rad1)
				if (cleanedInput.Length >= 3) {
					ambiguousCaseSpecified = Definitions.fileNames.Contains (cleanedInput [2] + "");
				}
				if ((movePieceCode & ~1) == Board.kingCode || (movePieceCode & ~1) == Board.pawnCode) { // king and pawn moves can't be ambiguous
					ambiguousCaseSpecified = true;
				}

				bool moveIsAmbiguous = false;

				if (!ambiguousCaseSpecified) { // check if case is ambiguous if no specification has been given in input
					int movesFoundFollowingInput = 0;

					for (int i =0; i < legalMoves.Length; i ++) {
						int moveFromIndex = legalMoves [i] & 127;
						int moveToIndex = (legalMoves [i] >> 7) & 127;
						if (Board.boardArray [moveFromIndex] == movePieceCode && moveToIndex == ((inputMove >> 7) & 127)) { // is move as described by input string
							movesFoundFollowingInput ++;
							if (movesFoundFollowingInput == 2) {
								moveIsAmbiguous = true;
								break;
							}
						}
					}
				}

				if (moveIsAmbiguous) {
					uiManager.SetMessage("move is ambiguous. please specify file/rank of piece you wish to move", 3, true);
				}
				else {
					if (player != null) {
						player.TryMakeMove(inputMove);
					}
					return true;
				}
			}
			else {
				print ("Move illegal/incorrect format");
			}
		}

		return false;
	}

	public void Freeze() {
		inputFrozen = true;
	}

	public void UnFreeze() {
		inputFrozen = false;
	}

	int PieceTypeCodeFromNotation(string notation) {
		switch (notation.ToUpper()[0]) {
		case 'R':
			return Board.rookCode;
		case 'N':
			return Board.knightCode;
		case 'B':
			return Board.bishopCode;
		case 'Q':
			return Board.queenCode;
		case 'K':
			return Board.kingCode;
		case 'O':
			return Board.kingCode;
		default:
			return Board.pawnCode;
		}
	}
}
