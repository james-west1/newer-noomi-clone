using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject head, torso, leftArm, rightArm, leftFemur, rightFemur, leftLowerLeg, rightLowerLeg, leftFoot, rightFoot, leftHand, rightHand, bar1; // get game objects from the scene
    public HingeJoint leftBarJoint, rightBarJoint, leftShoulder, rightShoulder, neck, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle; // declare hinge joints for each
    public JointSpring leftShoulderSpring, rightShoulderSpring, neckSpring, leftHipSpring, rightHipSpring, leftKneeSpring, rightKneeSpring, leftAnkleSpring, rightAnkleSpring; // declare joint springs for each joint

    // Array declaration
    public HingeJoint[] hingeJoints;
    public JointSpring[] jointSprings;

    public static PlayerController instance;

    public bool shouldArch, shouldTuck, shouldStraight, shouldLetGo, shouldReset;

    // Enum for body groups (These correspond to the first position in the hingeJoints and jointSprings arrays
    enum BodyGroup
    {
        Shoulders = 0,
        Hips = 3,
        Knees = 5,
        Ankles = 7
    }

    // Enum for body parts (These correspond to positions in the hingeJoints and jointSprings arrays
    enum BodyPart
    {
        LeftShoulder = 0,
        RightShoulder = 1,
        Neck = 2,
        LeftHip = 3,
        RightHip = 4,
        LeftKnee = 5,
        RightKnee = 6,
        LeftAnkle = 7,
        RightAnkle = 8
    }

    public float strength; //spring strength, determines how strong noomi is
    public float damp; //spring damper, also plays into the strength factor

    bool onBar; // is the player currently on the bar or not?
    bool distanceThreshold; // did the player leave the bar?

    GameObject lastBarGrabbed; // need to store this to determine if the player is about to grab the same bar that they were just on
    // if so, need to use distance threshold

    // Start is called before the first frame update
    void Start()
    {
        if (instance)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }
        initJoints();
        onBar = true;
        lastBarGrabbed = bar1;
        distanceThreshold = false;
        // no longer need to set max angular velocity higher because there is a way to do that in project settings now, much better
    }

    void initJoints() 
    {
        // Initialize joints (Touching this broke the game lmao, so it stays as is)
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

        // Initialize the arrays in such a way that makes initializing them easy
        hingeJoints = new HingeJoint[9] { leftShoulder, rightShoulder, neck, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle };
        jointSprings = new JointSpring[9] { leftShoulderSpring, rightShoulderSpring, neckSpring, leftHipSpring, rightHipSpring, leftKneeSpring, rightKneeSpring, leftAnkleSpring, rightAnkleSpring };

        // Initialize joint springs
        for (int i = 0; i < hingeJoints.Length; i++)
        {
            jointSprings[i] = hingeJoints[i].spring;
        }

        // Sets damper of all springs, creates list of them which we can iterate with a foreach and edit their damper property
        new List<JointSpring>(jointSprings).ForEach(i => i.damper = damp);
        
    }

    // sets body position of player by setting target positions and strengths of all joints
    void setBodyPosition(float shoulderAngle, float shoulderStrength, float neckAngle, float neckStrength, float hipAngle, float hipStrength, float kneeAngle, float kneeStrength, float ankleAngle, float ankleStrength)
    {
        setBodyGroupPosition(shoulderAngle, shoulderStrength, BodyGroup.Shoulders);
        setBodyPartPosition(neckAngle, neckStrength, BodyPart.Neck);
        setBodyGroupPosition(hipAngle, hipStrength, BodyGroup.Hips);
        setBodyGroupPosition(kneeAngle, kneeStrength, BodyGroup.Knees);
        setBodyGroupPosition(ankleAngle, ankleStrength, BodyGroup.Ankles);
    }

    // Sets a specific body group to set angle and strength
    void setBodyGroupPosition(float angle, float strength, BodyGroup group)
    {
        for (int i = (int) group; i < (int) group + 2; i++)
        {
            jointSprings[i].targetPosition = angle;
            jointSprings[i].spring = strength;
            hingeJoints[i].spring = jointSprings[i];
        }
    }

    // Sets a specific body part to set angle and strength
    void setBodyPartPosition(float angle, float strength, BodyPart part)
    {
        int i = (int) part;
        jointSprings[i].targetPosition = angle;
        jointSprings[i].spring = strength;
        hingeJoints[i].spring = jointSprings[i];
    }

    void checkRegrabs()
    {
        foreach (GameObject bar in GameObject.FindGameObjectsWithTag("bar"))
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

    // Update is called once every frame
    void Update()
    {
        if (!onBar) // if in the air, check potential regrabs
        {
            checkRegrabs();
        }

        if (Input.GetKey(KeyCode.R) || shouldReset) {
            // load scene again to reset
            EditorSceneManager.LoadScene("SampleScene");
        }

        if (Input.GetKey(KeyCode.UpArrow) || shouldLetGo)
        {
            // break joints to let go
            Destroy(leftBarJoint);
            Destroy(rightBarJoint);
            onBar = false;

            // want collisions between arms and bar to work again
            Physics.IgnoreCollision(leftArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);
            Physics.IgnoreCollision(rightArm.GetComponent<Collider>(), lastBarGrabbed.GetComponent<Collider>(), false);

        }

        // Toggles slow motion
        if (Input.GetKey(KeyCode.Z))
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale /= 2f;
            }
            else
            {
                Time.timeScale *= 2f;
            }

        }

        if (Input.GetKey(KeyCode.LeftArrow) || shouldArch)
        {
            // arch
            if (onBar)
            {
                // weaker arch
                setBodyPosition(-10, strength, 30, strength * 2, -20, strength, -30, strength * 2, -30, strength);
            } else
            {
                // stronger arch so he can do wall flips and shit
                setBodyPosition(-10, strength, 30, strength * 2, -20, strength * 3, -30, strength * 2, -30, strength);
            }
        }
        else if ((Input.GetKey(KeyCode.Space) || shouldTuck) && !onBar) // since tucking makes your hips stronger, don't want to do it on bar
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
