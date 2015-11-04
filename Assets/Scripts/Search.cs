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
	
	Dictionary<ulong, int> transpositionTable = new Dictionary<ulong, int>();
	
	int debug_hashHits;
	bool useHash = true;
	
	public void StartSearch() {
		bestMoveSoFar = 0;
		transpositionTable.Clear ();
		
		nodesSearched = 0;
		breakCount = 0;
		
		finishedSearch = false;
		
		moveGenerator = new MoveGenerator ();
		findingMoveForWhite = Board.IsWhiteToPlay ();
		
		Thread searchThread = new Thread (Iterate);
		searchThread.Start ();
		//Iterate ();
	}
	
	
	void Iterate() {
		Timer.Start ("Move Time");

		int startDepth = 4;

		for (int i = startDepth; i <= startDepth; i ++) {
			searchDepth = i;
			bestScoreThisIteration = (findingMoveForWhite)?int.MinValue:int.MaxValue;

			AlphaBetaSearch(searchDepth,int.MinValue, int.MaxValue, findingMoveForWhite);
			bestMoveSoFar = bestMoveThisIteration;
		}
		
		finishedSearch = true;
		Timer.Print ("Move Time");
		Timer.Reset ("Move Time");
		//Timer.Print ("Eval");
		Timer.Reset ("Eval");
		//UnityEngine.Debug.Log ("hash hits: " + debug_hashHits);
	}

	
	int AlphaBetaSearch(int plyRemaining, int alpha, int beta, bool isWhite) {
		Heap moveHeap = moveGenerator.GetMoves (false, false);
		int count = moveHeap.Count;

		if (moveHeap.Count == 0) {
			return ((isWhite)?-1:1) * (10000000+plyRemaining); // if no moves available, side has been checkmated. Return best score for opponent. Checkmating sooner (higher depth) is rewarded.
		}
		
		
		if (plyRemaining == 0) {
			nodesSearched ++;
			return Evaluate();
			//QuiescenceSearch(int.MinValue,int.MaxValue,!isWhite,true);
			//return quiescenceScore;
		}
		if (isWhite) { // white is trying to attain the highest evaluation possible
			
			for (int i =0; i < count; i ++) {
				ushort move = moveHeap.RemoveFirst();
				Board.MakeMove (move);
				alpha = Math.Max (alpha, AlphaBetaSearch (plyRemaining - 1, alpha, beta, false));
				Board.UnmakeMove (move);
				
				if (plyRemaining == searchDepth) { // has searched full depth and is now looking at top layer of moves to select the best
					if (alpha > bestScoreThisIteration) {
						bestScoreThisIteration = alpha;
						bestMoveThisIteration = move;
					}
				}
				
				if (beta <= alpha) { // break
					breakCount++;
					break;
				}
			}
			return alpha;
		} else {
			// black is trying to obtain the lowest evaluation possible
			for (int i =0; i < count; i ++) {
				ushort move = moveHeap.RemoveFirst();
				Board.MakeMove (move);
				beta = Math.Min (beta, AlphaBetaSearch (plyRemaining - 1, alpha, beta, true));
				Board.UnmakeMove (move);
				
				if (plyRemaining == searchDepth) { // has searched full depth and is now looking at top layer of moves to select the best
					if (beta < bestScoreThisIteration) {
						bestScoreThisIteration = beta;
						bestMoveThisIteration = move;
					}
				}
				
				if (beta <= alpha) { // break
					breakCount++;
					break;
				}
			}
			
			return beta;
		}
		
		return 0;
	}

	/*
	int QuiescenceSearch(int alpha, int beta, bool isWhite, bool topLevel) {
		SortedList<int, ushort>  captureMoves = moveGenerator.GetMoves (true, false);

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
			value = Math.Min(value,QuiescenceSearch(alpha, beta, true, false));
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
*/
	
	public int Evaluate() {
		return Evaluation.Evaluate ();
	}
	
}

