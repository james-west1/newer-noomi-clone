using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject head, torso, leftArm, rightArm, leftFemur, rightFemur, leftLowerLeg, rightLowerLeg, leftFoot, rightFoot, leftHand, rightHand, bar1, bar2, bar3; // get game objects from the scene
    HingeJoint leftBarJoint, rightBarJoint, leftShoulder, rightShoulder, neck, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle; // init hinge joints for each
    JointSpring leftShoulderSpring, rightShoulderSpring, neckSpring, leftHipSpring, rightHipSpring, leftKneeSpring, rightKneeSpring, leftAnkleSpring, rightAnkleSpring; // init joint springs for each joint

    public float strength; //spring strength, determines how strong noomi is
    public float damp; //spring damper, also plays into the strength factor

    bool onBar; // is the player currently on the bar or not?
    bool distanceThreshold; // did the player leave the bar?

    GameObject lastBarGrabbed; // need to store this to determine if the player is about to grab the same bar that they were just on
    // if so, need to use distance threshold

    // Start is called before the first frame update
    void Start()
    {
        initJoints();
        onBar = true;
        lastBarGrabbed = bar1;
        distanceThreshold = false;
        // no longer need to set max angular velocity higher because there is a way to do that in project settings now, much better
    }

    void initJoints() 
    {
        // declare joints
        leftBarJoint = leftArm.GetComponent<HingeJoint>();
        rightBarJoint = rightArm.GetComponent<HingeJoint>();
        leftShoulder = torso.GetComponents<HingeJoint>()[0];
        rightShoulder = torso.GetComponents<HingeJoint>()[1];
        neck = head.GetComponent<HingeJoint>();
        leftHip = leftFemur.GetComponent<HingeJoint>();
        rightHip = rightFemur.GetComponent<HingeJoint>();
        leftKnee = leftLowerLeg.GetComponent<HingeJoint>();
        rightKnee = rightLowerLeg.GetComponent<HingeJoint>();
        leftAnkle = leftFoot.GetComponent<HingeJoint>();
        rightAnkle = rightFoot.GetComponent<HingeJoint>();

        //declare joint springs
        leftShoulderSpring = leftShoulder.spring;
        rightShoulderSpring = rightShoulder.spring;
        neckSpring = neck.spring;
        leftHipSpring = leftHip.spring;
        rightHipSpring = rightHip.spring;
        leftKneeSpring = leftKnee.spring;
        rightKneeSpring = rightKnee.spring;
        leftAnkleSpring = leftAnkle.spring;
        rightAnkleSpring = rightAnkle.spring;

        //set spring damper because that never really needs to change, as far as i know lol
        leftShoulderSpring.damper = damp;
        rightShoulderSpring.damper = damp;
        neckSpring.damper = damp;
        leftHipSpring.damper = damp;
        rightHipSpring.damper = damp;
        leftKneeSpring.damper = damp;
        rightKneeSpring.damper = damp;
        leftAnkleSpring.damper = damp;
        rightAnkleSpring.damper = damp;
    }

    // sets body position of player by setting target positions and strengths of all joints
    void setBodyPosition(float shoulderAngle, float shoulderStrength, float neckAngle, float neckStrength, float hipAngle, float hipStrength, float kneeAngle, float kneeStrength, float ankleAngle, float ankleStrength)
    {
        leftShoulderSpring.targetPosition = shoulderAngle;
        leftShoulderSpring.spring = shoulderStrength;
        leftShoulder.spring = leftShoulderSpring;
        rightShoulderSpring.targetPosition = shoulderAngle;
        rightShoulderSpring.spring = shoulderStrength;
        rightShoulder.spring = rightShoulderSpring;

        neckSpring.targetPosition = neckAngle;
        neckSpring.spring = neckStrength;
        neck.spring = neckSpring;

        leftHipSpring.targetPosition = hipAngle;
        leftHipSpring.spring = hipStrength;
        leftHip.spring = leftHipSpring;
        rightHipSpring.targetPosition = hipAngle;
        rightHipSpring.spring = hipStrength;
        rightHip.spring = rightHipSpring;

        leftKneeSpring.targetPosition = kneeAngle;
        leftKneeSpring.spring = kneeStrength;
        leftKnee.spring = leftKneeSpring;
        rightKneeSpring.targetPosition = kneeAngle;
        rightKneeSpring.spring = kneeStrength;
        rightKnee.spring = rightKneeSpring;

        leftAnkleSpring.targetPosition = ankleAngle;
        leftAnkleSpring.spring = ankleStrength;
        leftAnkle.spring = leftAnkleSpring;
        rightAnkleSpring.targetPosition = ankleAngle;
        rightAnkleSpring.spring = ankleStrength;
        rightAnkle.spring = rightAnkleSpring;
    }

    void checkRegrabs()
    {
        foreach (GameObject bar in new GameObject[] { bar1, bar2, bar3 })
        {
            float leftDistance = Vector3.Distance(bar.transform.position, leftHand.transform.position);
            float rightDistance = Vector3.Distance(bar.transform.position, rightHand.transform.position);

            if (bar == lastBarGrabbed) 
            {
                // only allow regrab if player is past distance threshold
                if (leftDistance > 1 && rightDistance > 1) // if the player has actually left the bar
                {
                    distanceThreshold = true;
                }

                if (distanceThreshold && leftDistance < 0.35 && rightDistance < 0.35)
                {
                    regrab(bar);
                }
            } else
            {
                if (leftDistance < 0.35 && rightDistance < 0.35)
                {
                    regrab(bar);
                }
            }
        }
    }

    void regrab(GameObject bar)
    {
        // ignore collisions between arms and bar, otherwise it just doesn't work lol
        Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), bar.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), bar.GetComponent<Collider>(), true);

        // create joints
        leftBarJoint = leftArm.AddComponent<HingeJoint>();
        rightBarJoint = rightArm.AddComponent<HingeJoint>();
        leftBarJoint.anchor = new Vector3(0, 1, 0);
        rightBarJoint.anchor = new Vector3(0, 1, 0);
        leftBarJoint.axis = new Vector3(0, 0, 1);
        rightBarJoint.axis = new Vector3(0, 0, 1);

        // move to proper spot
        leftBarJoint.autoConfigureConnectedAnchor = false; // no im doing it myself bitch
        rightBarJoint.autoConfigureConnectedAnchor = false;
        leftBarJoint.connectedAnchor = new Vector3(bar.transform.position.x, bar.transform.position.y, 0.3f);
        rightBarJoint.connectedAnchor = new Vector3(bar.transform.position.x, bar.transform.position.y, -0.3f);

        // reset variables
        onBar = true;
        distanceThreshold = false;
        lastBarGrabbed = bar;
    }

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {
        if (!onBar) // if in the air, check potential regrabs
        {
            checkRegrabs();
        }

        if (Input.GetKey(KeyCode.R)) {
            // load scene again to reset
            EditorSceneManager.LoadScene("SampleScene");
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            // break joints to let go
            Destroy(leftBarJoint);
            Destroy(rightBarJoint);
            onBar = false;

            // want collisions between arms and bar to work again
            Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);
            Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // arch
            if (onBar)
            {
                // weaker arch
                setBodyPosition(-20, strength, 30, strength * 2, -20, strength, -30, strength * 2, -30, strength);
            } else
            {
                // stronger arch so he can do wall flips and shit
                setBodyPosition(-20, strength, 30, strength * 2, -20, strength * 3, -30, strength * 2, -30, strength);
            }
        }
        else if (Input.GetKey(KeyCode.Space) && !onBar) // since tucking makes your hips stronger, don't want to do it on bar
        {
            // tuck
            setBodyPosition(160, strength, -80, strength * 2, 150, strength * 3, -120, strength * 2, 30, strength);
        } 
        else
        {
            // default
            setBodyPosition(150, strength, 0, strength * 2, 120, strength, 0, strength * 2, 30, strength);
        }
        
    }
}
