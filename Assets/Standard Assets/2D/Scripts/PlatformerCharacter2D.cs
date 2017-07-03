using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f; // The fastest the player can travel in the x axis.
        [SerializeField] private float m_MaxSpeedSecondary = .5f;
        [SerializeField]
        private float m_MaxSlope = 60f;
        [SerializeField] private float m_MoveForce = 40f;
        [SerializeField] private float m_MoveForceSecondary = 5f;
        [SerializeField] private float m_JumpForce = 400f;
        [SerializeField] private float m_FloatForce = 100f;// Amount of force added when the player jumps.
        [SerializeField] private float m_FrictionCoefficient = 6f;
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        public Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
        public bool m_hanging = false;
        public bool isGrounded { get { return m_Grounded; } }

        //debug
        Vector2 _start = Vector2.zero;
        Vector2 _direction = Vector2.zero;

        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            //m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }


        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.


            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    m_Grounded = true;
                    //Debug.Log("grounded");
                }
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
        }


        public void Move(float move, bool crouch, bool jump, bool jumpHold)
        {
            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if ( (m_Grounded || (m_AirControl)&& !m_hanging))
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move*m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

                var maxSpeed = m_MaxSpeed;
                //if (m_hanging)
                    //maxSpeed = m_MaxSpeedSecondary;
                var moveForce = m_MoveForce;
                //if (m_hanging)
                    //moveForce = m_MoveForceSecondary;

                // Move the character if not maxspeed in that direction
                if (move != 0f && m_Rigidbody2D.velocity.x * Mathf.Sign(move) < m_MaxSpeed && !Input.GetKey(KeyCode.X))
                {
                    //Debug.Log("Adding Movement force.");
                    
                    var moveDir = DetermineMoveDir(move);

                    m_Rigidbody2D.AddForce(moveDir * move * moveForce);//new Vector2(move *10f * m_MaxSpeed, 0f));
                    if(m_Rigidbody2D.velocity.x > m_MaxSpeed)
                    {
                        var catchtis = "marfwr";
                    }
                }
                //slow the character down
                if (ShouldDecreaseSpeed(maxSpeed, move))
                {
                    //Debug.Log("Adding friction force.");
                    m_Rigidbody2D.AddForce(new Vector2(-m_Rigidbody2D.velocity.x*m_FrictionCoefficient, 0f));
                }


                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight && !m_hanging)
                {
                    // ... flip the player.
                    Flip();
                }
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight && !m_hanging)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground"))
            {
                //Debug.Log("Adding jump force.");
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
            //Debug.Log("jumphold: " + jumpHold.ToString() + "; !jump:" + (!jump).ToString() + ";  !mgrounded: " + (!m_Grounded).ToString());
            // If the player is holding the jump button...
            if (jumpHold && !jump && canFloat())
            {
                //Debug.Log("Adding float force.");
                //add the float force.
                m_Rigidbody2D.AddForce(new Vector2(0f, m_FloatForce));
            }

            
        }
        private bool ShouldDecreaseSpeed(float maxSpeed, float move)
        {
            if (!m_AirControl || !m_Grounded)
                return false;
            var decreaseDueToGroundSpeed = m_Grounded
                            && m_Rigidbody2D.velocity.magnitude > maxSpeed;
            var decreaseDueToNotMoving = (move == 0);

            var decreaseDueToAirSpeed = (
                m_AirControl
                && m_Rigidbody2D.velocity.x * Mathf.Sign(maxSpeed) > m_MaxSpeed
            );

            return decreaseDueToAirSpeed || decreaseDueToGroundSpeed || decreaseDueToNotMoving;  
        }
        private Vector2 DetermineMoveDir(float move)
        {
            if (!m_Grounded)
            {
                return Vector2.right;
            }

            RaycastHit2D[] hits = new RaycastHit2D[1];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(m_WhatIsGround);
            
            var degreeIteration = 0f;
            float degreeAddition = 0f;
            if (move > 0f)
                degreeAddition = 1f;
            else
                degreeAddition = -1f;
            Vector2 origin = (Vector2)m_GroundCheck.position + (Vector2.down * .05f);
            var checkLength = k_GroundedRadius *4f;
            Vector2 direction = Vector2.zero;
            do
            {
                hits = new RaycastHit2D[1];
                direction = Quaternion.Euler(new Vector3(0, 0, degreeIteration)) * (Vector2.right * Mathf.Sign(move)).normalized;
                var end = origin + (direction * checkLength);
                Physics2D.Linecast(origin, origin + (direction * checkLength), filter, hits);
                //transform.GetComponent<CircleCollider2D>().Cast(direction, filter, hits, .2f);
                degreeIteration += degreeAddition;
            } while (hits[0].transform != null && Math.Abs(degreeIteration) < m_MaxSlope);
            if(hits[0].transform != null)
            {
                return Vector2.zero;
            }
            _start = origin;
            _direction = direction*checkLength;

            direction = new Vector2(direction.x * Mathf.Sign(move), direction.y * Mathf.Sign(move));


            //only x should be negative depending on move
            return direction;
        }
        public void OnDrawGizmos()
        {
            var oldColor = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_start, _start + (_direction));
            Gizmos.color = oldColor;
        }

        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        public bool canFloat()
        {
            return m_Rigidbody2D.velocity.y >= 0f;
        }
    }
}
