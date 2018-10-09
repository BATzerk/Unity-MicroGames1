using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlideAndStick {
	[System.Serializable]
	public class Board {
        private int numTilesToWin; // set when we're made. Goal: One of each colored Tile!
		// Objects
		public BoardSpace[,] spaces;
        public List<Tile> tiles;
        // Reference Lists
		public List<BoardObject> objectsAddedThisMove;

		// Getters (Private)
		private bool GetAreGoalsSatisfied() {
			return tiles.Count <= numTilesToWin;
		}

        // Getters (Public)
        public bool AreGoalsSatisfied { get; private set; }
        public int NumCols { get; private set; }
        public int NumRows { get; private set; }

        public BoardSpace GetSpace(int col,int row) { return BoardUtils.GetSpace(this, col,row); }
		public BoardSpace[,] Spaces { get { return spaces; } }
        public Tile GetTile(BoardPos pos) { return GetTile(pos.col,pos.row); }
        public Tile GetTile(Vector2Int pos) { return GetTile(pos.x,pos.y); }
        public Tile GetTile(int col,int row) { return BoardUtils.GetOccupant(this, col,row) as Tile; }

		public Board Clone () {
			BoardData data = SerializeAsData();
			return new Board(data);
		}
		public BoardData SerializeAsData() {
			BoardData bd = new BoardData(NumCols,NumRows);
			foreach (Tile p in tiles) { bd.tileDatas.Add (p.SerializeAsData()); }
			for (int col=0; col<NumCols; col++) {
				for (int row=0; row<NumRows; row++) {
					bd.spaceDatas[col,row] = GetSpace(col,row).SerializeAsData();
				}
			}
			return bd;
		}



		// ----------------------------------------------------------------
		//  Initialize
		// ----------------------------------------------------------------
		public Board (BoardData bd) {
			NumCols = bd.numCols;
			NumRows = bd.numRows;

			// Add all gameplay objects!
			MakeEmptyPropLists ();
			MakeBoardSpaces (bd);
			AddPropsFromBoardData (bd);

			// TEMP TESTING
            int numColors = 3;//Random.Range(3,5);
			if (tiles.Count == 0) { Debug_AddRandomTiles(Mathf.FloorToInt(NumCols*NumRows*Random.Range(0.5f,0.85f)), numColors); }

			// Start our solo bubbas out merged, goro!
			MergeAdjacentTiles();

			CalculateNumTilesToWin();
		}

		private void MakeBoardSpaces (BoardData bd) {
			spaces = new BoardSpace[NumCols,NumRows];
			for (int i=0; i<NumCols; i++) {
				for (int j=0; j<NumRows; j++) {
					spaces[i,j] = new BoardSpace (this, bd.spaceDatas[i,j]);
				}
			}
		}
		private void MakeEmptyPropLists() {
			tiles = new List<Tile>();
			objectsAddedThisMove = new List<BoardObject>();
		}
		private void AddPropsFromBoardData (BoardData bd) {
			// Add Props to the lists!
			foreach (TileData data in bd.tileDatas) { AddTile (data); }
		}
		private void CalculateNumTilesToWin() {
			numTilesToWin = 0; // We increment next.
			HashSet<int> colorIDsInBoard = new HashSet<int>();
			for (int i=0; i<tiles.Count; i++) {
				if (!colorIDsInBoard.Contains(tiles[i].ColorID)) {
					colorIDsInBoard.Add(tiles[i].ColorID);
					numTilesToWin ++;
				}
			}
		}


		// ----------------------------------------------------------------
		//  Adding / Removing
		// ----------------------------------------------------------------
		private Tile AddTile(BoardPos pos, int colorID) {
            return AddTile(new TileData(pos, colorID));
		}
		private Tile AddTile (TileData data) {
			Tile prop = new Tile (this, data);
			tiles.Add (prop);
			return prop;
		}

		public void OnObjectRemovedFromPlay (BoardObject bo) {
			// Remove it from its lists!
			//		if (bo is BoardOccupant) { // Is it an Occupant? Remove it from allOccupants list!
			//			allOccupants.Remove (bo as BoardOccupant);
			//		}
			if (bo is Tile) { tiles.Remove(bo as Tile); }
			else { Debug.LogError ("Trying to RemoveFromPlay an Object of type " + bo.GetType().ToString() + ", but our OnObjectRemovedFromPlay function doesn't recognize this type!"); }
		}



        // ----------------------------------------------------------------
        //  Tile Group-Finding
        // ----------------------------------------------------------------
		private void MergeAdjacentTiles() {
			for (int i=tiles.Count-1; i>=0; --i) {
				if (i >= tiles.Count) { continue; } // Oh, if this tile was removed, skip it.
				Tile t = tiles[i];
                for (int j=0; j<t.FootprintGlobal.Count; j++) {
                    Vector2Int fpPos = t.FootprintGlobal[j];
    				MergeTilesAttempt(t, GetTile(fpPos.x-1, fpPos.y));
    				MergeTilesAttempt(t, GetTile(fpPos.x+1, fpPos.y));
    				MergeTilesAttempt(t, GetTile(fpPos.x,   fpPos.y-1));
    				MergeTilesAttempt(t, GetTile(fpPos.x,   fpPos.y+1));
                }
            }
		}

		private bool CanMergeTiles(Tile tileA, Tile tileB) {
			if (tileA==null || tileB==null) { return false; } // Check the obvious.
			if (tileA.ColorID != tileB.ColorID) { return false; } // Different colors? Nah.
			if (!tileA.IsInPlay || !tileB.IsInPlay) { return false; } // One's not in play anymore ('cause it was just merged)? Nah.
			if (tileA == tileB) { return false; } // Oh, we just merged and now it's looking at itself? Nah.
			return true; // Sure!
		}
		private void MergeTilesAttempt(Tile tileA, Tile tileB) {
			if (CanMergeTiles(tileA,tileB)) {
				MergeTiles(tileA,tileB);
			}
		}
		private void MergeTiles(Tile tileA, Tile tileB) {
			List<Vector2Int> tileBFootGlobal = new List<Vector2Int>(tileB.FootprintGlobal); // note: copy it for safety.
			// Remove tileB from the board!
			tileB.RemoveFromPlay();
			// Append tileA's footprint, yo.
			tileA.AppendMyFootprint(tileBFootGlobal);
		}


		// ----------------------------------------------------------------
		//  Doers
		// ----------------------------------------------------------------
		/** Moves requested Tile, and the Occupants it'll also push.
			Returns TRUE if we made a successful, legal move, and false if we couldn't move anything. */
		public MoveResults ExecuteMove (BoardPos boToMovePos, Vector2Int dir) {
			// Clear out the Objects-added list just before the move.
			objectsAddedThisMove.Clear ();

			BoardOccupant boToMove = BoardUtils.GetOccupant(this, boToMovePos);
			MoveResults result = BoardUtils.MoveOccupant (this, boToMove, dir);
            // ONLY if this move was a success, do the OnMoveComplete paperwork!
            if (result == MoveResults.Success) {
			    OnMoveComplete ();
            }
			return result;
		}
		private void OnMoveComplete () {
			MergeAdjacentTiles();
			AreGoalsSatisfied = GetAreGoalsSatisfied();
		}


		// ----------------------------------------------------------------
		//  Debug
		// ----------------------------------------------------------------
		private void Debug_AddRandomTiles(int numToAdd, int numColors) {
			for (int i=0; i<numToAdd; i++) {
				BoardPos randPos = BoardUtils.GetRandOpenPos(this);
				if (randPos == BoardPos.undefined) { break; } // No available spaces left?? Get outta here.
				int colorID = Random.Range(0, numColors);
				AddTile(randPos, colorID);
			}
		}
		public void Debug_PrintBoardLayout(bool alsoCopyToClipboard=true) {
			string boardString = "";
			for (int row=0; row<NumRows; row++) {
				for (int col=0; col<NumCols; col++) {
					Tile tile = GetTile(col,row);
					boardString += tile==null ? "." : tile.ColorID.ToString();
				}
				boardString += "\n";
			}
			Debug.Log (boardString);
            if (alsoCopyToClipboard) { UnityEditor.EditorGUIUtility.systemCopyBuffer = boardString; }
		}



	}
}