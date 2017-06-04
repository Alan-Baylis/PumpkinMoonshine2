using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class RopeBehavior : MonoBehaviour {
    [SerializeField]
    public List<PrioritizedTransform> prioritizedConnections;
    public Rigidbody2D myRigid;
    public Rigidbody2D receivingRigid;
    public float connectionDistance = .1f;
    public float breakDistance = .5f;
    public bool initialized = false;
    public float supportedMassBelow = 0f;
    public float supportedMassAbove = 0f;
    public Vector2 previousImpulseForce;
   
    [HideInInspector]
    public System.Guid id = System.Guid.NewGuid();

    void Start()
    {
        previousImpulseForce = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (!initialized)
            return;

        var rigid = myRigid;
        Vector2 overallForce = new Vector2();


        foreach (var prioritizedConnection in prioritizedConnections.OrderByDescending(x => x.priority))
        {
            if(prioritizedConnection.transform.GetComponent<RopeGrabber>() != null)
            {
                var something = "nothing";
            }

            var connectionTransform = prioritizedConnection.transform;
            
            Vector2 connectionPosition;
            float connectionMass;

            var connRigid2d = prioritizedConnection.positionalRigid;
            var physicsRigid2d = prioritizedConnection.appliedRigid;
            if(connRigid2d == null)
                connRigid2d = prioritizedConnection.transform.GetComponent<Rigidbody2D>();
            if (connRigid2d != null)
                connectionPosition = connRigid2d.position;
            else
                connectionPosition = new Vector3(prioritizedConnection.transform.position.x, prioritizedConnection.transform.position.y, 0);
            if (physicsRigid2d != null)
                connectionMass = physicsRigid2d.mass;
            else
                connectionMass = 1f;

            
            var relativePosition = (connectionPosition - rigid.position);
            var distance = Mathf.Abs(relativePosition.magnitude);
            var preexistingVelocity = rigid.velocity + (overallForce) + (((supportedMassBelow+supportedMassAbove)/2f)*-relativePosition);
            var normalForce = Vector2.Dot(
                   preexistingVelocity
                   , -relativePosition);
            
            var elasticityForce = ((distance-connectionDistance)*(connectionMass/receivingRigid.mass))/Time.fixedDeltaTime;
            if (distance >= connectionDistance)
            {
                var forcetotal = normalForce > 0f ? normalForce + elasticityForce : elasticityForce;
                overallForce += forcetotal*relativePosition.normalized;
            }
            if(distance >= breakDistance)
            {
                var ropePart = prioritizedConnection.transform.GetComponent<RopeBehavior>();
                if(ropePart != null)
                {
                    //Break!
                    ropePart.Detach(id);
                    Detach(ropePart.id);
                    continue;
                }
            }
        }

        previousImpulseForce = overallForce * receivingRigid.mass;
        receivingRigid.AddForce(overallForce * receivingRigid.mass , ForceMode2D.Impulse);
    }

    public PrioritizedTransform getPrioritizedTransform(bool IsPriority)
    {
        return new PrioritizedTransform
        {
            id = id,
            transform = transform,
            positionalRigid = myRigid,
            appliedRigid = receivingRigid,
            priority = IsPriority ? 900 : 0
        };
    }

    //do detach!!
    public bool Detach(System.Guid guid)
    {
        var count = prioritizedConnections.Count();
        var removableConnection = prioritizedConnections.Where(x => x.id == guid).FirstOrDefault();
        if (removableConnection != null)
        {
            prioritizedConnections.Remove(removableConnection);
        }

        return count != prioritizedConnections.Count();
    }

    public bool Detach(PrioritizedTransform prioritizedTransform)
    {
        return Detach(prioritizedTransform.id);
    }

    public System.Guid Attach(Transform t, Rigidbody2D positionalRigid, Rigidbody2D appliedRigid, bool IsPriority)
    {
        
        var _priority = IsPriority ? 900 : -1;
        var prioritizedTransform = new PrioritizedTransform
        {
            priority = _priority,
            transform = t,
            appliedRigid = appliedRigid,
            positionalRigid = positionalRigid
        };

        prioritizedConnections.Add(prioritizedTransform);

        return prioritizedTransform.id;
    }
    
    public System.Guid Attach(PrioritizedTransform prioritizedTransform)
    {
        prioritizedConnections.Add(prioritizedTransform);
        return prioritizedTransform.id;
    }
}




[System.Serializable]
public class PrioritizedTransform : System.IEquatable<PrioritizedTransform>
{
    public System.Guid id = System.Guid.NewGuid();
    public int priority;
    public Transform transform;
    public Rigidbody2D appliedRigid;
    public Rigidbody2D positionalRigid;

    public bool Equals(PrioritizedTransform other)
    {
        return (this.id == other.id && this.id != null);
    }

}
