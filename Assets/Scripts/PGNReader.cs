using System.Collections.Generic;
using System.Collections;

public class PGNReader {

	/// Returns a list of moves given a pgn string.
	/// Note that Board should be set to whatever starting position of pgn is.
	public static List<ushort> MovesFromPGN(string pgn) {
		List<string> moveStrings = MoveStringsFromPGN (pgn);
		List<ushort> allMoves = new List<ushort> ();

		MoveGenerator moveGen = new MoveGenerator ();

		for (int i =0; i < moveStrings.Count; i++) {
			string moveString = moveStrings[i];

			moveString = moveString.Replace("+",""); // remove check symbol
			moveString = moveString.Replace("#",""); // remove mate symbol
			moveString = moveString.Replace("x",""); // remove capture symbol
			string moveStringLower = moveStrings[i].ToLower();

			ushort[] movesInPosition = moveGen.GetMoves(false,false).moves;
			ushort move = 0;
			for (int j =0; j < movesInPosition.Length; j ++) {
				move = movesInPosition[j];
				int moveFromIndex = move & 127;
				int moveToIndex = (move >> 7) & 127;
				int movePieceType = Board.boardArray[moveFromIndex] & ~1;
				int colourCode = Board.boardArray[moveFromIndex] & 1;


				if (moveStringLower == "oo") { // castle kingside
					if (movePieceType == Board.kingCode && moveToIndex - moveFromIndex == 2) {
						break;
					}
				}
				else if (moveStringLower == "ooo") { // castle queenside
					if (movePieceType == Board.kingCode && moveToIndex - moveFromIndex == -2) {
						break;
					}
				}
				else if (Definitions.fileNames.Contains(moveString[0] + "")) { // pawn move if starts with any file indicator (e.g. 'e'4. Note that uppercase B is used for bishops)
					if (movePieceType != Board.pawnCode) {
						continue;
					}
					if (Definitions.FileNumberFromAlgebraicName(moveStringLower[0]) == Board.FileFrom128(moveFromIndex)) { // correct starting file
						if (moveString.Contains("=")) { // is promotion
							char promotionChar = moveStringLower[moveStringLower.Length-1];

							int promotionPieceIndex = move >> 14 & 3;
							int promotionPieceCode = Board.pieceCodeArray [promotionPieceIndex];

							if ((promotionPieceCode == Board.queenCode && promotionChar != 'q') || (promotionPieceCode == Board.rookCode && promotionChar != 'r')
							    || (promotionPieceCode == Board.bishopCode && promotionChar != 'b') || (promotionPieceCode == Board.knightCode && promotionChar != 'n')) {
								continue; // skip this move, incorrect promotion type
							}
							break;
						}
						else {
					
							char targetFile = moveString[moveString.Length-2];
							char targetRank = moveString[moveString.Length-1];

							if (Definitions.FileNumberFromAlgebraicName(targetFile) == Board.FileFrom128(moveToIndex)) { // correct ending file
								if (Definitions.RankNumberFromAlgebraicName(targetRank) == Board.RankFrom128(moveToIndex)) { // correct ending rank
									break;
								}
							}
						}
					}
				}
				else { // regular piece move
				
					char movePieceChar = moveString[0];
					if (!(movePieceType == Board.queenCode && movePieceChar == 'Q') && !(movePieceType == Board.rookCode && movePieceChar == 'R')
					    && !(movePieceType == Board.bishopCode && movePieceChar == 'B') && !(movePieceType == Board.knightCode && movePieceChar == 'N') && !(movePieceType == Board.kingCode && movePieceChar == 'K')) {
						continue; // skip this move, incorrect move piece type
					}

					char targetFile = moveString[moveString.Length-2];
					char targetRank = moveString[moveString.Length-1];
					if (Definitions.FileNumberFromAlgebraicName(targetFile) == Board.FileFrom128(moveToIndex)) { // correct ending file
						if (Definitions.RankNumberFromAlgebraicName(targetRank) == Board.RankFrom128(moveToIndex)) { // correct ending rank
							if (moveString.Length == 4) { // addition char present for disambiguation (e.g. Nbd7 or R7e2)
								char disambiguationChar = moveString[1];

								if (Definitions.fileNames.Contains(disambiguationChar + "")) { // is file disambiguation
									if (Definitions.FileNumberFromAlgebraicName(disambiguationChar) != Board.FileFrom128(moveFromIndex)) { // incorrect starting file
										continue;
									}
								}
								else { // is rank disambiguation
									if (Definitions.RankNumberFromAlgebraicName(disambiguationChar) != Board.RankFrom128(moveFromIndex)) { // incorrect starting rank
										continue;
									}

								}
							}
							break;
						}
					}
				}

			}
			if (move == 0) { // move is illegal; discard and return moves up to this point
				UnityEngine.Debug.Log(moveString);
				break;
			}
			else {
				allMoves.Add(move);
			}
			Board.MakeMove(move);
		}
		for (int i = allMoves.Count-1; i>= 0; i --) {
			Board.UnmakeMove(allMoves[i]);
		}

		return allMoves;
	}

