using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHuman : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    private Animator animatorBody; 

    // Start is called before the first frame update
    void Start()
    {
        animatorBody = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var velocity = Vector3.forward * Input.GetAxis("Vertical") * speed;
        transform.Translate(velocity * Time.deltaTime);
        transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * Time.deltaTime * turnSpeed);
        animatorBody.SetFloat("speed", velocity.z);
        PlayAction();
        PlayReaction();
    }

    void PlayAction()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            animatorBody.SetInteger("ActionIndex", Random.Range(0, 6));
            animatorBody.SetTrigger("Action");
        }
    }

    void PlayReaction()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            animatorBody.SetInteger("ReactionIndex", Random.Range(0, 3));
            animatorBody.SetTrigger("Reaction");
        }
    }
}
