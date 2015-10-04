using System.Collections.Generic;
using System;
using System.Threading;

public class Search {

	public ushort bestMoveSoFar { get; private set; }
	public bool finishedSearch { get; private set; }

	int searchDepth;
	int bestScoreThisIteration;
	ushort bestMoveThisIteration;
	bool findingMoveForWhite;

	MoveGenerator moveGenerator;

	/// Stat variables
	public int nodesSearched;
	public int breakCount;

	int quiescenceScore;
	
	public void StartSearch() {
		nodesSearched = 0;
		breakCount = 0;

		finishedSearch = false;

		moveGenerator = new MoveGenerator ();
		findingMoveForWhite = Board.isWhiteToPlay ();

		Thread searchThread = new Thread (Iterate);
		searchThread.Start ();
		//Iterate ();
	}


	void Iterate() {
		for (int i =1; i < 5; i ++) {
			searchDepth = i;
			bestScoreThisIteration = (findingMoveForWhite)?int.MinValue:int.MaxValue;

			AlphaBetaSearch(searchDepth,int.MinValue, int.MaxValue, findingMoveForWhite);
			bestMoveSoFar = bestMoveThisIteration;
		}

		finishedSearch = true;
	}


	int AlphaBetaSearch(int depth, int alpha, int beta, bool isWhite) {

		List<ushort> moves = GetOrderedMoves ();
		if (moves.Count == 0) {
			return ((isWhite)?-1:1) * 10000*depth; // if no moves available, side has been checkmated. Return best score for opponent. Checkmating sooner (higher depth) is rewarded.
		}


		if (depth == 0) {
			nodesSearched ++;
			return Evaluate();
			QuiescenceSearch(int.MinValue,int.MaxValue,!isWhite,true);
			return quiescenceScore;
		}

		int value = int.MinValue;
		if (isWhite) { // white is trying to attain the highest evaluation possible

			for (int i =0; i < moves.Count; i ++) {
				Board.MakeMove(moves[i]);
				value = Math.Max(value,AlphaBetaSearch(depth-1, alpha, beta, false));
				alpha = Math.Max(alpha, value);
				Board.UnmakeMove(moves[i]);

				if(depth == searchDepth && findingMoveForWhite) { // has searched full depth and is now looking at top layer of moves to select the best
					if (value>bestScoreThisIteration) {
						bestScoreThisIteration = value;
						bestMoveThisIteration = moves[i];
					}
				}

				if (beta <= alpha) { // break
					breakCount++;
					break;
				}
			}
			return value;
		}

		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		for (int i =0; i < moves.Count; i ++) {
			Board.MakeMove(moves[i]);
			value = Math.Min(value,AlphaBetaSearch(depth-1, alpha, beta, true));
			beta = Math.Min(beta, value);
			Board.UnmakeMove(moves[i]);

			if(depth == searchDepth && !findingMoveForWhite) { // has searched full depth and is now looking at top layer of moves to select the best
				if (value<bestScoreThisIteration) {
					bestScoreThisIteration = value;
					bestMoveThisIteration = moves[i];
				}
			}
			
			if (beta <= alpha) { // break
				breakCount++;
				break;
			}
		}

		return value;
	}

	int QuiescenceSearch(int alpha, int beta, bool isWhite, bool topLevel) {
		List<ushort> captureMoves = GetOrderedCaptures ();

		if (captureMoves.Count == 0) {
			nodesSearched ++;
			int evaluation = Evaluate();
			if (topLevel) {
				quiescenceScore = evaluation;
			}
			return Evaluate();
		}
		
		int value = int.MinValue;
		if (isWhite) { // white is trying to attain the highest evaluation possible
			
			for (int i =0; i < captureMoves.Count; i ++) {
				Board.MakeMove(captureMoves[i]);
				value = Math.Max(value,QuiescenceSearch(alpha, beta, false, false));
				alpha = Math.Max(alpha, value);
				Board.UnmakeMove(captureMoves[i]);
				
				if(topLevel && findingMoveForWhite) { // has searched full depth and is now looking at top layer of moves to select the best
					if (value>quiescenceScore) {
						quiescenceScore = value;;
					}
				}
				
				if (beta <= alpha) { // break
					breakCount++;
					break;
				}
			}
			return value;
		}
		
		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		for (int i =0; i < captureMoves.Count; i ++) {
			Board.MakeMove(captureMoves[i]);
			value = Math.Min(value,QuiescenceSearch(alpha, beta, false, true));
			beta = Math.Min(beta, value);
			Board.UnmakeMove(captureMoves[i]);

			if(topLevel && !findingMoveForWhite) { // has searched full depth and is now looking at top layer of moves to select the best
				if (value<quiescenceScore) {
					quiescenceScore = value;;
				}
			}
			
			if (beta <= alpha) { // break
				breakCount++;
				break;
			}
		}
		
		return value;
	}

	public List<ushort> GetOrderedMoves() {
		return moveGenerator.GetMoves (false, false); // TODO: order these moves in a clever way
	}

	public List<ushort> GetOrderedCaptures() {
		return moveGenerator.GetMoves (true, false); // TODO: order these moves in a clever way
	}

	public int Evaluate() {
		return Evaluation.Evaluate ();
		System.Random r = new Random ();
		return r.Next(-500,500); // TODO: evaluate the current board position
	}

}

