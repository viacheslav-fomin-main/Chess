using System;
using System.Collections;
using System.Collections.Generic;

/*
 * The board is represented as a 64 bit integer.
 * The least significant bit represents a1, the most, h8
 */

public struct BitBoard {

	public ulong board;

	public BitBoard(ulong _board) {
		board = _board;
	}

	public void MakeMove(Move move) {
		bool isMovingPiece = ContainsPieceAtSquare (move.from);
		SetSquare (move.to, isMovingPiece);
		SetSquare (move.from, false);
	}

	public void MakeMove(Coord from, Coord to) {
		SetSquare (from, false);
		SetSquare (to, true);
	}

	/// <summary>
	/// Sets whether or not a piece is located at given coordinate
	/// </summary>
	public void SetSquare(Coord square, bool containsPiece = true) {
		if (containsPiece) {
			board |= 1UL << square.flatIndex;
		} else {
			board &= ~(1UL << square.flatIndex);
		}
	}

	/// <summary>
	/// Sets whether or not a piece is located at given coordinate
	/// </summary>
	public void SetSquares(List<Coord> squares, bool containsPiece = true) {
		for (int i =0; i < squares.Count; i ++) {
			SetSquare(squares[i],containsPiece);
		}
	}

	/// <summary>
	/// IF the given coordinate exists on the board, sets whether or not a piece is located at given coordinate 
	/// </summary>
	public void SafeSetSquare(Coord square, bool containsPiece = true) {
		if (square.inBoard) {
			SetSquare(square, containsPiece);
		}
	}


	public int[] GetActiveIndices() {
		ulong tempBoard = board;

		int[] bitIndices = new int[ActiveBitCount()];
		int i = 0;
		for (int shiftIndex = 0; shiftIndex < 64; shiftIndex ++) {
			if ((board & 1) == 1) {
				bitIndices[i] = shiftIndex;
				i++;
			}
			if (i == bitIndices.Length) {
				break;
			}
			board >>= 1;
		}
		return bitIndices;
	}

	public int ActiveBitCount() { // hamming weights
		ulong i = board;
		i = i - ((i >> 1) & 0x5555555555555555UL);
		i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
		return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
	}

	/// <summary>
	/// Returns true if a piece is located at given coordinate
	/// </summary>
	public bool ContainsPieceAtSquare(Coord square) {
		return ContainsPieceAtSquare(square.flatIndex);
	}

	/// <summary>
	/// Returns true if a piece is located at given index
	/// </summary>
	public bool ContainsPieceAtSquare(int squareIndex) {
		return (board & (1UL << squareIndex)) != 0;
	}

	/// <summary>
	/// Returns true if a piece is located at given coordinate
	/// </summary>
	public bool SafeContainsPieceAtSquare(Coord square) {
		return SafeContainsPieceAtSquare(square.flatIndex);
	}
	
	/// <summary>
	/// Returns true if a piece is located at given index
	/// </summary>
	public bool SafeContainsPieceAtSquare(int squareIndex) {
		if (squareIndex >= 0 && squareIndex < 64) {
			return (board & (1UL << squareIndex)) != 0;
		}
		return false;
	}


	/// <summary>
	/// Removes all bits from this board which are not 1 in the mask
	/// </summary>
	public void And(ulong mask) {
		board &= mask;
	}

	public void Combine(params BitBoard[] bitBoards) {
		for (int i =0; i < bitBoards.Length; i ++) {
			board |= bitBoards[i].board;
		}
	}

	/// <summary>
	/// Combines given bitboards into one
	/// </summary>
	public static BitBoard Combination(params BitBoard[] bitBoards) {
		ulong combinedBoard = 0UL;

		for (int i =0; i < bitBoards.Length; i ++) {
			combinedBoard |= bitBoards[i].board;
		}

		return new BitBoard(combinedBoard);
	}

	/// <summary>
	/// Returns the index of the piece in a bitboard containing only one piece.
	/// </summary>
	public static int BitIndex(ulong singlePieceBoard) {
		int index = 0;
		while (singlePieceBoard != 1) {
			singlePieceBoard >>= 1;
			index ++;
		}
		return index;
	}


	/// <summary>
	/// Prints a graphical representation of this bitboard to the console
	/// </summary>
	public void PrintBoardToConsole(string heading = "") {
		string output = heading + " val = " + board;

		for (int y = 7; y >= 0; y--) {
			output += "\n";
			for (int x = 0; x < 8; x ++) {
				Coord square = new Coord(x,y);
				output += ((ContainsPieceAtSquare(square))?"1":"0");
			}
		}
		UnityEngine.Debug.Log (output);
	}


}
