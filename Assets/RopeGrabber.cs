using System.Collections;
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

    public bool hasGrabbed { get { return AttachedRopePart != null; } }

    private List<Collider2D> grabbables;

    private Vector2 currentDirection = Vector2.zero;

    private bool shouldClimb = false;

    // Use this for initialization
    void Start()
    {
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
        
        if(AttachedRopePart != null && (Input.GetAxis("Horizontal")!=0f || Input.GetAxis("Vertical") != 0f))
        {
            var isFlipped = transform.parent.localScale.x < 0f;
            var horiz = Input.GetAxis("Horizontal");
            var vert = Input.GetAxis("Vertical");
            if (isFlipped)
                horiz = -horiz;
            currentDirection = new Vector2(horiz, vert);
            if (shouldClimb == false)
            {
                Debug.Log("Starting Climb at " + System.DateTime.Now.ToString());
                StartCoroutine("ClimbSequence");
                shouldClimb = true;
            }    
        }
        else
        {
            shouldClimb = false;
            StopCoroutine("ClimbSequence");
        }
    }
    

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("trigger enter");
        if (other.GetComponent<RopeBehavior2>() == null)
            return;

        grabbables.Add(other);
        if(AttachedRopePart==null)
            TryGrab(other);
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
        physics.gravityScale = physics.gravityScale * 4f;
    }

    public Collider2D getClosestCollider(bool ignoreAttached = false)
    {
        return getClosestCollider(Vector2.zero, ignoreAttached);
    }
    public Collider2D getClosestCollider(Vector2 relativePoint, bool ignoreAttached = true)
    {
        var point = (Vector2)transform.position + relativePoint;

        var connectedRopeParts = grabbables.Where(x => 
            RopePartsAreConnected(x.GetComponent<RopeBehavior2>(), AttachedRopePart.GetComponent<RopeBehavior2>()) 
            || AttachedRopePart.transform.GetInstanceID() == x.transform.GetInstanceID()
        );

        if (ignoreAttached && AttachedRopePart != null)
            return grabbables.Where(x => x.GetInstanceID() != AttachedRopePart.transform.GetComponent<Collider2D>().GetInstanceID())
                .OrderBy(x => ((Vector2)x.transform.position - point).magnitude).FirstOrDefault();
        else if (ignoreAttached && PreviouslyAttachedRopePart != null)
            return grabbables.Where(x => x.GetInstanceID() != PreviouslyAttachedRopePart.transform.GetComponent<Collider2D>().GetInstanceID())
                .OrderBy(x => ((Vector2)x.transform.position - point).magnitude).FirstOrDefault();
        else
            return grabbables
               .OrderBy(x => ((Vector2)x.transform.position - point).magnitude).FirstOrDefault();
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
        physics.gravityScale = physics.gravityScale / 4f;

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

        TryRelease();
        TryGrab(closestCollider);
    }

    IEnumerator ClimbSequence()
    {
        while (true)
        {
            TryClimb(currentDirection);
            yield return new WaitForSeconds(.5f);
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
}
