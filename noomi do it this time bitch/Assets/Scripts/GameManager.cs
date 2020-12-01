using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // these are the methods for the buttons - sets the respective bools true or false in PlayerController

    public void setPlayerArch(bool shouldArch)
    {
        PlayerController.instance.shouldArch = shouldArch;
    }

    public void setPlayerTuck(bool shouldTuck)
    {
        PlayerController.instance.shouldTuck = shouldTuck;
    }

    public void setPlayerStraight(bool shouldStraight)
    {
        PlayerController.instance.shouldStraight = shouldStraight;
    }

    public void letGo(bool shouldLetGo)
    {
        PlayerController.instance.shouldLetGo = shouldLetGo;
    }

    public void resetScene(bool shouldReset) //this one should be onClick instead of pointer down / pointer up because don't want to reset continuously
    {
        PlayerController.instance.shouldReset = shouldReset;
    }
}