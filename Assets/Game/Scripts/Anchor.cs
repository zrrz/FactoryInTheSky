using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor {
	
    public Vector3 position;

    public Anchor() {
        World.instance.RegisterAnchor(this);
    }

  //  public void ClearAnchor() {
		//World.instance.UnregisterAnchor(this);
    //}


    ~Anchor()
    {
        World.instance.UnregisterAnchor(this);
        //Debug.LogError("Removing Anchor");
    }
}
