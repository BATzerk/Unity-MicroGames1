﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageLines : MonoBehaviour {
    // Components
    private List<ImageLine> lines; // the actual line images.
    private List<Image> joints; // the circles between lines for those smooth, round joints.
	// Properties
	private Color lineColor;
	private float lineThickness;
	private List<Vector2> points;

    // Getters
    public int NumPoints { get { return points.Count; } }
    public Vector2 GetPoint(int index) { return points[index]; }
    public Vector2 GetPointFirst() { return GetPoint(0); }
    public Vector2 GetPointLast() { return GetPoint(NumPoints-1); }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (Transform tf_parent) {
        GameUtils.ParentAndReset(this.gameObject, tf_parent);
		this.gameObject.name = "ImageLines";
        Initialize();
    }
    public void Initialize() {
        lines = new List<ImageLine>();
        joints = new List<Image>();
		points = new List<Vector2> ();
	}


	// ----------------------------------------------------------------
	//  Add/Remove/Set
	// ----------------------------------------------------------------
    public void AddPoint() { AddPoint(Vector2.zero); }
    public void AddPoint (Vector2 _point) {
		points.Add (_point);
        // If we have at least 1 point (to make a circle)!...
        if (points.Count > 0) {
            AddJoint (points[NumPoints-1]);
        }
		// If we have at least 2 points (to make a line)!...
		if (points.Count > 1) {
			AddLine (points[NumPoints-2], points[NumPoints-1]);
		}
	}
	public void RemovePoint () {
        points.RemoveAt (NumPoints-1);
        // If we at least have any joints left...
        if (joints.Count > 0) {
            RemoveJoint ();
        }
        // If we at least have any lines left...
        if (lines.Count > 0) {
            RemoveLine ();
        }
    }
    public void SetNumPoints(int _numPoints) {
        // I've too many? Remove extras.
        if (NumPoints > _numPoints) {
            while (points.Count > _numPoints) {
                RemovePoint();
            }
        }
        // I've too few? Add more.
        else if (NumPoints < _numPoints) {
            int pointsToAdd = _numPoints - NumPoints;
            for (int i=0; i<pointsToAdd; i++) {
                AddPoint();
            }
        }
   }

    private void AddLine (Vector2 startPos, Vector2 endPos) {
        ImageLine newLine = Instantiate(ResourcesHandler.Instance.imageLine).GetComponent<ImageLine>();
        newLine.Initialize (this.transform, startPos,endPos);
        newLine.gameObject.name = "ImageLine" + lines.Count;
        newLine.SetColor (lineColor);
        newLine.SetThickness (lineThickness);
        lines.Add (newLine);
    }
    private void RemoveLine () {
        ImageLine line = lines[lines.Count-1];
        lines.Remove (line);
        Destroy (line.gameObject);
    }
    private void AddJoint (Vector2 pos) {
        Image newImage = Instantiate(ResourcesHandler.Instance.imageLinesJoint).GetComponent<Image>();
        GameUtils.ParentAndReset(newImage.gameObject, this.transform);
        newImage.rectTransform.anchoredPosition = pos;
        newImage.gameObject.name = "Joint" + joints.Count;
        newImage.color = lineColor;
        newImage.rectTransform.sizeDelta = new Vector2(lineThickness, lineThickness);
        joints.Add (newImage);
    }
    private void RemoveJoint () {
        Image joint = joints[joints.Count-1];
        joints.Remove (joint);
        Destroy (joint.gameObject);
    }

	public void SetPoint (int index, Vector2 _point) {
		points[index] = _point;
		// Update the corresponding joints and lines!
        joints[index].rectTransform.anchoredPosition = _point;
		if (index>0) {
			lines[index-1].EndPos = _point;
		}
		if (index<NumPoints-1) {
			lines[index].StartPos = _point;
		}
	}

	// ----------------------------------------------------------------
	//  Visuals
	// ----------------------------------------------------------------
	public void SetActive (bool _isActive) {
		this.gameObject.SetActive (_isActive);
	}
	public void SetColor (Color _color) {
        lineColor = _color;
        foreach (ImageLine line in lines) { line.SetColor (lineColor); }
        foreach (Image obj in joints) { obj.color = lineColor; }
	}
    public void SetAlpha (float _alpha) {
        Color newColor = new Color(lineColor.r,lineColor.g,lineColor.b, _alpha);
        SetColor (newColor);
    }
	public void SetThickness (float _thickness) {
		lineThickness = _thickness;
        foreach (ImageLine line in lines) { line.SetThickness (lineThickness); }
        foreach (Image obj in joints) { obj.rectTransform.sizeDelta = new Vector2(lineThickness,lineThickness); }
	}

}