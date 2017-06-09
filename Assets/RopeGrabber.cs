using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
using System.Linq;
public class RopeGrabber : MonoBehaviour
{
    [HideInInspector]
    private RopeBehavior2 AttachedRopePart;
    public RopeBehavior2 MyAttachedRopePart { get { return AttachedRopePart; } }
    private System.Guid? AttachmentId;
    public Rigidbody2D physics;
    public HingeJoint2D fixedJoint;
    public float climbSpeed;
    public RopeDeployer deployer;

    public bool hasGrabbed { get { return AttachedRopePart != null; } }

    // Use this for initialization
    void Start()
    {
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

        if(Input.GetAxis("Horizontal")!=0f || Input.GetAxis("Vertical") != 0f)
        {
            var isFlipped = transform.parent.localScale.x < 0f;
            var horiz = Input.GetAxis("Horizontal");
            var vert = Input.GetAxis("Vertical");
            if (isFlipped)
                horiz = -horiz;
            TryClimb(new Vector2(horiz, vert));
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("trigger enter");
        TryGrab(other);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("trigger stay");
        TryGrab(other);
    }

    void OnTrigger2D(Collider2D other)
    {
        Debug.Log("trigger leave");
        TryRelease(other);
    }

    private void TryRelease(Collider2D collider)
    {
        if (AttachedRopePart == null)
            return;

        var ropePart = collider.GetComponent<RopeBehavior>();
        if (ropePart == null)
            return;

        if (ropePart != AttachedRopePart)
            return;

        Release();
    }

    public void Release()
    {
        fixedJoint.enabled = false;
        AttachedRopePart = null;
        
        physics.GetComponent<PlatformerCharacter2D>().m_hanging = false;
        physics.gravityScale = physics.gravityScale * 4f;
    }

    

    public bool TryGrab(Collider2D collider)
    {
        Debug.Log("Try Grab");
        if (Input.GetKey(KeyCode.Space))
            return false;

        if (AttachedRopePart != null)
            return false;

        RopeBehavior2 ropePart = collider.transform.GetComponent<RopeBehavior2>();
        if (ropePart == null)
            return false;

        AttachedRopePart = ropePart;
        
        fixedJoint.connectedBody = AttachedRopePart.GetComponent<Rigidbody2D>();

        var connectedPosition = transform.localPosition;
        Debug.Log(transform.localPosition);
        Debug.Log(connectedPosition);
        fixedJoint.anchor =connectedPosition;
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

        var collider = transform.GetComponent<CircleCollider2D>();
        if (collider == null)
            return;

        var goalAnchor = fixedJoint.anchor - (direction.normalized * climbSpeed * Time.deltaTime);

        RaycastHit2D[] results = new RaycastHit2D[3];
        collider.Cast(Vector2.one, results, 0.0f);
        var resultList = new List<RaycastHit2D>(results);
        var inRange = resultList.Where(x => x.transform!=null && x.transform.GetComponent<RopeBehavior2>() != null
            && x.transform.GetComponent<RopeBehavior2>().GetInstanceID() == AttachedRopePart.GetInstanceID()).Count() > 0;

        RaycastHit2D[] physicsResults = new RaycastHit2D[1];
        var filter = (new ContactFilter2D()); filter.SetLayerMask(LayerMask.NameToLayer("Default")); 
        physics.Cast(-goalAnchor, filter ,physicsResults, goalAnchor.magnitude);
        var physicsResultsList = new List<RaycastHit2D>(physicsResults);
        var collisionFree = !physicsResultsList.Any(x => x.transform != null);   
        //only move if resulting position isn't out of reach
        if (inRange && collisionFree)
        {
            fixedJoint.anchor = goalAnchor;
        }
        else
        {
            //Try to find a a different rope is currently colliding.  if so, try to re attach to that rope.
            RaycastHit2D[] newResults = new RaycastHit2D[2];
            //resulting position is out of reach... try to find newer rope part
            collider.Cast(Vector2.one, newResults, 0.0f);

            var newResultList = new List<RaycastHit2D>(newResults);
            var candidate = newResultList.Where(x => x.transform != null && x.transform.GetComponent<RopeBehavior2>() != null && x.transform.GetComponent<RopeBehavior2>().GetInstanceID() != AttachedRopePart.GetInstanceID()).FirstOrDefault();
            if (candidate.transform != null && candidate.transform.GetComponent<Collider2D>() != null)
            {
                var previousAttachedRopePart = AttachedRopePart;
                Release();
                var grabbed = TryGrab(candidate.transform.GetComponent<Collider2D>());
                if (!grabbed)
                {
                    TryGrab(previousAttachedRopePart.GetComponent<Collider2D>());
                }
            }
            else
            {
                //last ditch effort, check if we have a deployer that is deploying...
                if (deployer.isDeploying)
                {
                    deployer.StartDeploy(AttachedRopePart.transform);
                }
            }
        }
    }
}
