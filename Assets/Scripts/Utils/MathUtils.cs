﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathUtils {

	static public readonly float HalfPI = Mathf.PI*0.5f;

	/// Maps Cos from (-1 to 1) to (0 to 1); also offsets so 0 returns 1.
	static public float Cos01(float val) { return (1-Mathf.Sin(val)) * 0.5f; }
	/// Maps Sin from (-1 to 1) to (0 to 1); also offsets so 0 returns 0.
	static public float Sin01(float val) { return (1-Mathf.Cos(val)) * 0.5f; }
	/// Maps Sin from (-1 to 1) to (a to b).
	static public float SinRange(float a,float b, float val) { return Mathf.Lerp(a,b, Sin01(val)); }

	static public bool RandBool() { return Random.Range(0f,1f) < 0.5f; }

	static public bool IsSameSign (float a, float b) { return a*b >= 0; }
	static public bool IsSameSign (double a, double b) { return a*b >= 0; }
	static public int Sign(float value, bool doAllow0=true) {
		if (value < 0) return -1;
		if (value > 0) return 1;
		if (doAllow0) return 0;
		return 1; // We can specify to prevent returning 0. Very useful for any variable with "dir" in the name.
	}

	/** Will turn numbers like 0.00008279 to a nice, fair 0! */
	static public float RoundTo4DPs (float _value) {
		return Mathf.Round (_value * 1000f) / 1000f;
	}
	
    static public float DirToRotation(Vector2Int dir) { return -(GetSide(dir)+2) * 90; }
	static public float GetDifferenceBetweenAnglesDegrees (float angleA, float angleB) {
		// Keep both angles between -180 and 180.
		float difference = angleA - angleB;
		if (difference < -180) difference += 360;
		else if (difference > 180) difference -= 360;
		return difference;
	}
	static public float GetDifferenceBetweenAnglesRadians (float angleA, float angleB) {
		// Keep both angles between -PI and PI.
		float difference = angleA - angleB;
		if (difference < -Mathf.PI) difference += Mathf.PI*2;
		else if (difference > Mathf.PI) difference -= Mathf.PI*2;
		return difference;
	}
	static public float GetVector2AngleRadians (Vector2 vector) { return Mathf.Atan2 (-vector.x,vector.y); }
	static public float GetVector2AngleDegrees (Vector2 vector) { return GetVector2AngleRadians (vector) * Mathf.Rad2Deg; }

	/** TO DO: #optimization This function uses way overkill with converting to vectors and back. There has GOT to be a simpler way with just using the angles. */
	public static float GetAngleReflection (float angleIn, float surfaceAngle) {
		return 180+GetVector2AngleDegrees (Vector2.Reflect (GetVectorFromAngleDeg(-angleIn), GetVectorFromAngleDeg(surfaceAngle)));
	}

	/** E.g. if arrayLength is 4, we may return 2,0,3,1. */
	public static int[] GetShuffledIntArray(int arrayLength) {
		int[] array = new int[arrayLength];
		for (int i=0; i<arrayLength; i++) { array[i] = i; }
		return GetShuffledIntArray(array);
	}
    public static List<int> GetShuffledIntArray(List<int> originalList) {
        return new List<int>(GetShuffledIntArray(originalList.ToArray()));
    }
    public static int[] GetShuffledIntArray(int[] originalArray) {
		int[] shuffledArray = new int[originalArray.Length];
		for (int i=0; i<shuffledArray.Length; i++) { shuffledArray[i] = originalArray[i]; }
		for (int i=0; i<shuffledArray.Length; i++) {
			int randIndex = Random.Range(0, shuffledArray.Length);
			int valA = shuffledArray[i];
			int valB = shuffledArray[randIndex];
			shuffledArray[i] = valB;
			shuffledArray[randIndex] = valA;
		}
		return shuffledArray;
	}


	static public float SnapAngle(float angle, float snap) {
		return Mathf.RoundToInt(angle/snap) * snap;
	}

	public static int ManhattanDistance(Vector2Int posA, Vector2Int posB) {
		return ManhattanDistance(posA.x,posA.y, posB.x,posB.y);
	}
	public static int ManhattanDistance(int x1,int y1, int x2,int y2) {
		return Mathf.Abs(x1-x2) + Mathf.Abs(y1-y2);
	}
	public static int ChebyshevDistance(Vector2Int posA, Vector2Int posB) {
		return ChebyshevDistance(posA.x,posA.y, posB.x,posB.y);
	}
	/** Aka "chessboard distance": Min number of moves for king to go from space A to B. */
	public static int ChebyshevDistance(int x1,int y1, int x2,int y2) {
		return Mathf.Max(Mathf.Abs(x1-x2), Mathf.Abs(y1-y2));
	}

    /** For 2D grids. Converts col/row to fit into a 1D array. */
    public static int GridIndex2Dto1D(int col,int row, int numCols) { return col + row*numCols; }
    /** For 2D grids. Converts 1D-array index to col/row. */
    public static Vector2Int GridIndex1Dto2D(int index, int numCols) { return new Vector2Int(index%numCols, Mathf.FloorToInt(index/(float)numCols)); }
    

	public static Vector2Int GetDir(Vector2 posA, Vector2 posB) {
		int degrees = Mathf.RoundToInt(LineUtils.GetAngle_Degrees(posA,posB));
		degrees = Mathf.RoundToInt(SnapAngle(degrees, 45)); // snap to 45-degree increments, yo.
		if (degrees < 0) { degrees += 360; } // keep it positive.
		switch (degrees) {
			case 0: return Vector2Int.R;
			case 45: return Vector2Int.TR;
			case 90: return Vector2Int.T;
			case 135: return Vector2Int.TL;
			case 180: return Vector2Int.L;
			case 225: return Vector2Int.BL;
			case 270: return Vector2Int.B;
			case 315: return Vector2Int.BR;
			default: return Vector2Int.undefined; // Hmm.
		}
	}
    public static Vector2Int GetDir (int side) {
		switch (side) {
			case Sides.L: return Vector2Int.L;
			case Sides.R: return Vector2Int.R;
			case Sides.B: return Vector2Int.B;
			case Sides.T: return Vector2Int.T;
			case Sides.TL: return Vector2Int.TL;
			case Sides.TR: return Vector2Int.TR;
			case Sides.BL: return Vector2Int.BL;
			case Sides.BR: return Vector2Int.BR;
			default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0 through 7.");
        }
    }
	public static int GetSide (Vector2Int dir) {
		if (dir == Vector2Int.L) { return Sides.L; }
		if (dir == Vector2Int.R) { return Sides.R; }
		if (dir == Vector2Int.T) { return Sides.T; }
		if (dir == Vector2Int.B) { return Sides.B; }
		if (dir == Vector2Int.TL) { return Sides.TL; }
		if (dir == Vector2Int.TR) { return Sides.TR; }
		if (dir == Vector2Int.BL) { return Sides.BL; }
		if (dir == Vector2Int.BR) { return Sides.BR; }
        return -1; // Whoops.
    }
	public static int GetOppositeSide (Vector2Int dir) { return GetOppositeSide(GetSide(dir)); }
	public static int GetOppositeSide (int side) { return Sides.GetOpposite(side); }
	public static Vector2Int GetOppositeDir (int side) { return GetDir(GetOppositeSide(side)); }
	/** Useful for flipping dirEntering to dirExiting, for example. Just returns the original value * -1. */
	public static Vector2Int GetOppositeDir (Vector2Int dir) { return new Vector2Int(-dir.x, -dir.y); }
    //public static int GetOppositeSide (Vector2Int dir) { return GetOppositeSide(GetSide(dir)); }
    //public static int GetOppositeSide (int side) {
    //    switch (side) {
    //        case 0: return 2;
    //        case 1: return 3;
    //        case 2: return 0;
    //        case 3: return 1;
    //        default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0, 1, 2, or 3.");
    //    }
    //}
    //public static Vector2Int GetOppositeDir (int side) { return GetDir(GetOppositeSide(side)); }
    ///** Useful for flipping dirEntering to dirExiting, for example. Just returns the original value * -1. */
    //public static Vector2Int GetOppositeDir (Vector2Int dir) { return new Vector2Int(-dir.x, -dir.y); }
    ///** corner: 0 top-left; 1 top-right; 2 bottom-right; 3 bottom-left. */
    //private static Vector2Int GetCornerDir (int corner) {
    //    switch (corner) {
    //        case 0: return new Vector2Int (-1,-1);
    //        case 1: return new Vector2Int ( 1,-1);
    //        case 2: return new Vector2Int ( 1, 1);
    //        case 3: return new Vector2Int (-1, 1);
    //        default: throw new UnityException ("Whoa, " + corner + " is not a valid corner. Try 0, 1, 2, or 3.");
    //    }
    //}



	/** easing: Higher is SLOWER. */
	public static void EaseRect (ref Rect rect, Rect rectTarget, float easing) {
		rect.xMin += (rectTarget.xMin-rect.xMin) / easing;
		rect.xMax += (rectTarget.xMax-rect.xMax) / easing;
		rect.yMin += (rectTarget.yMin-rect.yMin) / easing;
		rect.yMax += (rectTarget.yMax-rect.yMax) / easing;
	}
	public static Rect LerpRect (Rect rectA, Rect rectB, float t) {
		return new Rect (Vector2.Lerp (rectA.position,rectB.position, t), Vector2.Lerp (rectA.size,rectB.size, t));
	}
	public static float InverseLerpRect (Rect rectA, Rect rectB, Rect rectC) {
		float lerpPosX = Mathf.InverseLerp (rectA.position.x, rectB.position.x, rectC.position.x);
		float lerpPosY = Mathf.InverseLerp (rectA.position.y, rectB.position.y, rectC.position.y);
		float lerpSizeX = Mathf.InverseLerp (rectA.size.x, rectB.size.x, rectC.size.x);
		float lerpSizeY = Mathf.InverseLerp (rectA.size.y, rectB.size.y, rectC.size.y);
		// Return the average of all the sides' inverse lerps!
		float lerpAverage = (lerpPosX+lerpPosY+lerpSizeX+lerpSizeY) / 4f;
		return lerpAverage;
	}

	public static bool AreRectsAboutEqual (Rect rectA, Rect rectB, float threshold=0.1f) {
		return Mathf.Abs (rectA.center.x-rectB.center.x)<threshold
			&& Mathf.Abs (rectA.center.y-rectB.center.y)<threshold
			&& Mathf.Abs (rectA.size.x-rectB.size.x)<threshold
			&& Mathf.Abs (rectA.size.y-rectB.size.y)<threshold;
	}

	public static void UpdateRectFromPoint(ref Rect rect, Vector2 point) {
		if (rect.xMin > point.x) { // LEFT
			rect.xMin = point.x;
		}
		if (rect.xMax < point.x) { // RIGHT
			rect.xMax = point.x;
		}
		if (rect.yMin > point.y) { // TOP
			rect.yMin = point.y;
		}
		if (rect.yMax < point.y) { // BOTTOM
			rect.yMax = point.y;
		}
	}
	public static Rect GetCompoundRectangle (Rect rectA, Rect rectB) {
		// FIRST, check if either of these rectangles are total 0's. If one IS, we want to NOT include it in the return value, so simply return the OTHER rectangle. So we don't include the origin (0,0) accidentally.
		if (rectA == Rect.zero) {
			return rectB;
		}
		if (rectB == Rect.zero) {
			return rectA;
		}
		// Otherwise, make a compound rectangle of the two :)
		Rect returnRect = new Rect (rectA);
		UpdateRectFromPoint (ref returnRect, rectB.max);
		UpdateRectFromPoint (ref returnRect, rectB.min);
		return returnRect;
	}


	public static Vector2 AbsVector2 (Vector2 v) {
		return new Vector2 (Mathf.Abs (v.x), Mathf.Abs (v.y));
	}
	public static Vector2 GetRotatedVector2Rad (Vector2 v, float radians) {
		return GetRotatedVector2Deg (v, radians*Mathf.Rad2Deg);
	}
	public static Vector2 GetRotatedVector2Deg (Vector2 v, float degrees) {
		return Quaternion.Euler (0, 0, degrees) * v;
	}
	public static Vector3 GetRotatedVector3Deg (Vector3 v, float degrees) {
		return Quaternion.Euler (0, 0, degrees) * v;
	}
	/** 0 is UP, PI is RIGHT. */
	public static Vector2 GetVectorFromAngleRad (float radians) {
		return new Vector2 (Mathf.Sin (radians), Mathf.Cos (radians));
	}
	/** 0 is UP, 90 degrees is RIGHT. */
	public static Vector2 GetVectorFromAngleDeg (float degrees) { return GetVectorFromAngleRad (degrees*Mathf.Deg2Rad); }

	/*
	public static Vector3 SnapVector2ToGrid (Vector3 _vector) { return SnapVector2ToGrid (_vector.x, _vector.y); }
	public static Vector2 SnapVector2ToGrid (float _x, float _y) {
		float gridSize = EditModeGrid.gridSize;
		return new Vector2 (
			Mathf.Round (_x/gridSize) * gridSize,
			Mathf.Round (_y/gridSize) * gridSize);
	}
//	public static Vector3 SnapVector3ToGrid (Vector3 _vector) { return SnapVector3ToGrid (_vector.x, _vector.y, _vector.z); }
	public static Vector3 RoundVector3ToInts (Vector3 _vector) { return SnapVector3ToGrid (_vector, 1); }
	public static Vector3 SnapVector3ToGrid (Vector3 _vector) { return SnapVector3ToGrid (_vector, EditModeGrid.gridSize); }
	public static Vector3 SnapVector3ToGrid (Vector3 _vector, int _gridSize) {
		return new Vector3 (
			Mathf.Round (_vector.x/_gridSize) * _gridSize,
			Mathf.Round (_vector.y/_gridSize) * _gridSize,
			_vector.z);
	}
	
	public static void UpdateRectFromPoint(ref Rect rect, Vector2 point) {
		if (rect.xMin > point.x) { // LEFT
			rect.xMin = point.x;
		}
		if (rect.xMax < point.x) { // RIGHT
			rect.xMax = point.x;
		}
		if (rect.yMin > point.y) { // TOP
			rect.yMin = point.y;
		}
		if (rect.yMax < point.y) { // BOTTOM
			rect.yMax = point.y;
		}
	}
	*/

//	public static Rect GetMinScreenRectangle (Rect sourceRect) {
//		return GetMinScreenRectangle (sourceRect, Vector2.zero);
////		return GetCompoundRectangle (sourceRect, new Rect (-GameProperties.ORIGINAL_SIZE*0.5f, GameProperties.ORIGINAL_SIZE));
//	}
	/** Returns a rectangle at LEAST as big as our standard dimensions (1024x768).
	 sourceCenterPos: The center of the 1024x768 rectangle. So we can use this function for both locally and globally positioned rectangles. * /
	public static Rect GetMinScreenRectangle (Rect sourceRect, Vector2 sourceCenterPos) {
		return GetCompoundRectangle (sourceRect, new Rect (sourceCenterPos-GameProperties.ORIGINAL_SIZE*0.5f, GameProperties.ORIGINAL_SIZE));
	}

	public static Rect GetCompoundRectangle (Rect rectA, Rect rectB) {
		// FIRST, check if either of these rectangles are total 0's. If one IS, we want to NOT include it in the return value, so simply return the OTHER rectangle. So we don't include the origin (0,0) accidentally.
		if (rectA == new Rect ()) {
			return rectB;
		}
		if (rectB == new Rect ()) {
			return rectA;
		}
		// Otherwise, make a compound rectangle of the two :)
		Rect returnRect = new Rect (rectA);
		UpdateRectFromPoint (ref returnRect, rectB.max);
		UpdateRectFromPoint (ref returnRect, rectB.min);
		return returnRect;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuadInverse (float t) {
		t *= 2f;
		if (t < 1f) return Mathf.Pow (t, 1/3f) * 0.5f;
		t -= 2f;
		t *= -1f;
		return 1f - Mathf.Pow (t, 1/3f) * 0.5f;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuad (float t) {
		//		return EaseInOutQuad (t, 0,1,1);
		t *= 2f;
		if (t < 1f) return t*t*t * 0.5f;
		t -= 2f;
		return 1f + t*t*t * 0.5f;
	}
	/** t: current time
	 *  b: start value
	 *  c: change in value
	 *  d: duration
	 * */
	static public float EaseInOutQuad (float t, float b, float c, float d) {
		t /= d/2f;
		if (t < 1f) return c/2f*t*t*t + b;
		t -= 2f;
		return -c/2f * (t*t*t - 2f) + b;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuart (float t) {
		return EaseInOutQuart (t, 0,1,1);
	}
	/** t: current time
	 *  b: start value
	 *  c: change in value
	 *  d: duration
	 * */
	static public float EaseInOutQuart (float t, float b, float c, float d) {
		t /= d/2f;
		if (t < 1f) return c/2f*t*t*t*t + b;
		t -= 2f;
		return -c/2f * (t*t*t*t - 2f) + b;
	}


	
	/*
	public static int GetSelectableObjectIndexInDir (int currentlySelectedObjIndex, List<Vector2> objPoses, Vector2 dir) {
		// FIRST, if we've got NO object selected, return -1!
		if (currentlySelectedObjIndex == -1) {
			return -1;
		}
		float angle = Mathf.Atan2 (dir.y, dir.x);
		Vector2 currentObjPos = objPoses [currentlySelectedObjIndex];
		float bestFitAngleDifference = 999999; // just send it way up there
		float bestFitDistance = 9999999; // just send it way up there
		int bestFitObjIndex = -1;
		for (int i=0; i<objPoses.Count; i++) {
			if (i == currentlySelectedObjIndex) continue; // Don't check the one already selected hehe.
			Vector2 thisObjPos = objPoses [i];
			float thisDistance = Vector2.Distance(currentObjPos, thisObjPos);
			float thisAngle = Mathf.Atan2 (thisObjPos.y-currentObjPos.y, thisObjPos.x-currentObjPos.x);
			float angleDifference = Mathf.Abs(angle-thisAngle);
			if (angleDifference > Mathf.PI) angleDifference -= Mathf.PI*2; // Keep looped between -PI and PI, brah.
			// If this angleDifference is too large to matter, then fuggedaboudit!
			if (Mathf.Abs(angleDifference) > Mathf.PI*0.46f) continue;
			// If this object is CLOSER than the best-fit object...
			if (bestFitDistance > thisDistance) {
				// If the angle between my input and this object is CLOSER than for the best-fit object...
				if (bestFitAngleDifference > angleDifference) {
					bestFitDistance = thisDistance;
					bestFitAngleDifference = angleDifference;
					bestFitObjIndex = i;
				}
			}
		}
		return bestFitObjIndex;
	}
	*/

}




