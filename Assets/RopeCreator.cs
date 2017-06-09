using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeCreator : MonoBehaviour {

    public Transform RopePart;
    public float RopePartDistance;
    public int NumberOfRopeParts;
    public RopeGrabber holderGrabber;
    public List<Transform> ropes;
    void Start()
    {
        /*var grabbed = false;
        while (!grabbed)
        {
            addRopeTowardsHolder();
            grabbed =
                ((Vector2)(ropes.Last().position - holderGrabber.transform.position)).magnitude <= RopePartDistance;
        }*/
    }

    void Update()
    {
        
    }

    private void addHangingRopePart()
    {
        
        var ropeTransform = Instantiate(RopePart) as Transform;
        ropeTransform.position = new Vector2(transform.position.x, transform.position.y - ((ropes.Count() + 1) * (RopePartDistance*2f)));


        Rigidbody2D prevRigidBody = null;
        if (ropes.Count() == 0)
            prevRigidBody = transform.GetComponent<Rigidbody2D>();
        else
            prevRigidBody = ropes[ropes.Count - 1].GetComponent<Rigidbody2D>();
        Rigidbody2D newRigidBody = ropeTransform.GetComponent<Rigidbody2D>();

        var newHinge = ropeTransform.GetComponent<HingeJoint2D>();

        newHinge.connectedBody = prevRigidBody;

        newHinge.anchor = new Vector2((prevRigidBody.transform.position - newRigidBody.transform.position).x, (prevRigidBody.transform.position - newRigidBody.transform.position).y);

        newHinge.connectedAnchor = Vector2.zero;

        newHinge.breakForce = 999999f;
        newHinge.breakTorque = 999999f;

        ropes.Add(ropeTransform);
        
    }

    private bool addRopeTowardsHolder()
    {
        var ropeTransform = Instantiate(RopePart) as Transform;
        Vector2 ropePartGoalPos;
        Transform lastRope;
        if (ropes.Count() == 0)
            lastRope = transform;
        else
            lastRope = ropes.Last();

        var directionFromLastRope = (Vector2)(holderGrabber.transform.position - lastRope.transform.position).normalized;
        var goalPos = (Vector2)lastRope.transform.position + (directionFromLastRope * RopePartDistance*2f);

        ropeTransform.position = goalPos;

        Rigidbody2D prevRigidBody = lastRope.transform.GetComponent<Rigidbody2D>();
        Rigidbody2D newRigidBody = ropeTransform.GetComponent<Rigidbody2D>();

        var newHinge = ropeTransform.GetComponent<HingeJoint2D>();

        newHinge.connectedBody = prevRigidBody;

        newHinge.anchor = new Vector2((prevRigidBody.transform.position - newRigidBody.transform.position).x, (prevRigidBody.transform.position - newRigidBody.transform.position).y);

        newHinge.connectedAnchor = Vector2.zero;

        newHinge.breakForce = 999999f;
        newHinge.breakTorque = 999999f;

        ropes.Add(ropeTransform);

        //holderGrabber.Release();
        //var grabbed = holderGrabber.TryGrab(ropeTransform.GetComponent<Collider2D>());


        return false;
    }
}
