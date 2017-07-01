using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RopeBehavior2 : MonoBehaviour {
    public static List<RopeBehavior2> All = new List<RopeBehavior2>();
	// Use this for initialization
	void Start () {
        All.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDestroy()
    {
        All = All.Where(x => x.transform.GetInstanceID() != transform.GetInstanceID()).ToList();
    }
}
