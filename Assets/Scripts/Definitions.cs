
public class Definitions {

	public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	public const string fileNames = "abcdefgh";
	public const string rankNames = "12345678";
	public enum PieceName {NA, Rook, Knight, Bishop, Queen, King, Pawn};
	
	
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
			return "r1b3nr/pppppppp/2nbq3/8/6k1/Q4N2/PPPPPPPP/RNB1KB1R w KQkq -";
			
		}
	}

	public static int FileNumberFromAlgebraicName(char file) {
		return fileNames.IndexOf (file) + 1;
	}
	
	public static int RankNumberFromAlgebraicName(char rank) {
		return rankNames.IndexOf (rank) + 1;
	}

}
