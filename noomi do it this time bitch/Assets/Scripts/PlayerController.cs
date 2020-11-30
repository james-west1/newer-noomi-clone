// arms sometimes stay in arch position when you try to tuck, idk why this is
// he can't jump off the ground very hard anymore :( sad

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject head, torso, leftArm, rightArm, leftFemur, rightFemur, leftLowerLeg, rightLowerLeg, leftFoot, rightFoot; // get game objects from the scene
    HingeJoint leftBarJoint, rightBarJoint, leftShoulder, rightShoulder, neck, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle; // init hinge joints for each
    JointSpring leftShoulderSpring, rightShoulderSpring, neckSpring, leftHipSpring, rightHipSpring, leftKneeSpring, rightKneeSpring, leftAnkleSpring, rightAnkleSpring; // init joint springs for each joint

    public float strength; //spring strength, determines how strong noomi is
    public float damp; //spring damper, also plays into the strength factor

    bool onBar; // is the player currently on the bar or not?

    // Start is called before the first frame update
    void Start()
    {
        
        initJoints();
        onBar = true;
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

    // FixedUpdate is called once every physics frame
    void FixedUpdate()
    {
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
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // arch
            setBodyPosition(-20, strength, 30, strength * 2, -20, strength, -30, strength * 2, -30, strength);
        } else if (Input.GetKey(KeyCode.Space) && !onBar) // for now, player should only tuck if they are in the air
        {
            // tuck
            setBodyPosition(160, strength, -80, strength * 2, 150, strength * 3, -120, strength * 2, 30, strength);
        } else
        {
            // default
            setBodyPosition(150, strength, 0, strength * 2, 120, strength, 0, strength * 2, 30, strength);
        }

    }
}
