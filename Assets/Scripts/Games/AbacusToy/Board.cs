using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AbacusToy {
	[System.Serializable]
	public class Board {
		// Properties
		private bool areGoalsSatisfied;
		private int numCols,numRows;
        public int ParMoves { get; private set; }
        public int NumFootprintsDown { get; set; } // For in-progress moves. We don't wanna do groupfinding/islandtugging unless all footprints are down.
		// Objects
		public BoardSpace[,] spaces;
        public List<Tile> tiles;
        // Reference Lists
		public List<BoardObject> objectsAddedThisMove;

		// Getters (Private)
		private bool GetAreGoalsSatisfied() {
			return false; // FOR NOW, I'm never satisfied.
		}

		// Getters (Public)
		public bool AreGoalsSatisfied { get { return areGoalsSatisfied; } }
		public int NumCols { get { return numCols; } }
		public int NumRows { get { return numRows; } }

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
			BoardData bd = new BoardData(numCols,numRows);
            bd.parMoves = ParMoves;
			foreach (Tile p in tiles) { bd.tileDatas.Add (p.SerializeAsData()); }
			for (int col=0; col<numCols; col++) {
				for (int row=0; row<numRows; row++) {
					bd.spaceDatas[col,row] = GetSpace(col,row).SerializeAsData();
				}
			}
			return bd;
		}



		// ----------------------------------------------------------------
		//  Initialize
		// ----------------------------------------------------------------
		public Board (BoardData bd) {
			numCols = bd.numCols;
			numRows = bd.numRows;
            ParMoves = bd.parMoves;
            NumFootprintsDown = 0;

			// Add all gameplay objects!
			MakeEmptyPropLists ();
			MakeBoardSpaces (bd);
			AddPropsFromBoardData (bd);

			// TEMP TESTING
			if (tiles.Count == 0) { Debug_AddRandomTiles(Mathf.FloorToInt(numCols*numRows*0.65f), Random.Range(3,5)); }
		}

		private void MakeBoardSpaces (BoardData bd) {
			spaces = new BoardSpace[numCols,numRows];
			for (int i=0; i<numCols; i++) {
				for (int j=0; j<numRows; j++) {
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
		//  Doers
		// ----------------------------------------------------------------
		/** Moves requested Tile, and the Occupants it'll also push.
			Returns TRUE if we made a successful, legal move, and false if we couldn't move anything. */
		public MoveResults ExecuteMove(BoardPos boToMovePos, Vector2Int dir) {
			// Clear out the Objects-added list just before the move.
			objectsAddedThisMove.Clear ();

			BoardOccupant boToMove = BoardUtils.GetOccupant(this, boToMovePos);
			MoveResults result = BoardUtils.MoveOccupant(this, boToMove, dir);
            
            // ONLY if this move was a success, do the OnMoveComplete paperwork!
            if (result == MoveResults.Success) {
			    OnMoveComplete ();
            }
			return result;
		}
		private void OnMoveComplete () {
			areGoalsSatisfied = GetAreGoalsSatisfied();
		}


		// ----------------------------------------------------------------
		//  Debug
		// ----------------------------------------------------------------
		private void Debug_AddRandomTiles(int numToAdd, int numColors) {
			for (int i=0; i<numToAdd; i++) {
				BoardPos randPos = BoardUtils.GetRandOpenPos(this, 1);
                if (randPos == BoardPos.undefined) { break; } // No available spaces left?? Get outta here.
				//int colorID = Random.Range(0, numColors);
                int colorID = Mathf.FloorToInt(i/2f);
				AddTile(randPos, colorID);
			}
		}
		public void Debug_PrintBoardLayout(bool alsoCopyToClipboard=true) {
            string layoutString = LayoutString();
			Debug.Log (layoutString);
            if (alsoCopyToClipboard) { UnityEditor.EditorGUIUtility.systemCopyBuffer = layoutString; }
		}
        public string LayoutString() {
            string layoutString = "";
            for (int row=0; row<NumRows; row++) {
                for (int col=0; col<NumCols; col++) {
                    Tile tile = GetTile(col,row);
                    if (tile!=null) { layoutString += tile.ColorID.ToString(); }
                    else { layoutString += GetSpace(col,row).IsPlayable ? "." : "#"; }
                }
                layoutString += "\n";
            }
            return layoutString;
        }



	}
}