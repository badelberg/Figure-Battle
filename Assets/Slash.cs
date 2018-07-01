using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour {
    public float accumulatedTime;
    public float AnimaitonTime;
    GameController gameControler;
    // Use this for initialization
    void Start () {
        accumulatedTime = 0;
        GameObject gameControlerObj = GameObject.FindWithTag("GameController");
        if (gameControlerObj != null)
            gameControler = gameControlerObj.GetComponent<GameController>();

    }

    // Update is called once per frame
    void Update () {
        accumulatedTime += Time.deltaTime / AnimaitonTime;
        if (accumulatedTime >= AnimaitonTime)
        {
            gameControler.bAnimating = false;
            Destroy(this.gameObject);
        }
    }
}
