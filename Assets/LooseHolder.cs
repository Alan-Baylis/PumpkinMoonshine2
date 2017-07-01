using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets._2D;
public class LooseHolder : MonoBehaviour {
    public Transform Base;
    public Transform Prong1;
    public Transform Prong2;

    public Transform groundedKnowledge;

    private bool isLooseHolding;
    void Awake()
    {
        setEnabled(false);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        //gather info and potentially turn off
        if (!isGrounded())
        {
            setEnabled(false);
            return;
        }

        var ropeBetween = getRopeBetweenProngs();
        if(ropeBetween == null)
        {
            setEnabled(false);
            return;
        }

        //potentiallly turn on
        if (!isLooseHolding)
        {
            var connectedRopes = getConnectedRopes(ropeBetween);
            if (connectedRopes.Count() == 0 || connectedRopes.Count() > 2)
            {
                setEnabled(false);
                return;
            }

            var avgDirection = getAverageDirection(ropeBetween, connectedRopes);

            applyDirectionToBase(avgDirection);

            setEnabled(true);
        }
	}

    private void applyDirectionToBase(Vector2 avgDirection)
    {
        Base.transform.rotation = Quaternion.Euler((Vector3)avgDirection);
    }

    private Vector2 getAverageDirection(Transform middleRope, List<Transform> connectedRopes)
    {
        var avg = Vector2.zero;
        for(var i = 0; i < connectedRopes.Count(); i++)
        {
            var thisDir = ((Vector2)middleRope.position - (Vector2)connectedRopes[i].position).normalized;
            if (Vector2.Dot(avg, thisDir * -1f) > Vector2.Dot(avg, thisDir))
                avg += thisDir * -1f;
            else
                avg += thisDir;
        }
        avg = avg / connectedRopes.Count();

        return avg;
    }

    private Transform getRopeBetweenProngs()
    {
        
        return RopeBehavior2.All
            .Where  (x => Mathf.Abs(((Vector2)x.transform.position - (Vector2)Base.transform.position).magnitude) < 1f)
            .OrderBy(x => Mathf.Abs(((Vector2)x.transform.position - (Vector2)Base.transform.position).magnitude))
            .Select(x=>x.transform)
            .FirstOrDefault();
    }

    private List<Transform> getConnectedRopes(Transform rope)
    {
        return RopeBehavior2.All.Where(x =>
            x.transform.GetComponent<HingeJoint2D>().connectedBody.GetInstanceID() == rope.GetComponent<Rigidbody2D>().GetInstanceID()
          || rope.transform.GetComponent<HingeJoint2D>().connectedBody.GetInstanceID() == x.GetComponent<Rigidbody2D>().GetInstanceID()
        ).Select(x=>x.transform).ToList();
    }

    private void setEnabled(bool enabled)
    {
        Prong1.gameObject.SetActive(enabled);
        Prong2.gameObject.SetActive(enabled);
        Base.gameObject.SetActive(enabled);

        isLooseHolding = enabled;
    }

    private bool isGrounded()
    {
        return groundedKnowledge.GetComponent<UnityStandardAssets._2D.PlatformerCharacter2D>().isGrounded;
    }

}