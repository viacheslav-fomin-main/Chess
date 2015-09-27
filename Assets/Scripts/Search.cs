using System.Collections;
using System.Collections.Generic;
using System;

public class Search {

	public static MoveGenerator3 moveGenerator;

	static int nodesSearched;
	static int breakCount;

	int searchDepth;
	int bestScoreSoFar;
	Move bestMoveSoFar;

	public Move SearchForBestMove(Position position) {
		nodesSearched = 0;
		breakCount = 0;
		moveGenerator = new MoveGenerator3 ();

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
		watch.Start ();

		SearchNode origin = new SearchNode (position);
		searchDepth = 1;
		bool isMaximising = position.gameState.whiteToMove;
		bestScoreSoFar = (isMaximising) ? int.MinValue : int.MaxValue;
		AlphaBetaSearch (origin, searchDepth, int.MinValue, int.MaxValue, position.gameState.whiteToMove);

		watch.Stop ();
		UnityEngine.Debug.Log (nodesSearched + " nodes searched in " + watch.ElapsedMilliseconds + " ms; breakcount: " + breakCount);
		UnityEngine.Debug.Log ("Pos move time: " + Position.sw.ElapsedMilliseconds + " ms");
		moveGenerator.PrintTimes ();
		return bestMoveSoFar;
	}


	int AlphaBetaSearch(SearchNode node, int depth, int alpha, int beta, bool isWhite) {
		node.Init ();
		if (depth == 0 || node.NoMoves()) {
			return node.Evaluate();
		}
		List<SearchNode> children = node.GetChildren ();
		int value = int.MinValue;

		if (isWhite) { // white is trying to attain the highest evaluation possible
			foreach (SearchNode child in children) {
				value = Math.Max(value,AlphaBetaSearch(child, depth-1, alpha, beta, false));
				alpha = Math.Max(alpha, value);

				if(depth == searchDepth ) { // has searched full depth and is now looking at top layer of moves to select the best
					if (value>bestScoreSoFar) {
						bestScoreSoFar = value;
						bestMoveSoFar = child.move;
					}
				}

				if (beta <= alpha) { // break
					breakCount++;
					//break;
				}
			}
			return value;
		}

		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		foreach (SearchNode child in children) {
			value = Math.Min(value,AlphaBetaSearch(child, depth-1, alpha, beta, true));
			beta = Math.Min(beta, value);
			
			if(depth == searchDepth ) { // has searched full depth and is now looking at top layer of moves to select the best
				if (value<bestScoreSoFar) {
					bestScoreSoFar = value;
					bestMoveSoFar = child.move;
				}
			}
			
			if (beta <= alpha) { // break
				breakCount++;
				//break;
			}
		}
		return value;
	}

	void Print(object s) {
		UnityEngine.Debug.Log (s);
	}

	public class SearchNode {
		
		List<SearchNode> children;
		Position currentPosition;
		public Move move { get; private set; } // The move the got to this position
	

		public SearchNode(Position p) {
			currentPosition = p;
		}
		
		public SearchNode(Position p, Move m) {
			currentPosition = p;
			move = m;
		}

		public void Init() {
			Search.nodesSearched ++;
			Move[] allMoves = moveGenerator.GetAllLegalMoves (currentPosition);
			// TODO: prune and sort moves
			children = new List<SearchNode> ();
			
			for (int i =0; i <allMoves.Length; i ++) {
				Position childPosition = currentPosition;
				childPosition.MakeMove(allMoves[i]);
				children.Add(new SearchNode(childPosition, allMoves[i]));
			}
		}
		
		public List<SearchNode> GetChildren() {
			return children;
		}

		public bool NoMoves() {
			return false;
		}

		public int Evaluate() {

			//Random r = new Random ();
			//int eval = r.Next (-5000, 5000);

			//UnityEngine.Debug.Log ("Eval: " + eval);
			int eval = 0;
			for (int i = 0; i < 64; i ++) {
				if (currentPosition.AllPieces(false).ContainsPieceAtSquare(i)) {
					eval -= new Coord(64-i).y;
				}
				if (currentPosition.AllPieces(true).ContainsPieceAtSquare(i)) {
					eval += new Coord(63-i).y+1;
				}
			}
			//eval = UnityEngine.Random.Range (-5000, 5000);
			return eval;
		}
		
		
	}

}

