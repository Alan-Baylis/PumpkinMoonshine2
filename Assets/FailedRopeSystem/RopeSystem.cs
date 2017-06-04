using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class RopeSystem : MonoBehaviour {
    public Transform ropePartPrefab;
    public int length = 10;
    public float connectionDistance = .1f;
    private List<Transform> ropeParts = new List<Transform>();
    // Use this for initialization
	void Start () {
        //var ropeParts = new List<Transform>();
        ropeParts.Add(transform);
	    for(int i = 0; i < length; i++)
        {
            var ropepart = Instantiate(ropePartPrefab) as Transform;
            ropepart.transform.position = new Vector3(transform.position.x, transform.position.y - ((i + 1) * connectionDistance), 0);
            ropepart.GetComponent<RopeBehavior>().connectionDistance = connectionDistance;
            ropepart.GetComponent<CircleCollider2D>().radius = connectionDistance/2f;

            var conns = ropepart.GetComponent<RopeBehavior>().prioritizedConnections;

            var prevTransform = ropeParts[i];
            var prevTransformId = ropeParts[i].GetComponent<RopeBehavior>() != null ? (System.Guid?)ropeParts[i].GetComponent<RopeBehavior>().id : null;
            var appliedRigid = prevTransform.GetComponent<RopeBehavior>() != null ? prevTransform.GetComponent<RopeBehavior>().receivingRigid : prevTransform.GetComponent<Rigidbody2D>();
            var positionalRigid = prevTransform.GetComponent<RopeBehavior>() != null ? prevTransform.GetComponent<RopeBehavior>().myRigid : prevTransform.GetComponent<Rigidbody2D>();
            conns.Add(new PrioritizedTransform() { priority = 1, transform = prevTransform, appliedRigid = appliedRigid, positionalRigid = positionalRigid, id = (prevTransformId!= null?(System.Guid)prevTransformId:System.Guid.NewGuid()) });

            var prevRopePart = prevTransform.GetComponent<RopeBehavior>();
            if (prevRopePart != null)
            {
                int nextPriority = 1;
                var lastPriority = prevRopePart.prioritizedConnections.OrderBy(x=>x.priority).LastOrDefault();
                if (lastPriority != null)
                    nextPriority = (int)(lastPriority.priority) + 1;

                prevRopePart.prioritizedConnections.Add(new PrioritizedTransform() { priority = nextPriority, transform = ropepart, appliedRigid = ropepart.GetComponent<RopeBehavior>().receivingRigid, positionalRigid = ropepart.GetComponent<RopeBehavior>().myRigid, id = ropepart.GetComponent<RopeBehavior>().id });
            }

            ropeParts.Add(ropepart);
        }

        foreach(var ropePart in ropeParts)
        {
            var ropeBehavior = ropePart.GetComponent<RopeBehavior>();
            if (ropeBehavior != null)
                ropeBehavior.initialized = true;
        }
	}
	
	// Update is called once per frame
	void Update () {

        //start at top, and check if next is further away.  if so, rolling weight gets set to that ropeparts weight.
        float rollingWeight = 0f;
	    for(int i = 0; i < ropeParts.Count-1; i++)
        {
            if ((ropeParts[i].transform.GetComponent<Rigidbody2D>() == null))
                continue;
            if((ropeParts[i].transform.GetComponent<Rigidbody2D>().position - ropeParts[i+1].transform.GetComponent<Rigidbody2D>().position).magnitude >= connectionDistance)
            {
                rollingWeight += ropeParts[i].GetComponent<Rigidbody2D>().mass;
                ropeParts[i + 1].GetComponent<RopeBehavior>().supportedMassBelow = rollingWeight;
            }
            else
            {
                ropeParts[i + 1].GetComponent<RopeBehavior>().supportedMassBelow = 0;
                rollingWeight = 0f;
            }
        }
        rollingWeight = 0;

        for (int i = ropeParts.Count-1; i > 0; i--)
        {
            if ((ropeParts[i - 1].transform.GetComponent<Rigidbody2D>() == null))
                continue;
            if ((ropeParts[i].transform.GetComponent<Rigidbody2D>().position - ropeParts[i - 1].transform.GetComponent<Rigidbody2D>().position).magnitude >= connectionDistance)
            {
                rollingWeight += ropeParts[i].GetComponent<Rigidbody2D>().mass;
                ropeParts[i - 1].GetComponent<RopeBehavior>().supportedMassAbove = rollingWeight;
            }
            else
            {
                ropeParts[i - 1].GetComponent<RopeBehavior>().supportedMassAbove = 0;
                rollingWeight = 0f;
            }
        }


    }
}
