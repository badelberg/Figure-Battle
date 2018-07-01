using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootArrow : MonoBehaviour {

    GameController gameControler;
    public int startX;
    public int startY;
    public int targetX;
    public int targetY;
    public float AnimaitonTime;
    public Vector3 start;
    public Vector3 mid;
    public Vector3 target;
    public Quaternion startRot;
    public Quaternion midRot;
    public Quaternion targetRot;
    public float accumulatedTime;
    private bool secondHalf;

    // Use this for initialization
    void Start () {

        float x, z;
        secondHalf = false;
        GameObject gameControlerObj = GameObject.FindWithTag("GameController");
        if (gameControlerObj != null)
            gameControler = gameControlerObj.GetComponent<GameController>();
        
        gameControler.LogicalToScreen(startX, startY, out x, out z);
        this.transform.position = new Vector3(x, this.transform.position.y, z);
        accumulatedTime = 0;
        start = this.transform.position;
        Vector3 curRot = this.transform.rotation.eulerAngles;
        startRot = Quaternion.Euler(curRot.x -20, curRot.y, 0);
        gameControler.LogicalToScreen(targetX, targetY, out x, out z);
        target = new Vector3(x, this.transform.position.y, z);
        targetRot = Quaternion.Euler(curRot.x+20, curRot.y, 0);
        mid = (target - start)/2 + start;
        mid.y += 1;
        midRot = Quaternion.Euler(90, curRot.y, 0);


    }

    // Update is called once per frame
    void Update () {
        accumulatedTime += Time.deltaTime/AnimaitonTime;
        if (!secondHalf)
        {
            this.transform.position = Vector3.Lerp(start, mid, accumulatedTime);
            this.transform.rotation = Quaternion.Lerp(startRot, midRot, accumulatedTime);
            if(accumulatedTime >= AnimaitonTime)
            {
                accumulatedTime -= AnimaitonTime;
                secondHalf = true;
            }
        }
        else 
        {
            this.transform.position = Vector3.Lerp(mid, target, accumulatedTime);
            this.transform.rotation = Quaternion.Lerp(midRot, targetRot, accumulatedTime);
            if (accumulatedTime >= AnimaitonTime)
            {
                gameControler.bAnimating = false;
                Destroy(this.gameObject);
            }
        }
    }
}
