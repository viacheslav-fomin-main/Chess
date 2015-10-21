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
	}
	
	public static int Evaluate() {
		int eval = 0;
		
		int materialCount = 0; // amount of material left on board
		
		
		for (int squareIndex =0; squareIndex <= 127; squareIndex ++) {
			if ((squareIndex & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}
			
			if (Board.boardArray[squareIndex] != 0) {
				eval += pieceValues[Board.boardArray[squareIndex]];
				
				if (Board.boardColourArray[squareIndex] == Board.whiteCode) { // white piece
					
				}
				else { // black piece
					
				}
			}
			
		}
		
		int openingMaterialCount = 16 * pawnValue + 4 * (rookValue + knightValue + bishopValue) + 2 * queenValue;
		int endgameMaterialCount = 2 * (rookValue + knightValue);
		
		//float gameStage = 
		
		return eval;
	}
	
	/// Returns the material differnce between the captured piece and the capturing piece
	/// This eval is used for move ordering and operates under the assumption that one will generally use one's lowest value piece to capture the opponent's highest value piece.
	public static int MaterialDifference(int capturingPieceCode, int capturedPieceCode) {
		return Math.Abs (pieceValues [capturedPieceCode]) - Math.Abs (pieceValues [capturingPieceCode]);
	}
	
}
