using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class OpeningBookGenerator {

	const int openingCutoffPly = 30; // stop adding moves to book after cutoff
	static Dictionary<ulong, List<ushort>> book = new Dictionary<ulong, List<ushort>> ();

	public static void GenerateBook() {

		string[] games = Directory.GetFiles("Assets/Opening Book/PGNs/", "*.pgn");
		List<ulong> keys = new List<ulong> ();

		// Read pgns and convert to opening book dictionary
		for (int i =0; i < games.Length; i ++) {

			StreamReader reader = new StreamReader (games[i]);
			string pgn = reader.ReadToEnd();
			reader.Close();

			List<string> moveStrings = PGNReader.MoveStringsFromPGN(pgn);
			List<ushort> moves = PGNReader.MovesFromPGN(pgn);
			Board.SetPositionFromFen(Definitions.startFen);

			for (int j =0; j < Math.Min(moves.Count, openingCutoffPly); j ++) {
				if (!book.ContainsKey(Board.zobristKey)) {
					keys.Add(Board.zobristKey);
					book.Add(Board.zobristKey, new List<ushort>());
				}

				book[Board.zobristKey].Add(moves[j]);
				Board.MakeMove(moves[j]);
			}
		}

		// Write book to file
		StreamWriter writer = new StreamWriter ("Assets/Opening Book/Openings.txt");

		for (int i =0; i < keys.Count; i ++) {
			List <ushort> moves = book[keys[i]];
			string line = keys[i] + "";
			for (int j =0; j < moves.Count; j ++) {
				line += " " + moves[j];
			}

			writer.WriteLine(line);
		}

		writer.Close ();

	}

}
