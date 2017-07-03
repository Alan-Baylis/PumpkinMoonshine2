﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using System.Linq;
public class RopeGrabber : MonoBehaviour
{
    [HideInInspector]
    private RopeBehavior2 AttachedRopePart;
    private RopeBehavior2 PreviouslyAttachedRopePart;
    public RopeBehavior2 MyAttachedRopePart { get { return AttachedRopePart; } }
    private System.Guid? AttachmentId;
    public Rigidbody2D physics;
    public SpringJoint2D fixedJoint;
    public float climbSpeed;
    public RopeDeployer deployer;
    public bool holdingEndOfRope { get { return AtEndOfRope(); } }

    public bool hasGrabbed { get { return AttachedRopePart != null; } }
    public bool hasTaughtRope { get { return IsRopeTaught(); } }

    private List<Collider2D> grabbables;

    private Vector2 currentDirection = Vector2.zero;

    private bool shouldClimb = false;
    private Vector2 GrabberBasePosition;
    // Use this for initialization
    void Start()
    {
        GrabberBasePosition = (Vector2)(transform.position - physics.transform.position);
        grabbables = new List<Collider2D>();
        AttachedRopePart = null;
        AttachmentId = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && AttachedRopePart!=null)
        {
            Release();
        }

        //climbing
        if (AttachedRopePart != null && (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
             && (!physics.GetComponent<PlatformerCharacter2D>().isGrounded || (physics.GetComponent<PlatformerCharacter2D>().isGrounded && Input.GetKey(KeyCode.X)))
           )
        {
            var isFlipped = transform.parent.localScale.x < 0f;
            var horiz = Input.GetAxis("Horizontal");
            var vert = Input.GetAxis("Vertical");
            /*if (isFlipped)*/
                horiz = -horiz;
            currentDirection = new Vector2(-horiz, vert);
            if (shouldClimb == false)
            {
                Debug.Log("Starting Climb at " + System.DateTime.Now.ToString());
                StartCoroutine("ClimbSequence");
                shouldClimb = true;
            }

            if(!isFlipped)
                moveGrabbingAnchor( (Vector2)(new Vector2(horiz, -vert).normalized * .3f) + GrabberBasePosition);    
            else
                moveGrabbingAnchor((Vector2)(new Vector2(-horiz, -vert).normalized * .3f) + GrabberBasePosition);    
            
        }
        else
        {
            moveGrabbingAnchor(Vector2.zero + GrabberBasePosition);
            shouldClimb = false;
            StopCoroutine("ClimbSequence");
        }


        //grabbing
        if (!hasGrabbed && grabbables.Count() != 0)
        {
            var closest = getClosestCollider(Vector2.zero, true);
            TryGrab(closest);
        }
        ////holding loosely
        //if(AttachedRopePart != null && (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
        //     && physics.GetComponent<PlatformerCharacter2D>().isGrounded)
        //{
        //    var wasGrabbing = AttachedRopePart;
        //    TryRelease();
        //   /* TryGrab(null);
        //    if (AttachedRopePart == null)
        //        TryGrab(wasGrabbing.GetComponent<Collider2D>());*/
        //}
    }
    
    private bool IsRopeTaught()
    {
        return AttachedRopePart != null && ((Vector2)(AttachedRopePart.transform.position - transform.position)).magnitude > .1f;
    }


    void FixedUpdate()
    {
        //StartCoroutine("LateFixedUpdate");
    }
    IEnumerator LateFixedUpdate()
    {
        yield return new WaitForFixedUpdate();

        if(!hasGrabbed && grabbables.Count() != 0)
        {
            var closest = getClosestCollider(Vector2.zero, true);
            TryGrab(closest);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("trigger enter");
        if (other.GetComponent<RopeBehavior2>() == null)
            return;

        grabbables.Add(other);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.GetComponent<RopeBehavior2>()!=null)
            grabbables = grabbables.Where(x => x.GetInstanceID() != other.GetInstanceID()).ToList();
    }
    

    private void TryRelease()
    {
        if (AttachedRopePart == null)
            return;

        Release();
    }

    public void Release()
    {
        fixedJoint.enabled = false;
        PreviouslyAttachedRopePart = AttachedRopePart;
        AttachedRopePart = null;
        
        physics.GetComponent<PlatformerCharacter2D>().m_hanging = false;
        //physics.gravityScale = physics.gravityScale * 4f;
    }

    public Collider2D getClosestCollider(bool ignoreAttached = false)
    {
        return getClosestCollider(Vector2.zero, ignoreAttached);
    }
    public Collider2D getClosestCollider(Vector2 relativePoint, bool ignoreAttached = true)
    {
        var point = (Vector2)transform.position + relativePoint;

        IEnumerable<Collider2D> connectedRopeParts;
        if (ignoreAttached && AttachedRopePart != null)
            connectedRopeParts = grabbables.Where(x => x.GetComponent<RopeBehavior2>() != null && AttachedRopePart.GetComponent<RopeBehavior2>()!= null
                && RopePartsAreConnected(x.GetComponent<RopeBehavior2>(), AttachedRopePart.GetComponent<RopeBehavior2>())
                || AttachedRopePart.transform.GetInstanceID() == x.transform.GetInstanceID()
            );
        else
            connectedRopeParts = grabbables;


        Collider2D chosenCollider = null;
        if (ignoreAttached && AttachedRopePart != null)
            chosenCollider = connectedRopeParts
                .Where(
                  x => x.GetInstanceID() != AttachedRopePart.transform.GetComponent<Collider2D>().GetInstanceID()
                  && Vector2.Dot(relativePoint.normalized, (Vector2)(x.transform.position - transform.position).normalized) > .5f
                )
                .OrderByDescending(x => (
                  Vector2.Dot(relativePoint.normalized, (Vector2)(x.transform.position - transform.position).normalized)
                )
              ).FirstOrDefault();
        else
            chosenCollider = connectedRopeParts
               .OrderBy(x => ((Vector2)x.transform.position - point).magnitude).FirstOrDefault();

        return chosenCollider;
    }

