using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class OpeningBookGenerator {

	static Dictionary<ulong, List<ushort>> book = new Dictionary<ulong, List<ushort>> ();
	static List<ulong> keys = new List<ulong> ();

	public static void GenerateBook() {
		Timer.Start ("Book Generation");
		string[] gmGames = Directory.GetFiles("Assets/Opening Book/GM Games", "*.txt");
		string[] openings = Directory.GetFiles("Assets/Opening Book/Openings", "*.txt");

		ProcessFiles (gmGames, 60, 20); // for any gm game in database that lasted more than x ply, record first y ply
		ProcessFiles (openings, 0, 50); // record first n ply from specially prepared openings database
		WriteToFile ();

		Timer.Stop ("Book Generation");
		Timer.Print ("Book Generation");
	}

	static void ProcessFiles(string[] files, int minGameLengthPly, int recordToPly) {

		Board.SetPositionFromFen(Definitions.startFen);
		
		// Read pgns and convert to opening book dictionary
		for (int fileIndex =0; fileIndex < files.Length; fileIndex ++) {
			StreamReader reader = new StreamReader (files[fileIndex]);
			List<string> pgns = new List<string>();

			// split text into array of pgn strings
			bool readingPGN = false;
			int pgnIndex = -1;
			bool finishedReadingPGN = false;
			while (reader.Peek() != -1) {
				string line = reader.ReadLine() + " ";
				if (line.Contains("[")) { // comment line
					finishedReadingPGN = false;
					readingPGN = false;
					continue;
				}
				else if (!finishedReadingPGN) {
					for (int charIndex = 0; charIndex < line.Length; charIndex ++) {
						if (!readingPGN && line[charIndex] == '1') {
							readingPGN = true;
							pgns.Add("");
							pgnIndex ++;
						}
						if (readingPGN) {
						
							pgns[pgnIndex] += line[charIndex] + "";
							if (pgns[pgnIndex].Split('.').Length * 2 > recordToPly) { // only record the first n moves for opening book
								finishedReadingPGN = true;
								break;
							}
						}
					}
				}
			}

			reader.Close();

			// get moves from pgn files
			for (int i = 0; i < pgns.Count; i ++) {
				string pgn  = pgns[i];

				if (pgn.Split('.').Length * 2 < minGameLengthPly) { // don't record games that were shorter than minGameLengthPly. This is to avoid games where an opening distaster occured
					continue;
				}

				List<string> moveStrings = PGNReader.MoveStringsFromPGN(pgn);
				List<ushort> moves = PGNReader.MovesFromPGN(pgn);
				
				for (int j =0; j < moves.Count; j ++) {
					if (!book.ContainsKey(Board.zobristKey)) {
						keys.Add(Board.zobristKey);
						book.Add(Board.zobristKey, new List<ushort>());
					}
					if (!book[Board.zobristKey].Contains(moves[j])) {
						book[Board.zobristKey].Add(moves[j]);
					}
					Board.MakeMove(moves[j]);
				}
				for (int k = moves.Count-1; k>= 0; k --) {
					Board.UnmakeMove(moves[k]);
				}

			}
			
			
		}

	}

	static void WriteToFile() {
		// Write book to file
		StreamWriter writer = new StreamWriter ("Assets/Resources/Book.txt");
		
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
