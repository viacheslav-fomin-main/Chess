using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotationInput : MonoBehaviour {

	public Text inputUI;
	MoveGenerator moveGen = new MoveGenerator();

	string legalInputChars = "abcdefghRNBQK12345678O-x+#=";
	string currentInput;

	void Start() {
		/*
		currentInput = "e4";
		ValidateEntry ();
		currentInput = "e5";
		ValidateEntry ();
		currentInput = "Nf3";
		ValidateEntry ();
		currentInput = "Nf6";
		ValidateEntry ();
		currentInput = "Nbg1";
		ValidateEntry ();
*/
	}
	
	void Update () {
		if (Input.inputString.Length > 0) {

			char inputChar = Input.inputString [0];
			print(inputChar + "");
			if (legalInputChars.Contains (inputChar + "")) {
				currentInput += inputChar + "";
			}
		}

		// backspace
		if (Input.GetKeyDown(KeyCode.Backspace)) {
			currentInput.Remove(currentInput.Length-1);
		}

		inputUI.text = currentInput;

		// enter
		if (Input.GetKeyDown(KeyCode.Return)) {
			ValidateEntry();
		}
	}

	public void ValidateEntry() {
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
				bool moveIsLegal = false;
				bool moveIsAmbiguous = false;
				int movesFoundFollowingInput = 0;

				for (int i =0; i < legalMoves.Length; i ++) {
					if (legalMoves [i] == inputMove) {
						moveIsLegal = true;

						if (ambiguousCaseSpecified) {
							break;
						}
					}

					if (!ambiguousCaseSpecified) { // check if case is ambiguous if no specification has been given in input
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

				if (moveIsLegal) {
					if (moveIsAmbiguous) {
						print ("Move ambiguous");
					} else {
						Board.MakeMove (inputMove, true);
						print ("Move made");
					}
				} else {
					print ("Move illegal");
				}
			}
			else {
				print ("Move format rejected");
			}

		} else {
			print ("Move empty");
		}
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
