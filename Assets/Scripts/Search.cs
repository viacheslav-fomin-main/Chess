using System.Collections;
using System.Collections.Generic;
using System;

public class Search {

	IMoveGenerator moveGenerator;

	static int nodesSearched;

	int searchDepth;
	int bestScoreSoFar;
	Move bestMoveSoFar;

	public Move SearchForBestMove(Position position) {
		nodesSearched = 0;
		moveGenerator = new MoveGenerator ();

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
		watch.Start ();

		SearchNode origin = new SearchNode (position, moveGenerator);
		searchDepth = 3;
		bool isMaximising = position.gameState.whiteToMove;
		bestScoreSoFar = (isMaximising) ? int.MinValue : int.MaxValue;
		AlphaBetaSearch (origin, searchDepth, int.MinValue, int.MaxValue, position.gameState.whiteToMove);

		watch.Stop ();
		UnityEngine.Debug.Log (nodesSearched + " nodes searched in " + watch.ElapsedMilliseconds + " ms");

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

				if (alpha >= beta) { // break
					break;
				}
			}
			return value;
		}
		// black is trying to obtain the lowest evaluation possible
		value = int.MaxValue;
		foreach (SearchNode child in children) {
			value = Math.Min(value,AlphaBetaSearch(child, depth-1, alpha, beta, true));
			alpha = Math.Min(alpha, value);
			
			if(depth == searchDepth ) { // has searched full depth and is now looking at top layer of moves to select the best
				if (value<bestScoreSoFar) {
					bestScoreSoFar = value;
					bestMoveSoFar = child.move;
				}
			}
			
			if (alpha >= beta) { // break
				break;
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
		IMoveGenerator moveGenerator;

		public SearchNode(Position p, IMoveGenerator _moveGenerator) {
			currentPosition = p;
			moveGenerator = _moveGenerator;
		}
		
		public SearchNode(Position p, IMoveGenerator _moveGenerator, Move m) {
			currentPosition = p;
			move = m;
			moveGenerator = _moveGenerator;
		}

		public void Init() {
			Search.nodesSearched ++;
			Move[] allMoves = moveGenerator.GetAllLegalMoves (currentPosition);
			// TODO: prune and sort moves
			children = new List<SearchNode> ();
			
			for (int i =0; i <allMoves.Length; i ++) {
				Position childPosition = currentPosition;
				childPosition.MakeMove(allMoves[i]);
				children.Add(new SearchNode(childPosition, moveGenerator, allMoves[i]));
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
			// = UnityEngine.Random.Range (-5000, 5000);
			//UnityEngine.Debug.Log ("Eval: " + eval);
			int eval = 0;
			for (int i = 0; i < 64; i ++) {
				if (currentPosition.allPiecesB.ContainsPieceAtSquare(i)) {
					//eval -= new Coord(64-i).y;
				}
				if (currentPosition.allPiecesW.ContainsPieceAtSquare(i)) {
					eval += new Coord(64-i).y;
				}
			}
			return eval;
		}
		
		
	}

}

