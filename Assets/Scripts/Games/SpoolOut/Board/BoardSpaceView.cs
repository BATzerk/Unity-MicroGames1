﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpoolOut {
	public class BoardSpaceView : MonoBehaviour {
		// Components
		[SerializeField] private Image i_backing=null;
		[SerializeField] private Image i_border=null;
		// Properties
		private Color fillColor;
		// References
		private BoardSpace mySpace;

		// ----------------------------------------------------------------
		//  Initialize
		// ----------------------------------------------------------------
		public void Initialize (BoardView _boardView, BoardSpace mySpace) {
			this.mySpace = mySpace;
			int col = mySpace.Col;
			int row = mySpace.Row;

			// Parent me to my boooard!
			this.transform.SetParent (_boardView.tf_BoardSpaces);
			this.gameObject.name = "BoardSpace_" + col + "," + row;

			// Size/position me right!
			this.transform.localPosition = new Vector3 (_boardView.BoardToX(col), _boardView.BoardToY(row), 0);
			this.transform.localScale = Vector3.one;
			this.transform.localEulerAngles = Vector3.zero;

			//// YES playable? Make visuals nice!
			//if (mySpace.IsPlayable) {
				const float spaceGap = 0f; // make this something between 0 and 5, to taste.
				float diameter = _boardView.UnitSize-spaceGap;
				GameUtils.SizeUIGraphic (i_border, _boardView.UnitSize+26,_boardView.UnitSize+26);
				GameUtils.SizeUIGraphic (i_backing, diameter,diameter);
                
                float b = BoardUtils.IsSpaceEven(col,row) ? 0.92f : 0.98f;
                fillColor = new Color(b,b,b);
                i_backing.color = fillColor;
                //i_border.color = new Color(0.95f,0.95f,0.95f);
                i_border.enabled = false;
                
                if (!mySpace.IsPlayable) {
                    i_backing.color = new Color(0.2f,0.2f,0.2f);
                }
			//}
			//// NOT playable? Destroy my sprites and do nothing.
			//else {
   //             Destroy(i_border.gameObject);
   //             Destroy(i_backing.gameObject);
			//}
		}
        
        
        // ----------------------------------------------------------------
        //  Animations
        // ----------------------------------------------------------------
        public void PreAnimateInFreshBoard() {
            //float offset = (mySpace.Col + mySpace.Row*1.1f) * 0.1f;
            this.gameObject.transform.localScale = Vector3.zero;//Vector3.one * (0.4f-offset);
        }
        public void AnimateInFreshBoard() {
            //float offset = (mySpace.Col + mySpace.Row) * 0.05f;
            float delay = (mySpace.Col + mySpace.Row) * 0.042f;
            LeanTween.scale(this.gameObject, Vector3.one, 0.52f).setEaseOutBack().setDelay(delay);// + offset
        }


	}
}