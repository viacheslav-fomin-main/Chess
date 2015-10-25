using System.Collections.Generic;
using System.Collections;

public class PGNReader {
	
	public static List<ushort> MovesFromPGN(string pgn) {
		List<string> moveStrings = MoveStringsFromPGN (pgn);
		List<ushort> allMoves = new List<ushort> ();

		MoveGenerator moveGen = new MoveGenerator ();
		Board.SetPositionFromFen (Definitions.startFen, true);

		for (int i =0; i < moveStrings.Count; i++) {
			string moveString = moveStrings[i];

			moveString = moveString.Replace("+",""); // remove check symbol
			moveString = moveString.Replace("#",""); // remove mate symbol
			moveString = moveString.Replace("x",""); // remove capture symbol

			List<ushort> movesInPosition = moveGen.GetMoves(false,false);
			ushort move = 0;
			for (int j =0; j < movesInPosition.Count; j ++) {
				move = movesInPosition[j];
				int moveFromIndex = move & 127;
				int moveToIndex = (move >> 7) & 127;
				int movePieceType = Board.boardArray[moveFromIndex] & ~1;
				int colourCode = Board.boardArray[moveFromIndex] & 1;


				if (moveString == "OO") { // castle kingside
					if (movePieceType == Board.kingCode && moveToIndex - moveFromIndex == 2) {
						break;
					}
				}
				else if (moveString == "OOO") { // castle queenside
					if (movePieceType == Board.kingCode && moveToIndex - moveFromIndex == -2) {
						break;
					}
				}
				else if (Definitions.fileNames.Contains(moveString[0] + "")) { // pawn move if starts with any file indicator (e.g. e4)
					if (movePieceType != Board.pawnCode) {
						continue;
					}
					if (Definitions.FileNumberFromAlgebraicName(moveString[0]) == Board.FileFrom128(moveFromIndex)) { // correct starting file
						if (moveString.Contains("=")) { // is promotion
							char promotionChar = moveString[moveString.Length-1];
							moveString = moveString.Remove(moveString.IndexOf("="), 2); // remove promotion tag from moveString

							int promotionPieceIndex = move >> 14 & 3;
							int promotionPieceCode = Board.pieceCodeArray [promotionPieceIndex];
							if (!(promotionPieceCode == Board.queenCode && promotionChar == 'Q') && !(promotionPieceCode == Board.rookCode && promotionChar == 'R')
							    && !(promotionPieceCode == Board.bishopCode && promotionChar == 'B') && !(promotionPieceCode == Board.knightCode && promotionChar == 'N')) {
								continue; // skip this move, incorrect promotion type
							}
						}
					
						char targetFile = moveString[moveString.Length-2];
						char targetRank = moveString[moveString.Length-1];

						if (Definitions.FileNumberFromAlgebraicName(targetFile) == Board.FileFrom128(moveToIndex)) { // correct ending file
							if (Definitions.RankNumberFromAlgebraicName(targetRank) == Board.RankFrom128(moveToIndex)) { // correct ending rank
								break;
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
			allMoves.Add(move);
			Board.MakeMove(move,false);
		}

		return allMoves;
	}

	public static List<string> MoveStringsFromPGN(string pgn) {

		List<string> allMoveStrings = new List<string> ();

		bool readingComment = false;
		bool readingMove = false;

		string moveStartChars = "abcdefghRNBKQO";
		string moveChars = "12345678abcdefghRNBKQO=x+#";
		string currentMoveString = "";

		for (int i =0; i < pgn.Length; i++) {
			if (pgn[i] == '[' || pgn[i] == '{') {
				readingComment = true;
			}
			else if (pgn[i] == ']' || pgn[i] == '}') {
				readingComment = false;
			}

			if (!readingComment) {
				if (readingMove) {
					if (pgn[i] == ' ') { // space between moves
						allMoveStrings.Add(currentMoveString);
						currentMoveString = "";
						readingMove = false;
					}
					else if (moveChars.Contains(pgn[i] + "")) {
						currentMoveString += pgn[i] + "";
					}
				}
				else if (moveStartChars.Contains(pgn[i] + "")) {
					i--; // return to last char to begin reading at move start next iteration
					readingMove = true;
				}

			}
		}

		return allMoveStrings;
	}
}
