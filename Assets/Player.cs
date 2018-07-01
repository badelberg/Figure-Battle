using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Vector3 destinataion;
    private int currentDst;
    private Vector3 currentSrc;
    public Vector3 moveDirection;
    public List<Vector2> Path;
    private GameController gameControler;
    private bool hasDestination;
    private bool bDying;
    public float moveSpeed = 1.0f;
    public float turnSpeed = 10.0f;
    public float snapDistance = 0.5f;
    public float targetAngle;
    private float accumulatedTime;
    private float stepTime;
    // Use this for initialization
    void Start () {
        GameObject gameControlerObj = GameObject.FindWithTag("GameController");
        if (gameControlerObj != null)
            gameControler = gameControlerObj.GetComponent<GameController>();
	}
	
	// Update is called once per frame
	void Update () {
        if (hasDestination)
        {
            Vector3 currentPosition = transform.position;
            accumulatedTime += Time.deltaTime;
            if (accumulatedTime > stepTime)
            {
                if (++currentDst < Path.Count)
                {
                    while (accumulatedTime > stepTime)
                        accumulatedTime -= stepTime;
                    currentSrc = destinataion;
                    destinataion = new Vector3(Path[currentDst].x, currentPosition.y, Path[currentDst].y);
                    moveDirection = destinataion - currentPosition;
                    moveDirection.Normalize();
                }
                else
                {
                    hasDestination = false;
                    gameControler.bAnimating = false;
                }
            }

            {
                transform.position = Vector3.Lerp(currentSrc, destinataion, accumulatedTime/stepTime);

                targetAngle = Mathf.Atan2( moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation =
                    Quaternion.Slerp(transform.rotation,
                                     Quaternion.Euler(90, targetAngle, 0),
                                     accumulatedTime / stepTime);
                gameControler.SetFollowCamera(this.gameObject);
            }
        }
        else if(bDying)
        {
            accumulatedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(currentSrc, destinataion, accumulatedTime);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                            Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0),
                            accumulatedTime / stepTime);
            if (accumulatedTime > stepTime)
            {
                bDying = false;
                gameControler.bAnimating = false;
            }

        }

    }
    public void SetDying()
    {
        accumulatedTime = 0;
        bDying = true;
        stepTime = 3.0f;
        currentSrc = transform.position;
        destinataion = new Vector3(transform.position.x, transform.position.y -0.4f, transform.position.z);
    }
    public void MoveTo(List<Vector2> path)
    {
       currentSrc = transform.position;
        accumulatedTime = 0;
        currentDst = 0;
        stepTime = moveSpeed / path.Count;
        Path = path;
        destinataion = new Vector3(Path[0].x, currentSrc.y, Path[0].y);
        hasDestination = true;
        moveDirection = destinataion - currentSrc;
        moveDirection.Normalize();
        gameControler.bAnimating = true;
    }
}
