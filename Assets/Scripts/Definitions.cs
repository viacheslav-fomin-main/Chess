
public class Definitions {
	
	public const string fileNames = "abcdefgh";
	public const string rankNames = "12345678";
	public enum PieceName {NA, Rook, Knight, Bishop, Queen, King, Pawn};


	public static string startFen {
		get {
			return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // real start fen

			return "r3kr2/pppbP2p/2n1pn2/1Bb2pB1/3P4/N1P2N2/PP3PPP/R2QK2R w KQq - 20 11";
			return "rnbqkbnr/pppp3p/4p3/4Ppp1/8/2P5/PP1P1PPP/RNBQKBNR b KQkq f6 6 4";
			return "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"; // perft fen
			return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // real start fen
			return "r1b2bnr/pp2pppp/2p1k3/3p4/3P4/qn1K1PP1/PPP1P2P/RNBQ1BNR b KQkq - 13 7"; // double check pos
			return "rnbq1bnr/pppp1ppp/5k2/4p3/4P3/5K2/PPPP1PPP/RNBQ1BNR w KQkq - 0 1"; // opening pos
		}
	}

}