	/// Returns a list containing individual move strings as extracted from supplied pgn string
	public static List<string> MoveStringsFromPGN(string pgn) {

		List<string> allMoveStrings = new List<string> ();

		bool readingComment = false;
		bool readingMove = false;

		string moveStartChars = "abcdefghrnbkqo";
		string moveChars = "12345678abcdefghrnbkqo=x+#";
		string currentMoveString = "";

		for (int i =0; i < pgn.Length; i++) {
			string currentCharStringLower = pgn.ToLower()[i] + "";
			string currentCharString = pgn[i] + "";
			if (currentCharStringLower == "[" || currentCharStringLower == "{") {
				readingComment = true;
			}
			else if (currentCharStringLower == "]" ||currentCharStringLower == "}") {
				readingComment = false;
			}

			if (!readingComment) {
				if (readingMove) {
					if (currentCharStringLower == " ") { // space between moves
						allMoveStrings.Add(currentMoveString);
						currentMoveString = "";
						readingMove = false;
					}
					else if (moveChars.Contains(currentCharStringLower)) {
						currentMoveString += currentCharString;
					}
				}
				else if (moveStartChars.Contains(currentCharStringLower)) {
					i--; // return to last char to begin reading at move start next iteration
					readingMove = true;
				}

			}
		}

		if (readingMove) {
			allMoveStrings.Add(currentMoveString);
		}

	
		return allMoveStrings;
	}

	public static string NotationFromMove(ushort move) {
		Board.UnmakeMove (move); // unmake move on board

		MoveGenerator moveGen = new MoveGenerator ();
		int moveFromIndex = move & 127;
		int moveToIndex = (move >> 7) & 127;
		int promotionPieceIndex = (move >> 14) & 3; // 0 = queen, 1 = rook, 2 = knight, 3 = bishop
		int colourToMove = Board.boardColourArray[moveFromIndex];
		
		int movePieceCode = Board.boardArray [moveFromIndex]; // get move piece code
		int movePieceType = movePieceCode & ~1; // get move piece type code (no colour info)
		int capturedPieceCode = Board.boardArray [moveToIndex]; // get capture piece code
		
		int promotionPieceType = Board.pieceCodeArray [promotionPieceIndex];
		
		if (movePieceType == Board.kingCode) {
			if (moveToIndex - moveFromIndex == 2) {
				Board.MakeMove (move); // remake move
				return "O-O";
			}
			else if (moveToIndex - moveFromIndex == -2) {
				Board.MakeMove (move); // remake move
				return "O-O-O";
			}
		}
		
		string moveNotation = GetSymbolFromPieceType(movePieceType);
		
		// check if any ambiguity exists in notation (e.g if e2 can be reached via Nfe2 and Nbe2)
		if (movePieceType != Board.pawnCode && movePieceType != Board.kingCode) {
			Heap allMoves = moveGen.GetMoves(false, false);
			
			for (int i =0; i < allMoves.Count; i ++) {
				int alternateMoveFromIndex = allMoves.moves[i] & 127;
				int alternateMoveToIndex = ( allMoves.moves[i]  >> 7) & 127;
				int alternateMovePieceCode = Board.boardArray [alternateMoveFromIndex];
				
				if (alternateMoveFromIndex != moveFromIndex && alternateMoveToIndex == moveToIndex) { // if moving to same square from different square
					if (alternateMovePieceCode == movePieceCode) { // same piece type
						int fromFileIndex = Board.FileFrom128(moveFromIndex) -1;
						int alternateFromFileIndex = Board.FileFrom128(alternateMoveFromIndex) -1;
						int fromRankIndex = Board.RankFrom128(moveFromIndex) -1;
						int alternateFromRankIndex = Board.RankFrom128(alternateMoveFromIndex) -1;
						
						if (fromFileIndex != alternateFromFileIndex) { // pieces on different files, thus ambiguity can be resolved by specifying file
							moveNotation += Definitions.fileNames[fromFileIndex];
							break; // ambiguity resolved
						}
						else if (fromRankIndex != alternateFromRankIndex) {
							moveNotation += Definitions.rankNames[fromRankIndex];
							break; // ambiguity resolved
						}
					}
				}
				
			}
		}
		
		if (capturedPieceCode != 0) { // add 'x' to indicate capture
			if (movePieceType == Board.pawnCode) {
				moveNotation += Definitions.fileNames[Board.FileFrom128(moveFromIndex)-1];
			}
			moveNotation += "x";
		} else { // check if capturing ep
			if (movePieceType == Board.pawnCode) {
				if (System.Math.Abs (moveToIndex - moveFromIndex) != 16 && System.Math.Abs (moveToIndex - moveFromIndex) != 32) {
					moveNotation += Definitions.fileNames[Board.FileFrom128(moveFromIndex)-1] + "x";
				}
			}
		}
		
		moveNotation += Definitions.fileNames [Board.FileFrom128 (moveToIndex) - 1];
		moveNotation += Definitions.rankNames [Board.RankFrom128 (moveToIndex) - 1];
		
		// add = piece type if promotion
		if (movePieceType == Board.pawnCode) {
			if (moveToIndex >= 112 || moveToIndex <= 7) { // pawn has reached first/eighth rank
				moveNotation += "=" + GetSymbolFromPieceType(promotionPieceType);
			}
		}
		
		// add check/mate symbol if applicable
		Board.MakeMove (move); // remake move
		if (moveGen.PositionIsMate ()) {
			moveNotation += "#";
		} else if (moveGen.PositionIsCheck ()) {
			moveNotation += "+";
		}
		return moveNotation;
	}
	
	static string GetSymbolFromPieceType(int pieceType) {
		switch (pieceType) {
		case Board.rookCode:
			return "R";
		case Board.knightCode:
			return "N";
		case Board.bishopCode:
			return "B";
		case Board.queenCode:
			return "Q";
		case Board.kingCode:
			return "K";
		default:
			return "";
		}
	}
}
