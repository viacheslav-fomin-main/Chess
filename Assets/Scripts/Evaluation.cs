using System.Collections.Generic;
using System;

public static class Evaluation {
	
	const int pawnValue = 10;
	const int rookValue = 50;
	const int knightValue = 32;
	const int bishopValue = 32;
	const int queenValue = 100;
	const int kingValue = 999;
	
	static Dictionary<int,int> pieceValues;
	static MoveGenerator moveGenerator;
	
	public static void Init() {
		pieceValues = new Dictionary<int, int> ();
		for (int i = 0; i <=1; i++) { // piece values (white positive; black negative)
			int colourCode = i;
			int sign = i*2-1;
			pieceValues.Add (Board.pawnCode + colourCode, pawnValue * sign);
			pieceValues.Add (Board.rookCode + colourCode, rookValue * sign);
			pieceValues.Add (Board.knightCode + colourCode, knightValue * sign);
			pieceValues.Add (Board.bishopCode + colourCode, bishopValue * sign);
			pieceValues.Add (Board.queenCode + colourCode, queenValue * sign);
			pieceValues.Add (Board.kingCode + colourCode, kingValue * sign);
		}

		moveGenerator = new MoveGenerator ();
	}
	
	public static int Evaluate() {
		Timer.Start ("Eval");
		int materialEval = 0;
		int mobilityEval = 0;
		int developmentEval = 0;
		int kingSafetyEval = 0;

		// piece index vars (assigned when found to be used in later calculations)
		int whiteKingSquareIndex = -1;
		int blackKingSquareIndex = -1;
		List<int> whiteQueenIndices = new List<int> (2);
		List<int> whiteRookIndices = new List<int> (2);
		List<int> whiteKnightIndices = new List<int> (2);
		List<int> whiteBishopIndices = new List<int> (2);
		List<int> whitePawnIndices = new List<int> (8);

		List<int> blackQueenIndices = new List<int> (2);
		List<int> blackRookIndices = new List<int> (2);
		List<int> blackKnightIndices = new List<int> (2);
		List<int> blackBishopIndices = new List<int> (2);
		List<int> blackPawnIndices = new List<int> (8);

		
		for (int squareIndex =0; squareIndex <= 127; squareIndex ++) {
			if ((squareIndex & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}
			
			if (Board.boardArray[squareIndex] != 0) {
				int pieceCode = Board.boardArray[squareIndex];
				if (pieceCode == Board.kingCode + 1) { // found white king
					whiteKingSquareIndex = squareIndex;
				}
				else if (pieceCode == Board.kingCode) { // found black king
					blackKingSquareIndex = squareIndex;
				}
				else { // non-king pieces
					materialEval += pieceValues[pieceCode]; // count material (excluding kings)

					switch (pieceCode) {
					case Board.queenCode +1:
						whiteQueenIndices.Add(squareIndex);
						break;
					case Board.rookCode +1:
						whiteRookIndices.Add(squareIndex);
						break;
					case Board.knightCode +1:
						whiteKnightIndices.Add(squareIndex);
						break;
					case Board.bishopCode +1:
						whiteBishopIndices.Add(squareIndex);
						break;
					case Board.pawnCode +1:
						whitePawnIndices.Add(squareIndex);
						break;

					case Board.queenCode:
						blackQueenIndices.Add(squareIndex);
						break;
					case Board.rookCode:
						blackRookIndices.Add(squareIndex);
						break;
					case Board.knightCode:
						blackKnightIndices.Add(squareIndex);
						break;
					case Board.bishopCode:
						blackBishopIndices.Add(squareIndex);
						break;
					case Board.pawnCode:
						blackPawnIndices.Add(squareIndex);
						break;
					}
				}
			}
		}

		if (whiteKingSquareIndex == -1) { // return best score for black if white's king has been captured (this may sometimes be allowed during alphabeta search for faster move generation)
			//return int.MinValue;
		}


		// piece mobility
		moveGenerator.SetMoveColour (1);
		mobilityEval += moveGenerator.GetMoves (false, true, false).Count; // white piece mobility
		mobilityEval += moveGenerator.GetMoves (true, true, false).Count; // white piece attacking black
		moveGenerator.SetMoveColour (0);
		mobilityEval -= moveGenerator.GetMoves (false, true, false).Count; // black piece mobility
		mobilityEval -= moveGenerator.GetMoves (true, true, false).Count; // black piece attacking white

		// piece development white
		for (int i = 0; i < whiteKnightIndices.Count; i ++) {
			if (Board.RankFrom128(whiteKnightIndices[i]) == 1) { // penalize knight remaining on first rank
				developmentEval -= 50;
			}
			else if (Board.RankFrom128(whiteKnightIndices[i]) == 2) { // penalize knight remaining on second rank
				developmentEval -= 10;
			}
			if (Board.FileFrom128(whiteKnightIndices[i]) == 1) { // knights on the rim are dim
				developmentEval -= 5;
			}
			else if (Board.FileFrom128(whiteKnightIndices[i]) == 8) { // knights on the rim are dim
				developmentEval -= 5;
			}
		}

		for (int i = 0; i < whiteBishopIndices.Count; i ++) {
			if (Board.RankFrom128(whiteBishopIndices[i]) == 1) { // penalize bishop remaining on first rank
				developmentEval -= 50;
			}
		}

		// piece development black
		for (int i = 0; i < blackKnightIndices.Count; i ++) {
			if (Board.RankFrom128(blackKnightIndices[i]) == 8) { // penalize knight remaining on eighth rank
				developmentEval += 50;
			}
			else if (Board.RankFrom128(blackKnightIndices[i]) == 7) { // penalize knight remaining on seventh rank
				developmentEval += 10;
			}
			if (Board.FileFrom128(blackKnightIndices[i]) == 1) { // knights on the rim are dim
				developmentEval += 5;
			}
			else if (Board.FileFrom128(blackKnightIndices[i]) == 8) { // knights on the rim are dim
				developmentEval += 5;
			}
		}
		
		for (int i = 0; i < blackBishopIndices.Count; i ++) {
			if (Board.RankFrom128(blackBishopIndices[i]) == 8) { // penalize bishop remaining on eighth rank
				developmentEval += 50;
			}
		}

		// king safety white
		if (Board.WhiteHasCastlingRights ()) {
			kingSafetyEval += 10; // not safe, but at least retaining ability to castle
		} else {
			if (whiteKingSquareIndex == 6 || whiteKingSquareIndex == 7) { // generally safe kingside squares for king (g1,h1)
				kingSafetyEval += 50;
				for (int i = 0; i < whiteRookIndices.Count; i ++) {
					if (Board.FileFrom128(whiteRookIndices[i]) > 6) {
						kingSafetyEval -= 55; // penalize non-castling king manoeuvres where rook is boxed in by king
					}
				}
			}
			else if (whiteKingSquareIndex == 2 || whiteKingSquareIndex == 1 || whiteKingSquareIndex == 0) { // generally safe queenside squares for king (a1,b1,c1)
				kingSafetyEval += 50;
				for (int i = 0; i < whiteRookIndices.Count; i ++) {
					if (Board.FileFrom128(whiteRookIndices[i]) < 3) {
						kingSafetyEval -= 55; // penalize non-castling king manoeuvres where rook is boxed in by king
					}
				}
			}
		}
		// king safety black
		if (Board.BlackHasCastlingRights ()) {
			kingSafetyEval -= 10; // not safe, but at least retaining ability to castle
		} else {
			if (blackKingSquareIndex == 118 || blackKingSquareIndex == 119) { // generally safe kingside squares for king (g8,h8)
				kingSafetyEval -= 50;
				for (int i = 0; i < blackRookIndices.Count; i ++) {
					if (Board.FileFrom128(blackRookIndices[i]) > 6) {
						kingSafetyEval += 55; // penalize non-castling king manoeuvres where rook is boxed in by king
					}
				}
			}
			else if (blackKingSquareIndex == 114 || blackKingSquareIndex == 113 || blackKingSquareIndex == 112) { // generally safe queenside squares for king (a8,b8,c8)
				kingSafetyEval -= 50;
				for (int i = 0; i < blackRookIndices.Count; i ++) {
					if (Board.FileFrom128(blackRookIndices[i]) < 3) {
						kingSafetyEval += 55; // penalize non-castling king manoeuvres where rook is boxed in by king
					}
				}
			}
		}
		
		int openingMaterialCount = 16 * pawnValue + 4 * (rookValue + knightValue + bishopValue) + 2 * queenValue;
		int endgameMaterialCount = 2 * (rookValue + knightValue);
		
		//float gameStage = 

		int finalEval = materialEval * 1000 + mobilityEval + kingSafetyEval + developmentEval;
		Timer.Stop ("Eval");
		return finalEval;
	}
	
	/// Returns the material differnce between the captured piece and the capturing piece
	/// This eval is used for move ordering and operates under the assumption that one will generally use one's lowest value piece to capture the opponent's highest value piece.
	public static int MaterialDifference(int capturingPieceCode, int capturedPieceCode) {
		return Math.Abs (pieceValues [capturedPieceCode]) - Math.Abs (pieceValues [capturingPieceCode]);
	}
	
}
