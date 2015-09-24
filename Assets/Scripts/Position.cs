
public struct Position {

	public BitBoard pawnsW;
	public BitBoard rooksW;
	public BitBoard knightsW;
	public BitBoard bishopsW;
	public BitBoard queensW;
	public BitBoard kingW;
	public BitBoard allPiecesW;

	public BitBoard pawnsB;
	public BitBoard rooksB;
	public BitBoard knightsB;
	public BitBoard bishopsB;
	public BitBoard queensB;
	public BitBoard kingB;
	public BitBoard allPiecesB;

	public GameState gameState;

	public void MakeMove(Move move) {
		pawnsW.MakeMove(move);
		rooksW.MakeMove(move);
		knightsW.MakeMove(move);
		bishopsW.MakeMove(move);
		queensW.MakeMove(move);
		kingW.MakeMove(move);
		allPiecesW.MakeMove(move);
		
		pawnsB.MakeMove(move);
		rooksB.MakeMove(move);
		knightsB.MakeMove(move);
		bishopsB.MakeMove(move);
		queensB.MakeMove(move);
		kingB.MakeMove(move);
		allPiecesB.MakeMove(move);

		gameState = move.gameStateAfterMove;
	}

	public void SetPositionFromFen(string fen) {
	
		string[] fenSections = fen.Split (' ');

		string pieceChars = "rnbqkpRNBQKP";
		string pieceFen = fenSections[0];
		int boardX = 0;
		int boardY = 7;
		
		for (int i = 0; i < pieceFen.Length; i ++) {
			char key = pieceFen[i];
			
			if (pieceChars.Contains(key.ToString())) {
				Coord currentSquare = new Coord(boardX,boardY);

				switch (key) {
				case 'r':
					rooksB.SetSquare(currentSquare, true);
					break;
				case 'n':
					knightsB.SetSquare(currentSquare, true);
					break;
				case 'b':
					bishopsB.SetSquare(currentSquare, true);
					break;
				case 'q':
					queensB.SetSquare(currentSquare, true);
					break;
				case 'k':
					kingB.SetSquare(currentSquare, true);
					break;
				case 'p':
					pawnsB.SetSquare(currentSquare, true);
					break;

				case 'R':
					rooksW.SetSquare(currentSquare, true);
					break;
				case 'N':
					knightsW.SetSquare(currentSquare, true);
					break;
				case 'B':
					bishopsW.SetSquare(currentSquare, true);
					break;
				case 'Q':
					queensW.SetSquare(currentSquare, true);
					break;
				case 'K':
					kingW.SetSquare(currentSquare, true);
					break;
				case 'P':
					pawnsW.SetSquare(currentSquare, true);
					break;
				}

				boardX ++;
			}
			else if (key == '/') {
				boardX = 0;
				boardY --;
			}
			else {
				int skipCount;
				if (int.TryParse(key + "", out skipCount)) {
					boardX += skipCount;
				}
			}
		}

		allPiecesW = BitBoard.Combine (rooksW, knightsW, bishopsW, queensW, kingW, pawnsW);
		allPiecesB = BitBoard.Combine (rooksB, knightsB, bishopsB, queensB, kingB, pawnsB);

		// Game state
		string sideToMove = fenSections [1];
		string castlingRights = fenSections [2];
		string enPassantCaptureSquare = fenSections [3];
		string halfMoveNumber = fenSections [4];
		string fullMoveNumber = fenSections [5];

		bool whiteToMove = sideToMove == "w";

		bool whiteKingside = false;
		bool whiteQueenside = false;
		bool blackKingside = false;
		bool blackQueenside = false;

		for (int i = 0; i < castlingRights.Length; i ++) {
			switch (castlingRights[i]) {
			case 'K':
				whiteKingside = true;
				break;
			case 'Q':
				whiteQueenside = true;
				break;
			case 'k':
				blackKingside = true;
				break;
			case 'q':
				blackQueenside = true;
				break;
			}
		}

		int enPassantFile = Definitions.fileNames.IndexOf (enPassantCaptureSquare [0]);

		gameState = new GameState (whiteKingside, blackKingside, whiteQueenside, blackQueenside, enPassantFile, whiteToMove);

	}

}