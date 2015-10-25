using System;
using System.Collections;

public static class ZobristKey {

	const int seed = 1982734;

	public static ulong[,,] piecesArray = new ulong[6,2,120]; // piece type, side to move, square index (0x88 board)
	public static ulong[] castlingRightsWhite = new ulong[4]; // kingside only, queenside only, both sides, no castling rights
	public static ulong[] castlingRightsBlack = new ulong[4]; // kingside only, queenside only, both sides, no castling rights
	public static ulong[] enPassantSquare = new ulong[120]; // ep square index
	public static ulong sideToMove; // toggle for side to move

	static Random prng = new Random(seed);

	public static void Init() {

		for (int squareIndex = 0; squareIndex < 120; squareIndex ++) {
			for (int pieceIndex = 0; pieceIndex < 6; pieceIndex ++) {
				piecesArray[pieceIndex,0,squareIndex] = PsuedoRandomNumber();
				piecesArray[pieceIndex,1,squareIndex] = PsuedoRandomNumber();
			}

			enPassantSquare[squareIndex] = PsuedoRandomNumber();
		}

		for (int i = 0; i < 4; i ++) {
			castlingRightsWhite[i] = PsuedoRandomNumber();
			castlingRightsBlack[i] = PsuedoRandomNumber();
		}

		sideToMove = PsuedoRandomNumber ();
	}

	public static ulong GetZobristKey() {
		ulong zobristKey = 0;

		for (int squareIndex = 0; squareIndex < 120; squareIndex ++) {
			if (Board.boardColourArray[squareIndex] != -1) {
				zobristKey ^= piecesArray[((Board.boardArray[squareIndex] & ~1) >> 1) -1,Board.boardColourArray[squareIndex],squareIndex];
			}
		}

		int epIndex = (Board.currentGamestate >> 5 & 15) -1;
		if (epIndex != -1) {
			zobristKey ^= enPassantSquare[epIndex + (((Board.currentGamestate & 1) == 1)?80:32)];
		}

		if ((Board.currentGamestate & 1) == 0) {
			zobristKey ^= sideToMove;
		}

		zobristKey ^= castlingRightsWhite[(Board.currentGamestate >> 1) & 3];
		zobristKey ^= castlingRightsBlack[(Board.currentGamestate >> 3) & 3];

		return zobristKey;
	}

	public static ulong PsuedoRandomNumber() {
		byte[] buffer = new byte[8];
		prng.NextBytes (buffer);
		return BitConverter.ToUInt64 (buffer,0);
	}

}
