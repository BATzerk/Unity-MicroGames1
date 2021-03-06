﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExtrudeMatch {
    public class PropData {
    }

    public class BoardObjectData : PropData {
    	public BoardPos boardPos;
    //	public BoardObjectData (BoardPos _boardPos) {
    //		boardPos = _boardPos;
    //	}
    }
    public class BoardOccupantData : BoardObjectData {
    }
    public class BoardSpaceData : BoardObjectData {
    	public bool isPlayable = true;
    	public BoardSpaceData (int _col,int _row) {
    		boardPos.col = _col;
    		boardPos.row = _row;
    	}
    }

    public class TileData : BoardOccupantData {
    	public int colorID;
        public int value;
        public TileData (BoardPos _boardPos, int _colorID, int _value) {
    		boardPos = _boardPos;
    		colorID = _colorID;
            value = _value;
    	}
    }
}