class Spool {
  int col,row;
  int colorID;
  float cardThickness;
  PVector corePos;
  float textSize;
  int[] spaceTiers; // incremented when we cross over ourselves.
  Vector2Int[] pathSpaces;
  boolean isHighlighted;
  private float highlightLoc;
  color bodyColor;
  color sideColor;
  color textColor;
  
  // Getters
  private Vector2Int lastSpacePos() {
    return pathSpaces[pathSpaces.length-1];
  }
  Vector2Int secondLastSpacePos() {
    if (pathSpaces.length < 2) { return null; } // No second-last space? Return null.
    return pathSpaces[pathSpaces.length-2];
  }
  public boolean IsLastSpace(int col,int row) {
    Vector2Int lps = lastSpacePos();
    return lps.x==col && lps.y==row;
  }
  public boolean IsSecondLastSpacePos(Vector2Int pos) {
    Vector2Int secondLastPos = secondLastSpacePos();
    if (secondLastPos == null) { return false; } // Don't even have second-to-last space? False.
    return secondLastPos.x==pos.x && secondLastPos.y==pos.y;
  }
  public boolean PathContains(Vector2Int pos) {
    for (int i=0; i<pathSpaces.length; i++) {
      if (pathSpaces[i].Equals(pos)) { return true; }
    }
    return false;
  }
  Spool clone() {
    Spool clone = new Spool(col,row, pathSpaces, colorID);
    clone.corePos = new PVector(corePos.x,corePos.y);
    clone.textSize = textSize;
    clone.bodyColor = bodyColor;
    clone.sideColor = sideColor;
    clone.textColor = textColor;
    clone.highlightLoc = highlightLoc;
    return clone;
  }
  
  Spool(int Col,int Row, Vector2Int[] _pathSpaces, int ColorID) {
    col = Col;
    row = Row;
    SetColorID(ColorID);
    corePos = GridToPos(col,row);
    pathSpaces = CopyVector2Int(_pathSpaces);
    if (pathSpaces == null) { // Convenience: Pass in null to default to just empty path.
      pathSpaces = new Vector2Int[0];
      AddPathSpace(new Vector2Int(col,row));
    }
    UpdateSpaceTiers();
  }
  void SetColorID(int colorID) {
    this.colorID = colorID;
    bodyColor = GetFillColor(colorID);
    sideColor = GetStrokeColor(bodyColor);
    textColor = color(0, 180);
    textSize = unitSize.x*0.5;
  }
  
  void AddPathSpace(Vector2Int spacePos) {
    pathSpaces = (Vector2Int[]) append(pathSpaces, spacePos);
    GetSpace(spacePos.x,spacePos.y).SetSpoolOnMe(this);
    UpdateSpaceTiers();
  }
  void RemovePathSpace() {
    if (pathSpaces.length < 2) { return; } // Safety check!
    // Remove from gridSpaces.
    Vector2Int lps = lastSpacePos();
    GetSpace(lps.x,lps.y).RemoveSpoolOnMe();
    pathSpaces = (Vector2Int[]) shorten(pathSpaces);
    UpdateSpaceTiers();
  }
  void RemoveAllPathSpaces() {
    for (int i=0; i<pathSpaces.length; i++) {
      GetSpace(pathSpaces[i]).RemoveSpoolOnMe();
    }
    pathSpaces = new Vector2Int[0];
    AddPathSpace(new Vector2Int(col,row));
    UpdateSpaceTiers();
  }
  private void UpdateSpaceTiers() {
    spaceTiers = new int[pathSpaces.length];
    int currTier = 0;
    for (int i=0; i<spaceTiers.length; i++) {
      // TODO: This.
      spaceTiers[i] = 0;
    }
  }
  
  
  void Update() {
    UpdateHighlightValues();
    cardThickness = 6 - (highlightLoc*5);//numSpacesLeft*1.3;//
  }
  private void UpdateHighlightValues() {
    isHighlighted = spoolOver==this || spoolGrabbing==this;
    float highlightLocTarget = isHighlighted ? 1 : 0;
    if (highlightLoc != highlightLocTarget) {
      highlightLoc += (highlightLocTarget-highlightLoc) * 0.4;
    }
  }
  
  void Draw() {
    DrawPath();
    DrawCore();
    DrawEndHighlight();
  }
  private void DrawPath() {
    // Path!
    noFill();
    stroke(bodyColor);
    strokeWeight(unitSize.x*0.6);
    beginShape();
    for (int i=0; i<pathSpaces.length; i++) {
      vertex(GridToX(pathSpaces[i].x), GridToY(pathSpaces[i].y));
    }
    endShape();
  }
  private void DrawCore() {
    pushMatrix();
    translate(corePos.x,corePos.y);
    
    // Base
    fill(sideColor);
    noStroke();
    rectMode(CENTER);
    rect(0,0, unitSize.x,unitSize.y);
    // Body
    fill(bodyColor);
    noStroke();
    rectMode(CENTER);
    rect(0,-cardThickness, unitSize.x,unitSize.y-cardThickness*0.5);
//    // Text
//    if (numSpacesLeft > 0) {
//      fill(0, 120);
//      textAlign(CENTER, CENTER);
//      textSize(textSize);
//      text(numSpacesLeft, 0,-cardThickness);
//    }
    
    popMatrix();
  }
  private void DrawEndHighlight() {
    if (isHighlighted) {
      fill(sideColor, highlightLoc*80);
      PVector lastPos = GridToPos(lastSpacePos());
//      rect(lastPos.x,lastPos.y-cardThickness*0.5, unitSize.x,unitSize.y+cardThickness);
      ellipse(lastPos.x,lastPos.y, unitSize.x*0.7,unitSize.y*0.7);
    }
  }
  
  
}
