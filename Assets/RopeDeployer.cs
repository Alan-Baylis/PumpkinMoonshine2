
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeDeployer : MonoBehaviour
{
    public Transform RopePart;
    public float RopePartDistance;
   
    public RopeGrabber holderGrabber;
    private Transform lastRope = null;
    public bool isDeploying { get{ return lastRope != null;} }
    
    public void StartDeploy(Transform anchor)
    {
        var initiallyHolding = holderGrabber.hasGrabbed;
        var grabbed = false;
        var minimumDistance = RopePart.GetComponent<CircleCollider2D>().radius;
        while (!grabbed)
        {
            addRopeTowardsHolder(anchor);
            var remainingDistance = ((Vector2)(lastRope.position - holderGrabber.transform.position)).magnitude;
            grabbed = remainingDistance < minimumDistance;
            //((Vector2)(lastRope.position - holderGrabber.transform.position)).magnitude < holderGrabber.GetComponent<CircleCollider2D>().radius + lastRope.GetComponent<CircleCollider2D>().radius;
            anchor = null;
        }
        
    }

    public void StartDeploy(Transform anchor, Vector2 goalPosition)
    { 
       addRopeTowardPosition(anchor, transform.position);   
    }



    public void StartConnect ( Transform anchor, Transform connectTo)
    {
        if (lastRope == null)
            lastRope = anchor;
        var connected = false;

        var minimumDistance = lastRope.GetComponent<CircleCollider2D>().radius;

        var count = 0;
        while (!connected)
        {
            count += 1;
            if(count % 20 == 0)
            {
                var whatevs = "break here";
                if (count > 100)
                    break;
            }

            addRopeTowardPosition(anchor, connectTo.position);
            var remainingDistance = ((Vector2)(lastRope.position - connectTo.position)).magnitude;
            connected = remainingDistance < minimumDistance;
                //((Vector2)(lastRope.position - goalConnection.position)).magnitude <= lastRope.GetComponent<CircleCollider2D>().radius + goalConnection.GetComponent<CircleCollider2D>().radius;
            anchor = null;
        }
        var hingerHinge = lastRope.gameObject.AddComponent<HingeJoint2D>();
        HingeTwoTransforms(lastRope, hingerHinge, connectTo);
    }


    void Update()
    {
        if (!Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.CapsLock))
            lastRope = null;

        if (Input.GetKey(KeyCode.Z) && holderGrabber.holdingEndOfRope)
        {
            TryDeployOneRope();
        }
        /*if (lastRope != null)
        {
            var grabbed =
                    ((Vector2)(lastRope.position - holderGrabber.transform.position)).magnitude <= RopePartDistance*2f;
            while (!grabbed)
            {
                addRopeTowardsHolder(null); //null continues deployment...
                grabbed =
                    ((Vector2)(lastRope.position - holderGrabber.transform.position)).magnitude <= RopePartDistance;
            }
        }*/

    }

    public void TryDeployOneRope()
    {
        StartDeploy(holderGrabber.MyAttachedRopePart.transform, transform.position);
        if (lastRope != null)
        {
            if (holderGrabber.hasGrabbed)
            {
                holderGrabber.Release();

                holderGrabber.TryGrab(lastRope.transform.GetComponent<Collider2D>());
            }
        }
    }

    public void HingeTwoTransforms(Transform hinger, HingeJoint2D hingerHinge, Transform hinged)
    {
        //hinger has the hinge, hinged gets connected to

        Rigidbody2D prevRigidBody = hinged.transform.GetComponent<Rigidbody2D>();
        Rigidbody2D newRigidBody = hinger.GetComponent<Rigidbody2D>();

        var newHinge = hingerHinge;
        newHinge.autoConfigureConnectedAnchor = false;
        newHinge.enableCollision = false;

        newHinge.connectedBody = prevRigidBody;

        newHinge.anchor = new Vector2((prevRigidBody.transform.position - newRigidBody.transform.position).x, (prevRigidBody.transform.position - newRigidBody.transform.position).y);

        newHinge.connectedAnchor = Vector2.zero;

        newHinge.breakForce = 999999f;
        newHinge.breakTorque = 999999f;
    }

    private bool addRopeTowardsHolder(Transform anchor)
    {
        var ropeTransform = Instantiate(RopePart) as Transform;
        
        if (anchor!= null)
            lastRope = anchor;

        
        var directionFromLastRope = (Vector2)(holderGrabber.transform.position - lastRope.transform.position).normalized;
        var goalPos = (Vector2)lastRope.transform.position + (directionFromLastRope * RopePartDistance * 2f);

        ropeTransform.position = goalPos;

        HingeTwoTransforms(ropeTransform, ropeTransform.GetComponent<HingeJoint2D>(), lastRope);

        /*Rigidbody2D prevRigidBody = lastRope.transform.GetComponent<Rigidbody2D>();
        Rigidbody2D newRigidBody = ropeTransform.GetComponent<Rigidbody2D>();

        var newHinge = ropeTransform.GetComponent<HingeJoint2D>();

        newHinge.connectedBody = prevRigidBody;

        newHinge.anchor = new Vector2((prevRigidBody.transform.position - newRigidBody.transform.position).x, (prevRigidBody.transform.position - newRigidBody.transform.position).y);

        newHinge.connectedAnchor = Vector2.zero;

        newHinge.breakForce = 999999f;
        newHinge.breakTorque = 999999f;*/

        lastRope = ropeTransform;

        //holderGrabber.Release();
        //var grabbed = holderGrabber.TryGrab(ropeTransform.GetComponent<Collider2D>());


        return false;
    }

    private bool addRopeTowardPosition(Transform anchor, Vector2 goalPosition)
    {
        var ropeTransform = Instantiate(RopePart) as Transform;

        if (anchor != null)
            lastRope = anchor;


        var directionFromLastRope = (goalPosition - (Vector2)lastRope.transform.position).normalized;
        var goalPos = (Vector2)lastRope.transform.position + (directionFromLastRope * RopePartDistance * 2f);

        ropeTransform.position = goalPos;

        HingeTwoTransforms(ropeTransform, ropeTransform.GetComponent<HingeJoint2D>(), lastRope);

        lastRope = ropeTransform;

        return false;
    }
}


