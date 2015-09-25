using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ABTest : MonoBehaviour {
	

	static int nodesSearched;

	SearchNode bestNode;
	int bestScoreInLastDepth;

	void Start() {
		bool isWhite = false;
		bestScoreInLastDepth = (isWhite)?int.MinValue:int.MaxValue;
		SearchNode origin = new SearchNode ();
		AlphaBetaSearch (origin, 3, int.MinValue, int.MaxValue, isWhite);
		//print ("search outcome: " + AlphaBetaSearch(origin,3,int.MinValue,int.MaxValue,false));
		print ("Best node: " + bestNode.name);
	}

	
	int AlphaBetaSearch(SearchNode node, int depth, int alpha, int beta, bool isWhite) {
	//	print ("In ab - node = " + node.name + "  depth: " + depth + " alpha " + alpha + "  beta " + beta + " iswhite " + isWhite);
		if (depth == 0 || node.NoMoves()) {
			int eval = node.Evaluate();
			print (node.name + " evaluated to " + eval + "   : alpha " + alpha +"   beta " + beta);
			return eval;
		}
		List<SearchNode> children = node.GetChildren ();
		
		if (isWhite) { // white is trying to attain the highest evaluation possible
			int value = int.MinValue;
			foreach (SearchNode child in children) {
				value = Mathf.Max(value,AlphaBetaSearch(child, depth-1, alpha, beta, false));
				alpha = Mathf.Max(alpha, value);
				if(depth == 3 ) { // new best move
					print ("white considers new best move: "+ "v " +value + "> best " + bestScoreInLastDepth + " Result: " + (value>bestScoreInLastDepth) +  "  node name: " + node.name + " child name: " + child.name);
					if (value>bestScoreInLastDepth) {
						bestScoreInLastDepth = value;
						bestNode = child;
					}
				}
				if (alpha >= beta) { // break
					break;
					print("CUTOFF");
				}
			}
			print ("Found value for " + node.name + " a= " + value);
			return value;
		}
		else { // black is trying to obtain the lowest evaluation possible
			int value = int.MaxValue;
			foreach (SearchNode child in children) {
				
				value =Mathf.Min(value, AlphaBetaSearch(child, depth-1, alpha, beta, true));
				beta = Mathf.Min(beta, value);
				if(depth == 3 ) { // new best move
					print ("black considers new best move: "+ "v " +value + "< best " + bestScoreInLastDepth + " Result: " + (value<bestScoreInLastDepth) +  "  node name: " + node.name + " child name: " + child.name);
					if (value<bestScoreInLastDepth) {
						bestScoreInLastDepth = value;
						bestNode = child;
					}
				}
				if (alpha >= beta) { // break
					break;
					
					print("CUTOFF");
				}
			}
			print ("Found value for " + node.name + " b= " + value);
			return value;
			
		}
		
		return 0;
	}
	
	public class SearchNode {
		
		List<SearchNode> children;
		static string names = "ABCDEFGHIJKLMNOP";
		static int nameIndex;

		static int[] evals = new int[]{4,6,7,9,1,7,0,9};
		static int evalIndex;
		public string name;

		public SearchNode() {
			name = names[nameIndex] + "";
			nameIndex ++;
		}
		
		public List<SearchNode> GetChildren() {
			children = new List<SearchNode> ();
			SearchNode a = new SearchNode ();
			SearchNode b = new SearchNode ();
			children.Add (a);
			children.Add (b);
			return children;
		}
		
		public bool NoMoves() {
			return false;
		}
		
		public int Evaluate() {
			int eval = evals [evalIndex];
			evalIndex ++;
			return eval;
		}
		
		
	}

}
