using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SlideAndStick {
	[System.Serializable]
	public class Board {
        // Properties
        public int DevRating { get; private set; }
        public int Difficulty { get; private set; }
        public int NumCols { get; private set; }
        public int NumRows { get; private set; }
        public string FUEID { get; private set; }
        // Properties (Variable)
        public bool AreGoalsSatisfied { get; private set; }
        public bool IsInKnownFailState { get; private set; }
        private int numTilesToWin; // set when we're made. Goal: One of each colored Tile!
		private List<MergeSpot> lastMergeSpots; // remade when we call MergeAllTiles.
		// Objects
		public BoardSpace[,] spaces;
        public List<Tile> tiles;
        // Reference Lists
		public List<BoardObject> objectsAddedThisMove;

		// Getters (Private)
		private bool GetAreGoalsSatisfied() {
			return false; // QQQ
//			return tiles.Count <= numTilesToWin;
		}

        // Getters (Public)
        public BoardSpace GetSpace(int col,int row) { return BoardUtils.GetSpace(this, col,row); }
		public BoardSpace[,] Spaces { get { return spaces; } }
        public Tile GetTile(BoardPos pos) { return GetTile(pos.col,pos.row); }
        public Tile GetTile(Vector2Int pos) { return GetTile(pos.x,pos.y); }
        public Tile GetTile(int col,int row) { return BoardUtils.GetOccupant(this, col,row) as Tile; }
		public int GetNumTiles(int colorID) {
			int total = 0;
			foreach (Tile t in tiles) { if (t.ColorID==colorID) { total ++; } }
			return total;
		}
		public List<MergeSpot> LastMergeSpots { get { return lastMergeSpots; } }

		public Board Clone() {
			BoardData data = SerializeAsData();
			return new Board(data);
		}
		public BoardData SerializeAsData() {
			BoardData bd = new BoardData(NumCols,NumRows);
            bd.devRating = DevRating;
            bd.difficulty = Difficulty;
            bd.fueID = FUEID;
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
            Difficulty = bd.difficulty;
            DevRating = bd.devRating;
            FUEID = bd.fueID;

			// Add all gameplay objects!
			MakeEmptyPropLists ();
			MakeBoardSpaces (bd);
			AddPropsFromBoardData (bd);

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
			lastMergeSpots = new List<MergeSpot>();

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
			AddMergeSpots(tileA, tileB);
			//List<Vector2Int> tileBFootGlobal = new List<Vector2Int>(tileB.FootprintGlobal); // note: copy it so we don't modify the original.
			// Remove tileB from the board!
			tileB.RemoveFromPlay();
			// Append tileA's footprint, yo.
			tileA.AppendMyFootprint(tileB);
		}
		/** For each global footprint of tileA, looks around to see if it's mergin' with tileB. If so, we add a MergeLocation there. */
		private void AddMergeSpots(Tile tileA, Tile tileB) {
			for (int i=0; i<tileA.FootprintGlobal.Count; i++) {
				Vector2Int footA = tileA.FootprintGlobal[i];
				MaybeAddMergeLocation(tileA,tileB, footA, Vector2Int.B);
				MaybeAddMergeLocation(tileA,tileB, footA, Vector2Int.L);
				MaybeAddMergeLocation(tileA,tileB, footA, Vector2Int.R);
				MaybeAddMergeLocation(tileA,tileB, footA, Vector2Int.T);
			}
		}
		private void MaybeAddMergeLocation(Tile tileA,Tile tileB, Vector2Int footAPos, Vector2Int dir) {
			// Yes, tileB has a footprint in this dir!
			if (tileB.FootprintGlobal.Contains(footAPos+dir)) {
				lastMergeSpots.Add(new MergeSpot(tileA,tileB, footAPos,dir));
			}
		}


		// ----------------------------------------------------------------
		//  Doers
		// ----------------------------------------------------------------
		/** Moves requested Tile, and the Occupants it'll also push.
			Returns TRUE if we made a successful, legal move, and false if we couldn't move anything. */
		public MoveResults ExecuteMove (BoardPos boToMovePos, Vector2Int dir) {
			// Clear out the Objects-added list just before the move.
			objectsAddedThisMove.Clear();
            foreach (Tile t in tiles) { t.DidJustMove = false; } // Reset DidJustMove for all!

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
            // Update IsInKnownFailState!
            IsInKnownFailState = BoardUtils.IsInHardcodedFailState(this);
		}

		/// Weird, but MUCH easier to program: This is for the merging animation. If tileGrabbing is always at the start of the list, we can count on it always being taken out of play.
		public void OnSetTileGrabbing(Tile _tile) {
			// If there's a tileGrabbing, move it to the beginning of the list (so that it'll be merged last).
			if (_tile != null) {
				tiles.Remove(_tile);
				tiles.Insert(0, _tile);
			}
		}



		// ----------------------------------------------------------------
		//  Debug
		// ----------------------------------------------------------------
		public void Debug_AddTilesIfNone(GameController gameController) {
			if (tiles.Count > 0) { return; } // Nah, we've got some.
			int numToAdd = Mathf.FloorToInt(NumCols*NumRows * gameController.PercentTiles);
			int numColors = gameController.NumColors;
            int stickiness = gameController.Stickiness;
			//			if (tiles.Count == 0) { Debug_AddRandomTiles(Mathf.FloorToInt(NumCols*NumRows*Random.Range(0.5f,0.85f)), numColors); }
			Debug_AddRandomTiles(numToAdd, numColors, stickiness);
			OnMoveComplete();
		}
		private void Debug_AddRandomTiles(int numToAdd, int numColors, int stickiness) {
            //for (int i=0; i<numToAdd; i++) {TEST TEMP
            //    BoardPos randPos = BoardUtils.GetRandOpenPos(this);
            //    if (randPos == BoardPos.undefined) { break; } // No available spaces left?? Get outta here.
            //    int colorID = Random.Range(0, numColors);
            //    AddTile(randPos, colorID);
            //}
            int safetyCount=0;
            while (numToAdd > 0 && safetyCount++<99) {
                BoardPos randPos = BoardUtils.GetRandOpenPos(this);
                if (randPos == BoardPos.undefined) { break; } // No available spaces left?? Get outta here.
                int colorID = Random.Range(0, numColors);
                int clusterAttemptSize = Random.Range(1, stickiness+1);
                for (int j=0; j<clusterAttemptSize; j++) { // For every ONE tile, add more of the same color next to it!
                    if (!BoardUtils.CanAddTile(this,randPos)) { continue; }
                    AddTile(randPos, colorID);
                    numToAdd --;
                    if (numToAdd <= 0) { break; }
                    Vector2Int randDir = BoardUtils.GetRandOpenDir(this, randPos);
                    if (randDir == Vector2Int.zero) { continue; }
                    randPos.col += randDir.x;
                    randPos.row += randDir.y;
                }
            }
		}
		public void Debug_PrintBoardLayout(bool alsoCopyToClipboard=true) {
			string boardString = Debug_GetBoardLayout();
			Debug.Log (boardString);
            if (alsoCopyToClipboard) { GameUtils.CopyToClipboard(boardString); }
		}
		public string Debug_GetBoardLayout() {
			string str = "";
			for (int row=0; row<NumRows; row++) {
                str += "        "; // put it on my tab!
				for (int col=0; col<NumCols; col++) {
					Tile tile = GetTile(col,row);
					str += tile==null ? "." : tile.ColorID.ToString();
				}
				str += ",";
                if (row<NumRows-1) { str += "\n"; }
			}
			return str;
		}



	}
}