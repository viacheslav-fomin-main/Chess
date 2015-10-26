using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class OpeningBookGenerator {

	const int openingCutoffPly = 20; // stop adding moves to book after cutoff
	const int minGameLengthPly = 0; // game must have lasted at least this many ply in order to be considered for opening book (to prevent quick draw lines + opening disasters)
	static Dictionary<ulong, List<ushort>> book = new Dictionary<ulong, List<ushort>> ();

	public static void GenerateBook() {
		//return;
		string[] files = Directory.GetFiles("Assets/Opening Book/PGNs/", "*.pgn");
		List<ulong> keys = new List<ulong> ();

		// Read pgns and convert to opening book dictionary
		for (int fileIndex =0; fileIndex < files.Length; fileIndex ++) {

			StreamReader reader = new StreamReader (files[fileIndex]);
			string gameFile = reader.ReadToEnd();
			reader.Close();

			List<string> pgns = new List<string>();
			//UnityEngine.Debug.Log("Reading file " +files[fileIndex]);

			bool commentSection = false;
			bool readingPGN = false;
			int pgnIndex = -1;
			for (int charIndex = 0; charIndex < gameFile.Length; charIndex ++) {
				if (gameFile[charIndex] == '[') {
					commentSection = true;
					readingPGN = false;
				}
				else if (gameFile[charIndex] == ']') {
					commentSection = false;
				}
				else if (!commentSection) {
					if (!readingPGN && gameFile[charIndex] == '1') {
						readingPGN = true;
						pgns.Add("");
						pgnIndex ++;
					}
					if (readingPGN) {
						pgns[pgnIndex] += gameFile[charIndex] + "";
					}
				}
			}

	
			for (int i = 0; i < pgns.Count; i ++) {
				string pgn = Sanitize(pgns[i]);
				if (pgn.Split('.').Length * 2 < minGameLengthPly) {
					continue;
				}

				//UnityEngine.Debug.Log("fine " + pgns[i].Length + "  " + pgns[i]);
				//UnityEngine.Debug.Log(files[fileIndex] + " game index: " + i + "  total: " + pgns.Count + "\n" + pgns[i]);
				List<string> moveStrings = PGNReader.MoveStringsFromPGN(pgn);
				List<ushort> moves = PGNReader.MovesFromPGN(pgn);
				Board.SetPositionFromFen(Definitions.startFen);

				for (int j =0; j < Math.Min(moves.Count, openingCutoffPly); j ++) {
					if (!book.ContainsKey(Board.zobristKey)) {
						keys.Add(Board.zobristKey);
						book.Add(Board.zobristKey, new List<ushort>());
					}
					if (!book[Board.zobristKey].Contains(moves[j])) {
						book[Board.zobristKey].Add(moves[j]);
						//UnityEngine.Debug.Log("Move count: " + j + " " + moveStrings[j] + "  " + moves[j]);
					}
					Board.MakeMove(moves[j]);
				}
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
	
	static string Sanitize(string pgn) { // makes pgn format friendly for reader
		string[] sections = pgn.Split ('\n');
		string result = "";
		for (int i = 0; i < sections.Length; i ++) {
			result += sections[i].Replace("\n","") + " ";
		}
		return result;
	}

}
