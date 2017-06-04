using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;
public class RopeGrabber : MonoBehaviour
{
    [HideInInspector]
    private RopeBehavior2 AttachedRopePart;
    private System.Guid? AttachmentId;
    public Rigidbody2D physics;

    public HingeJoint2D fixedJoint;

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

    private void Release()
    {
        fixedJoint.enabled = false;
        AttachedRopePart = null;
        
        physics.GetComponent<PlatformerCharacter2D>().m_hanging = false;
        physics.gravityScale = physics.gravityScale * 2f;
    }

    private void TryGrab(Collider2D collider)
    {
        Debug.Log("Try Grab");
        if (Input.GetKey(KeyCode.Space))
            return;

        if (AttachedRopePart != null)
            return;

        RopeBehavior2 ropePart = collider.transform.GetComponent<RopeBehavior2>();
        if (ropePart == null)
            return;

        AttachedRopePart = ropePart;
        
        fixedJoint.connectedBody = AttachedRopePart.GetComponent<Rigidbody2D>();

        var connectedPosition = transform.localPosition;
        Debug.Log(transform.localPosition);
        Debug.Log(connectedPosition);
        fixedJoint.anchor =connectedPosition;
        fixedJoint.connectedAnchor = Vector2.zero;

        physics.GetComponent<PlatformerCharacter2D>().m_hanging = true;
        physics.gravityScale = physics.gravityScale / 2f;

        fixedJoint.enabled = true;
    }
}
