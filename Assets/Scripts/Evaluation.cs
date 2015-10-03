using System.Collections.Generic;

public static class Evaluation {
	
	static Dictionary<int,int> pieceValues;
	
	public static void Init() {
		pieceValues = new Dictionary<int, int> ();
		for (int i = 0; i <=1; i++) { // piece values (white positive; black negative)
			int colourCode = i;
			int sign = i*2-1;
			pieceValues.Add (Board.pawnCode + colourCode, 10 * sign);
			pieceValues.Add (Board.rookCode + colourCode, 50 * sign);
			pieceValues.Add (Board.knightCode + colourCode, 35 * sign);
			pieceValues.Add (Board.bishopCode + colourCode, 35 * sign);
			pieceValues.Add (Board.queenCode + colourCode, 90 * sign);
			pieceValues.Add (Board.kingCode + colourCode, 999 * sign);
		}
	}
	
	public static int Evaluate() {
		int currentEval = 0;
		
		for (int squareIndex =0; squareIndex <= 127; squareIndex ++) {
			if ((squareIndex & 8) != 0) { // don't look at indices which are not on the real board
				continue;
			}
			
			if (Board.boardArray[squareIndex] != 0) {
				currentEval += pieceValues[Board.boardArray[squareIndex]];
			}
			
		}
		
		
		return currentEval;
	}
	
}
