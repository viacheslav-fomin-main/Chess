
public class Definitions {

	public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	public const string fileNames = "abcdefgh";
	public const string rankNames = "12345678";
	public enum PieceName {NA, Rook, Knight, Bishop, Queen, King, Pawn};
	public enum ResultType {NA, Checkmate, Resignation, Stalemate, InsufficientMaterial, Repetition, FiftyMoveRule, Timeout};
	
	
	public static string gameStartFen {
		get {
			if (GameManager.instance.useTestPosition) {
				return testPosition;
			}
			return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		}
	}

	public static string testPosition {
		get {
			return "8/8/8/3q4/K1k5/8/8/8 b - -";
			return "b2qkb1r/2pp1pp1/2n2n1p/1p2p3/4P3/1B3N2/1PPPQPPP/1NB1K2R w Kk - 1 10";
			
		}
	}

	public static int FileNumberFromAlgebraicName(char file) {
		return fileNames.IndexOf (file) + 1;
	}
	
	public static int RankNumberFromAlgebraicName(char rank) {
		return rankNames.IndexOf (rank) + 1;
	}

}