    public bool AtEndOfRope()
    {
        if (AttachedRopePart == null)
            return false;

        var connectedRopeParts = RopeBehavior2.All.Where(x => x.GetComponent<RopeBehavior2>() != null && AttachedRopePart.GetComponent<RopeBehavior2>() != null
            && RopePartsAreConnected(x.GetComponent<RopeBehavior2>(), AttachedRopePart.GetComponent<RopeBehavior2>())
        );

        return connectedRopeParts.Count() <= 1; 
    }

    public bool TryGrab(Collider2D collider)
    {
        
        if (Input.GetKey(KeyCode.Space))
            return false;

        if (AttachedRopePart != null)
            return false;

        if (grabbables.Count() == 0)
            return false;

        Debug.Log("Try Grab");

        
        //get collider for grabbing//
        Collider2D goalRopePart;
        if (collider == null)
        {
            var nearestRopePart = getClosestCollider(true);
            goalRopePart = nearestRopePart;
        }
        else
            goalRopePart = collider;
        if (collider == null)
            return false;
        RopeBehavior2 ropePart = goalRopePart.transform.GetComponent<RopeBehavior2>();//collider.transform.GetComponent<RopeBehavior2>();
        if (ropePart == null)
            return false;

        Debug.Log("Should Grab");

        AttachedRopePart = ropePart;
        fixedJoint.connectedBody = AttachedRopePart.GetComponent<Rigidbody2D>();

        var connectedPosition = (Vector2)fixedJoint.transform.InverseTransformPoint(transform.position);
        fixedJoint.anchor = connectedPosition;
        fixedJoint.connectedAnchor = Vector2.zero;

        physics.GetComponent<PlatformerCharacter2D>().m_hanging = true;
        //physics.gravityScale = physics.gravityScale / 4f;

        fixedJoint.enabled = true;

        return true;
    }

    private void TryClimb(Vector2 direction)
    {
        if (AttachedRopePart == null)
            return;

        var relativePoint = direction.normalized * transform.GetComponent<CircleCollider2D>().radius;

        Debug.Log("Relative point:" + relativePoint.ToString());

        var closestCollider = getClosestCollider(relativePoint, true);

        //test
        /*if (closestCollider == null && Input.GetKey(KeyCode.LeftShift) && AtEndOfRope())
            //deploy more rope
            deployer.StartDeploy(AttachedRopePart.transform);
        else {*/
        //climb
        if (closestCollider != null) { 
            TryRelease();
            TryGrab(closestCollider);
        }
        //}
    }

    IEnumerator ClimbSequence()
    {
        while (true)
        {
            TryClimb(currentDirection);
            yield return new WaitForSeconds(.5f);
        }
    }

    IEnumerator HoldLoosely()
    {
        var wasGrabbed = AttachedRopePart;
        while (true)
        {
            if(AttachedRopePart == null)
            {
                TryGrab(null);
                if (AttachedRopePart == null)
                    TryGrab(wasGrabbed.transform.GetComponent<Collider2D>());
            }
            else
            {
                Release();
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private bool RopePartsAreConnected(RopeBehavior2 part1, RopeBehavior2 part2)
    {
        return RopePartConnectedToRopePart(part1, part2) || RopePartConnectedToRopePart(part2, part1);
    }
    private bool RopePartConnectedToRopePart(RopeBehavior2 part1, RopeBehavior2 part2)
    {
        var hingeJoint = part1.transform.GetComponent<HingeJoint2D>();
        if (hingeJoint == null)
            return false;

        var rigid = part2.GetComponent<Rigidbody2D>();
        if (rigid == null)
            return false;

        if (hingeJoint.connectedBody.GetInstanceID() == rigid.GetInstanceID())
            return true;

        return false;
    }

    private void moveGrabbingAnchor(Vector2 positionRelativeToPhysics)
    {
        if (positionRelativeToPhysics != Vector2.zero)
        {
            var whatever = "shamy";
        }
        var speedCoeffecient = .025f;
        
        var currentPos = physics.GetComponent<SpringJoint2D>().anchor;
        var newPos = currentPos + ((Vector2)positionRelativeToPhysics-currentPos) * speedCoeffecient;

        var worldNewPoint = (Vector2)physics.transform.TransformPoint(newPos);
        var ropePosition = worldNewPoint;
        if (AttachedRopePart != null)
            ropePosition = (Vector2)AttachedRopePart.transform.position;

        //don't move if attached rope part is outside center of grabber
        /*if (positionRelativeToPhysics != Vector2.zero && AttachedRopePart != null && 
            (worldNewPoint - ropePosition).magnitude > transform.GetComponent<CircleCollider2D>().radius * .9f)
            return;*/

        physics.GetComponent<SpringJoint2D>().anchor = newPos;
        transform.localPosition = currentPos;
    }
}
