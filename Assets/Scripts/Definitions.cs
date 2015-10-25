
public class Definitions {

	public const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	public const string fileNames = "abcdefgh";
	public const string rankNames = "12345678";
	public enum PieceName {NA, Rook, Knight, Bishop, Queen, King, Pawn};
	
	
	public static string gameStartFen {
		get {
			return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		}
	}

	public static int FileNumberFromAlgebraicName(char file) {
		return fileNames.IndexOf (file) + 1;
	}
	
	public static int RankNumberFromAlgebraicName(char rank) {
		return rankNames.IndexOf (rank) + 1;
	}

}
