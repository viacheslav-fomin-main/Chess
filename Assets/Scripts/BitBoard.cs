using System;
using System.Collections;
using System.Collections.Generic;

/*
 * The board is represented as a 64 bit integer.
 * The least significant bit represents a1, the most, h8
 */

public sealed class Bitboard {

	ulong board;

	public Bitboard() {
	}

	public Bitboard(ulong newBoardValue) {
		board = newBoardValue;
	}

	/// Returns a clone of the current board that can safely be modified
	public Bitboard Clone() {
		return new Bitboard (board);
	}

	/// Sets square at from index and clears square at to index
	public void MakeMove(int from, int to) {
		SetSquare (from);
		ClearSquare (to);
	}

	
	/// sets or clears the bit at given index
	public void DefineSquare(int squareIndex, bool set) {
		if (set) {
			SetSquare(squareIndex);
		} else {
			ClearSquare(squareIndex);
		}
	}


	/// Sets bit at specified index to 1
	public void SetSquare(int squareIndex) {
		board |= 1UL << squareIndex;
	}

	/// Clears bit at specified index
	public void ClearSquare(int squareIndex) {
		board &= ~(1UL << squareIndex);
	}
	
	/// Returns true if a piece is located at given index
	public bool ContainsPieceAtSquare(int squareIndex) {
		return (board & (1UL << squareIndex)) != 0;
	}

	// Static methods

	/// Returns an overlay of two given bitboards
	public static Bitboard Combination(params Bitboard[] bitboards) {
		ulong combinedBoard = 0UL;
		
		for (int i =0; i < bitboards.Length; i ++) {
			combinedBoard |= bitboards[i].board;
		}
		
		return new Bitboard(combinedBoard);
	}
}
