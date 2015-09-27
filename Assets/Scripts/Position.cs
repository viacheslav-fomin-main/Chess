using System.Diagnostics;

public struct Position {

	public static Stopwatch sw;

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

	BitBoard[] whiteBoards;
	BitBoard[] blackBoards;

	static Definitions.PieceName[] boardOrder = new Definitions.PieceName[]{
		Definitions.PieceName.Pawn,
		Definitions.PieceName.Rook,
		Definitions.PieceName.Knight,
		Definitions.PieceName.Bishop,
		Definitions.PieceName.Queen,
		Definitions.PieceName.King
	};

	public GameState gameState;

	public void MakeMove(Move move) {
		if (sw == null) {
			sw = new Stopwatch();
		}
		sw.Start ();

		UnityEngine.Debug.Log ("MOVE: " + MoveGenerator3.Convert(move.pieceType) +"  Capture: " + move.capturePiece);

		BitBoard moveBoard = GetBoard (move.pieceType);
		moveBoard.SetSquare (move.from, false);
		moveBoard.SetSquare (move.to);

		if (move.isCapture) {
			BitBoard captureBoard = GetBoard(move.capturePiece);
			captureBoard.SetSquare(move.to,false);
		}

		if (move.isWhiteMove) {
			allPiecesW.SetSquare (move.from, false);
			allPiecesW.SetSquare (move.to);
			if (move.isCapture) {
				allPiecesB.SetSquare (move.to, false);
			}
		} else {
			allPiecesB.SetSquare (move.from, false);
			allPiecesB.SetSquare (move.to);
			if (move.isCapture) {
				allPiecesW.SetSquare (move.to, false);
			}
		}

		/*
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
*/

		// put queen on board if promotion
		if (move.isPawnPromotion) {
			if (move.isWhiteMove) {
				queensW.SetSquare(move.to);
				pawnsW.SetSquare(move.to,false);
			}
			else {
				queensB.SetSquare(move.to);
				pawnsB.SetSquare(move.to,false);
			}
		}

		// remove captured pawn if en passant
		if (move.isEnPassantCapture) {
			pawnsW.SetSquare(move.enPassantPawnLocation,false);
			pawnsB.SetSquare(move.enPassantPawnLocation,false);
			allPiecesB.SetSquare(move.enPassantPawnLocation,false);
			allPiecesW.SetSquare(move.enPassantPawnLocation,false);
		}

		// Castles
		if (move.isCastles) {
			if (move.isWhiteMove) {
				rooksW.SetSquare(move.rookFrom,false);
				rooksW.SetSquare(move.rookTo);
				allPiecesW.SetSquare(move.rookFrom,false);
				allPiecesW.SetSquare(move.rookTo);
			}
			else {
				rooksB.SetSquare(move.rookFrom,false);
				rooksB.SetSquare(move.rookTo);
				allPiecesB.SetSquare(move.rookFrom,false);
				allPiecesB.SetSquare(move.rookTo);
			}
		}
		gameState = move.gameStateAfterMove;
		sw.Stop ();

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

		allPiecesW = BitBoard.Combination (rooksW, knightsW, bishopsW, queensW, kingW, pawnsW);
		allPiecesB = BitBoard.Combination (rooksB, knightsB, bishopsB, queensB, kingB, pawnsB);

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

	BitBoard GetBoard(int code) {
		bool white = ((code&8) != 0);
		int pieceCode = code & 7;

		switch (pieceCode) {
		case 1:
			return Pawns(white);
			break;
		case 2:
			return Rooks(white);
			break;
		case 3:
			return Knights(white);
			break;
		case 4:
			return Bishops(white);
			break;
		case 5:
			return Queens(white);
			break;
		case 6:
			return King(white);
			break;
		}
		UnityEngine.Debug.Log ("Failed to find board. Code: " + code + "  Piece code: " + pieceCode);
		return allPiecesB;
	}

	public BitBoard Rooks(bool white) {
		return (white) ? rooksW : rooksB;
	}

	public BitBoard Knights(bool white) {
		return (white) ? knightsW : knightsB;
	}

	public BitBoard Bishops(bool white) {
		return (white) ? bishopsW : bishopsB;
	}

	public BitBoard Queens(bool white) {
		return (white) ? queensW : queensB;
	}

	public BitBoard King(bool white) {
		return (white) ? kingW : kingB;
	}

	public BitBoard Pawns(bool white) {
		return (white) ? pawnsW : pawnsB;
	}
	
	public BitBoard AllPieces(bool white) {
		return (white) ? allPiecesW : allPiecesB;
	}

}