﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlideAndStick {
    public struct ArmpitPos {
        public Vector2Int emptyPos; // the only EMPTY space in the 2x2.
        public Vector2Int dir; // always diagonal. Points from empty space DIRECTLY into the armpit (careful; it's ticklish).
        // Getters (Public)
        /** Returns the vertex (in between 4 board spaces) where I sit. */
        public Vector2 CenterPos() { return emptyPos.ToVector2() + dir.ToVector2()*0.5f; } // Start at source; go halfway in this dir.
    }
    
    
    public class TileViewBody : MonoBehaviour {
        // Constants
//        private readonly Vector2Int[] dirsCardinal = {Vector2Int.L, Vector2Int.R, Vector2Int.B, Vector2Int.T};
        //private readonly Vector2Int[] dirsDiagonal = {Vector2Int.TL, Vector2Int.TR, Vector2Int.BL, Vector2Int.BR};
        // Components
        private List<Image> allImages;
		private List<MergeSpotView> mergeSpotViews; // only exists when we're animating (aka between loc from and to).
        // References
        [SerializeField] private TileView myTileView=null;
        [SerializeField] private Sprite s_bodyUnitRound=null;
        [SerializeField] private Sprite s_square=null; // for bw and belly-button images.
        [SerializeField] private Sprite s_armpitArc=null;
        private Material m_bodyMaterial; // my texture! Set in Initialize, based on my Tile's ColorID.
        // Properties
        [SerializeField] internal bool isShadow=false; // a little weird how we handle the shadow here. Not a huge deal tho.
		public Color BodyColor { get; private set; }
        private int numBodyUnitImages=0; // bodyUnits are the base round rects (one for every footprint). Round rects. Note: This system is simpler than my other image types; we only need an int (no HashSet required).
        private HashSet<Vector2> bwPoses = new HashSet<Vector2>(); // if footprints are toes, these are the spaces bw the toes (very common). Square images; they hide inner bodyUnits' round rects.
        private HashSet<ArmpitPos> armpitPoses = new HashSet<ArmpitPos>(); // armpits are L patterns (somewhat common). Arc images.
        private HashSet<Vector2> bellyButtonPoses = new HashSet<Vector2>(); // belly buttons are 2x2 patterns (somewhat common). Square images.
        private float diameter;
        private float highlightAlpha=0;
        
        // Getters (Static)
		static public float GetDiameter(float unitSize) { return unitSize * 0.9f; }
		static public Color GetBodyColor(int _colorID) {
            switch (_colorID) {
            //case 0: return new Color( 71/255f,128/255f,214/255f);
            //case 1: return new Color(100/255f,220/255f, 95/255f);
            //case 2: return new Color(231/255f,100/255f,192/255f);
            //case 3: return new Color( 75/255f, 28/255f,151/255f);
            //case 4: return new Color(255/255f,240/255f,102/255f);
            //case 5: return new Color(230/255f,170/255f,112/255f);
            //case 6: return new Color(133/255f,220/255f,223/255f);
            //case 7: return new Color(154/255f,156/255f,172/255f);
            case 0: return new Color(158/255f,216/255f,252/255f);
            case 1: return new Color(141/255f,255/255f,131/255f);
            case 2: return new Color(244/255f,255/255f,120/255f);
            case 3: return new Color(200/255f,131/255f,255/255f);
            case 4: return new Color(255/255f,108/255f,185/255f);
            case 5: return new Color(255/255f,185/255f,100/255f);
            case 6: return new Color(154/255f,156/255f,172/255f);
            case 7: return new Color( 58/255f,104/255f,197/255f);
            default: return Color.red; // Oops! Too many colors.
            }
        }
        // Getters (Private)
        private bool HasFPLocal(Vector2Int pos) { return MyTile.HasFPLocal(pos); }
		private Color GetAppliedBodyColor() { return Color.Lerp(BodyColor, Color.white, highlightAlpha); }
        private float UnitSize { get { return myTileView.MyBoardView.UnitSize; } }
        private Tile MyTile { get { return myTileView.MyTile; } }
        private List<Vector2Int> footprintLocal { get { return MyTile.FootprintLocal; } }
        
        private ArmpitPos GetArmpitPos(Vector2Int sourcePos, Vector2Int sourceDir) {
            ArmpitPos ap = new ArmpitPos();
            if (!HasFPLocal(sourcePos + new Vector2Int(sourceDir.x,0))) { // X-adjacent space is empty?
                ap.emptyPos = sourcePos + new Vector2Int(sourceDir.x,0);
                ap.dir = new Vector2Int(-sourceDir.x, sourceDir.y); // simply flip the X!
            }
            else { // Right space is empty?
                ap.emptyPos = sourcePos + new Vector2Int(0,sourceDir.y);
                ap.dir = new Vector2Int(sourceDir.x, -sourceDir.y); // simply flip the Y!
            }
            if (HasFPLocal(ap.emptyPos)) { // Safety check.
                Debug.LogError("Uh-oh! We're saying an armpitPos is in a space where the Tile DOES have a footprint! Hmm.");
            }
            return ap;
        }
        private float GetArmpitImageRotation(ArmpitPos ap) {
            if (ap.dir == Vector2Int.BL) { return 0; }
            if (ap.dir == Vector2Int.BR) { return 90; }
            if (ap.dir == Vector2Int.TR) { return 180; }
            if (ap.dir == Vector2Int.TL) { return -90; }
            return 0; // Hmmm.
        }
        private Vector2 GetArmpitImagePos(ArmpitPos ap) {
            Vector2 cp = ap.CenterPos();
            Vector2 pos = new Vector2(cp.x*UnitSize, -cp.y*UnitSize);
            // Offset so it's flush with my actual diameter.
            float diameterGap = (UnitSize - diameter) * 0.5f;
            pos += new Vector2(diameterGap*ap.dir.x, -diameterGap*ap.dir.y);
            return pos;
        }
        private Vector2 GetBetweenImagePos(Vector2 bwPosBoard) {
            return new Vector2(bwPosBoard.x*UnitSize, -bwPosBoard.y*UnitSize);
        }
        private MergeSpotView GetMergeSpotView(Vector2 bp) {
            // BRUTE-force. Not a huge deal though.
            for (int i=0; i<mergeSpotViews.Count; i++) {
                if (mergeSpotViews[i].IsMergeSpot(bp)) { return mergeSpotViews[i]; }
            }
            return null;
        }
        
        

        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize() {
			BodyColor = GetBodyColor(MyTile.ColorID);
//			BodyColor = new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f)); // DEBUG TEST
            diameter = GetDiameter(UnitSize);
            allImages = new List<Image>(); // Note: We add our first image in UpdateVisualsPostMove.
            m_bodyMaterial = isShadow ? null : ResourcesHandler.Instance.SlideAndStickTileBodyMat(MyTile.ColorID);
            
            // Kinda sloppy how we handle the shadow. :P
            if (isShadow) {
				BodyColor = Color.Lerp(BodyColor, Color.black, 0.11f); // Darkness.
                this.transform.localPosition = new Vector3(0, -UnitSize*0.05f, 0); // nudge shadow down
            }
            else {
                this.transform.localPosition = new Vector3(0, UnitSize*0.10f, 0); // nudge body up
            }
        }
        
        
        
        // ----------------------------------------------------------------
        //  Adding Things
        // ----------------------------------------------------------------
        private Image AddImage(Sprite sprite, string _name) {
            Image newImage = new GameObject().AddComponent<Image>();
            GameUtils.ParentAndReset(newImage.gameObject, this.transform);
            newImage.name = _name;
            newImage.sprite = sprite;
            newImage.material = m_bodyMaterial;
            newImage.color = GetAppliedBodyColor();
            newImage.transform.SetAsFirstSibling(); // put behind everything else.
            GameUtils.SizeUIGraphic(newImage, diameter,diameter); // default size it to my diameter.
            allImages.Add(newImage);
            return newImage;
        }
        
        private void AddBodyImage(Vector2Int pos) {
            numBodyUnitImages ++; // note: Instead of adding to a HashSet, we just use a single counter.
            Image newImage = AddImage(s_bodyUnitRound, "i_BodyUnit");
			newImage.type = Image.Type.Sliced;
            newImage.rectTransform.anchoredPosition = new Vector2(pos.x*UnitSize, -pos.y*UnitSize);
        }
        private void MaybeAddBetweenImage(Vector2 bp) {//Vector2Int sourcePos, Vector2Int dir) {
            //Vector2 bp = sourcePos.ToVector2() + dir.ToVector2()*0.5f; // Start at source; go halfway in this dir.
            // We HAVEN'T yet added this one?? Add it!!
            if (!bwPoses.Contains(bp)) {
                bwPoses.Add(bp);
                Image newImage = AddImage(s_square, "i_Between");
                newImage.rectTransform.anchoredPosition = GetBetweenImagePos(bp);
            }
        }
        private void MaybeAddBellyButtonImage(Vector2Int sourcePos, Vector2Int dir) {
            Vector2 bp = sourcePos.ToVector2() + dir.ToVector2()*0.5f; // Start at source; go halfway in this dir.
            // We HAVEN'T yet added this one?? Add it!!
            if (!bellyButtonPoses.Contains(bp)) {
                bellyButtonPoses.Add(bp);
                Image newImage = AddImage(s_square, "i_BellyButton");
                newImage.rectTransform.anchoredPosition = new Vector2(bp.x*UnitSize, -bp.y*UnitSize);
                newImage.rectTransform.sizeDelta = new Vector2(UnitSize*0.15f,UnitSize*0.15f);
                // Extra step: Maybe remove an armpit image, if there was one here!
                RemoveArmpitImageAtCenter(bp);
            }
        }
        private void MaybeAddArmpitImage(Vector2Int sourcePos, Vector2Int sourceDir) {
            ArmpitPos ap = GetArmpitPos(sourcePos, sourceDir);
            // We HAVEN'T yet added this one?? Add it!!
            if (!armpitPoses.Contains(ap)) {
                armpitPoses.Add(ap);
                Image newImage = AddImage(s_armpitArc, "i_Armpit");
                RectTransform rt = newImage.rectTransform;
                rt.pivot = Vector2.zero;
                rt.anchoredPosition = GetArmpitImagePos(ap);
                rt.localEulerAngles = new Vector3(0,0,GetArmpitImageRotation(ap));
                // Animate in the armpit (by size)! :)
                float d = Mathf.Min(30, diameter); // Note: All armpit images are the same curviness, because our roundRects are sliced! Also, use Min in extreme case this would be bigger than the Tile.
                Vector2 finalSize = new Vector2(d,d);
                if (myTileView.MyBoardView.IsInitializing) { // Initializing? Oh, ok, don't animate.
                    GameUtils.SizeUIGraphic(newImage, finalSize);
                }
                else { // Not initializing? DO animate!
                    GameUtils.SizeUIGraphic(newImage, 0,0); // start at no size.
                    LeanTween.size(rt, finalSize, 0.4f).setEaseOutQuint();
                }
            }
        }
        // ----------------------------------------------------------------
        //  Removing Things
        // ----------------------------------------------------------------
        private void RemoveExBetweenImages() {
            List<Vector2> bwPosesToRemove = new List<Vector2>(); // make a separate list FIRST (so we don't remove items as we loop).
            foreach (Vector2 bwPos in bwPoses) {
                if (!MyTile.HasMergePosLocal(bwPos)) { // my Tile no longer has this between-pos?? Remove the corresponding image!
                    bwPosesToRemove.Add(bwPos);
                }
            }
            // Now actually remove 'em.
            foreach (Vector2 bwPos in bwPosesToRemove) { RemoveBetweenImage(bwPos); }
        }
        private void RemoveArmpitImageAtCenter(Vector2 centerPos) {
            foreach (ArmpitPos ap in armpitPoses) {
                if (ap.CenterPos() == centerPos) {
                    RemoveArmpitImage(ap);
                    return; // There should only be one, so we're good to stop looking.
                }
            }
        }
        private void RemoveBetweenImage(Vector2 bwPos) {
            bwPoses.Remove(bwPos);
            // Brute-force look through all my images; find the one that matches.
            Vector2 imagePos = GetBetweenImagePos(bwPos);
            foreach (Image image in allImages) {
                // Correct sprite AND pos? It's the image we're looking for!
                if (image.sprite==s_square && image.rectTransform.anchoredPosition==imagePos) {
                    allImages.Remove(image);
                    Destroy(image.gameObject);
                    return;
                }
            }
            Debug.LogError("Whoa, couldn't find between image to remove!");
        }
        private void RemoveArmpitImage(ArmpitPos armpitPos) {
            armpitPoses.Remove(armpitPos);
            // Brute-force look through all my images; find the one that matches this armpit.
            Vector2 imagePos = GetArmpitImagePos(armpitPos);
            foreach (Image image in allImages) {
                // Armpit sprite, AND in this position? It's the image we're looking for!
                if (image.sprite==s_armpitArc && image.rectTransform.anchoredPosition==imagePos) {
                    allImages.Remove(image);
                    Destroy(image.gameObject);
                    return;
                }
            }
            Debug.LogError("Whoa, couldn't find armpit image to remove!");
        }
        
        
        
        // ----------------------------------------------------------------
        //  Adding Doers
        // ----------------------------------------------------------------
        private void AddMissingBodyUnitImages() {
            for (int i=numBodyUnitImages; i<footprintLocal.Count; i++) {
                AddBodyImage(footprintLocal[i]);
            }
        }
        private void AddMissingBetweenImages() {
            //HashSet<Vector2Int> fpLocalHash = new HashSet<Vector2Int>(footprintLocal); // make HashSet for easy accessin'.
            //foreach (Vector2Int fpLocal in footprintLocal) { // For each footprint space...
            //    foreach (Vector2Int dir in dirsCardinal) { // For each side...
            //        Vector2Int posInDir = fpLocal + dir;
            //        if (fpLocalHash.Contains(posInDir)) { // I have a footprint here!...
            //            MaybeAddBetweenImage(fpLocal, dir);
            //        }
            //    }
            //}
            foreach (Vector2 mergePosLocal in MyTile.MergePosesLocal) {
                MaybeAddBetweenImage(mergePosLocal);
            }
        }
        private void AddMissingArmpitAndBellyButtonImages() {
            Vector2Int[] _dirs = { Vector2Int.TR, Vector2Int.TL }; // NOTE: We only need to look in TWO directions, not all 4.
            foreach (Vector2Int fpLocal in footprintLocal) { // For each footprint space...
                foreach (Vector2Int dir in _dirs) { // For each DIAGONAL space around this one...
                    // We DO have another footprint at this diagonal!
                    if (HasFPLocal(fpLocal + dir)) {
                        Vector2Int adjPosA = fpLocal + new Vector2Int(dir.x, 0);
                        Vector2Int adjPosB = fpLocal + new Vector2Int(0, dir.y);
                        bool hasFPA = HasFPLocal(adjPosA);
                        bool hasFPB = HasFPLocal(adjPosB);
                        // 1) Both adjacent sides (towards the diagonal) ARE mine! It's a belly button!
                        if (hasFPA && hasFPB) {
                            MaybeAddBellyButtonImage(fpLocal, dir);
                        }
                        // 2) ONE of the adjacent sides is mine and one isn't. It's an armpit!
                        else if (hasFPA ^ hasFPB) {
                            MaybeAddArmpitImage(fpLocal, dir);
                        }
                        // 3) Both adjacent sides (towards the diagonal) aren't mine? Do nothing.
                        else { }
                    }
                }
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void ApplyBodyColor() {
            Color color = GetAppliedBodyColor();
            for (int i=0; i<allImages.Count; i++) { allImages[i].color = color; }
        }
        public void SetHighlightAlpha(float alpha) {
            highlightAlpha = alpha;
            ApplyBodyColor();
        }
        
		public void UpdateVisualsPostMove() {
			DestroyMergeSpotViews(); // We wanna nix any MergeSpotViews for finished-animation.
            RemoveExBetweenImages(); // Remove between images that have been removed from a Tile-split.
            AddMissingBodyUnitImages();
            AddMissingBetweenImages();
            AddMissingArmpitAndBellyButtonImages();
		}

		public void GoToValues(float loc) {
			for (int i=0; i<mergeSpotViews.Count; i++) {
				mergeSpotViews[i].GoToValues(loc);
			}
		}
//		public void SetValues_From_ByCurrentValues() {
//			for (int i=0; i<mergeSpotViews.Count; i++) {
//				mergeSpotViews[i].SetValues_From_ByCurrentValues();
//			}
//		}
		public void SetValues_To(Tile simTile) {
			RemakeMergeSpotViews(simTile);
		}
		private void RemakeMergeSpotViews(Tile simTile) {
			DestroyMergeSpotViews(); // Just in case.

//			if (bodyImages != null) {
//				BodyColor = new Color(BodyColor.r,BodyColor.g,BodyColor.b, simBO.IsInPlay ? 1 : 0.4f);
//				ApplyBodyColor();
//			}
            
            // ONLY make MergeSpotViews for Tiles still in play!
            if (simTile.IsInPlay) {
                Board simBoard = simTile.BoardRef;
                // BaseUnit MergeSpots.
    			for (int i=0; i<simBoard.LastMergeSpots.Count; i++) {
    				MergeSpot ms = simBoard.LastMergeSpots[i];
    				if (simTile.FootprintGlobal.Contains(ms.PosPlusDir())) {
    					AddMergeSpotView(ms);
    				}
    			}
                // Belly-button (aka between BaseUnits, aka 2x1's) MergeSpots.
                // 1) Plan which ones we're gonna add.
                List<MergeSpot> bbSpotsToAdd = new List<MergeSpot>();
                for (int i=0; i<mergeSpotViews.Count; i++) {
                    MergeSpotView msv = mergeSpotViews[i];
                    MergeSpot ms = msv.MyMergeSpot;
                    Vector2Int msPos = new Vector2Int(ms.pos.x,ms.pos.y);
                    MaybeAddBetweenMergeSpot(bbSpotsToAdd, msPos,Vector2Int.B);
                    MaybeAddBetweenMergeSpot(bbSpotsToAdd, msPos,Vector2Int.R);
                }
                // 2) Add the ones we planned!
                for (int i=0; i<bbSpotsToAdd.Count; i++) {
                    AddMergeSpotView(bbSpotsToAdd[i]);
                }
            }
		}
        private void MaybeAddBetweenMergeSpot(List<MergeSpot> bbSpotsToAdd, Vector2Int msPos, Vector2Int dir) {
            MergeSpotView msv = GetMergeSpotView((msPos+dir).ToVector2());
            if (msv != null) {
                MergeSpot otherMergeSpot = msv.MyMergeSpot;
                MergeSpot newMergeSpot = new MergeSpot(msPos.ToVector2() + dir.ToVector2()*0.5f, otherMergeSpot.dir, MyTile.ColorID);
                bbSpotsToAdd.Add(newMergeSpot);
            }
        }


		private void DestroyMergeSpotViews() {
			if (mergeSpotViews != null) {
				for (int i=0; i<mergeSpotViews.Count; i++) {
					Destroy(mergeSpotViews[i].gameObject);
				}
			}
			mergeSpotViews = new List<MergeSpotView>();
		}
		private void AddMergeSpotView(MergeSpot mergeSpot) {
			MergeSpotView obj = Instantiate(ResourcesHandler.Instance.slideAndStick_mergeSpotView).GetComponent<MergeSpotView>();
			obj.Initialize(myTileView, this, mergeSpot);
			mergeSpotViews.Add(obj);
		}
        
        
    }
}


//case 0: return new ColorHSB(128/255f,250/255f,180/255f).ToColor();
//case 1: return new ColorHSB( 58/255f,250/255f,180/255f).ToColor();
//case 2: return new ColorHSB( 28/255f,250/255f,220/255f).ToColor();
//case 3: return new ColorHSB(180/255f,250/255f,180/255f).ToColor();
//case 4: return new ColorHSB(220/255f,210/255f,220/255f).ToColor();
//case 5: return new ColorHSB(  5/255f,250/255f,220/255f).ToColor();
//case 0: return new ColorHSB(128/255f,220/255f,200/255f).ToColor();
//case 1: return new ColorHSB( 58/255f,220/255f,200/255f).ToColor();
//case 2: return new ColorHSB( 28/255f,200/255f,245/255f).ToColor();
//case 3: return new ColorHSB(180/255f,250/255f,200/255f).ToColor();
//case 4: return new ColorHSB(220/255f,150/255f,245/255f).ToColor();
//case 5: return new ColorHSB(  5/255f,250/255f,245/255f).ToColor();
