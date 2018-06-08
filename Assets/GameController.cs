using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Model;
using DDModel;
using Model.Events;
using System;
using DDModel.Events;

public class GameController : MonoBehaviour {

    public GameObject selectedObject;
    public GameObject combatant;
    public float gridWidth;
    public float gridHeight;
    public float offsetX;
    public float offsetY;
    private Dictionary<string, GameObject> combatants;

    // Use this for initialization
    private Model.State state;
    void Start() {
        state = new State("test");
        combatants = new Dictionary<string, GameObject>();
        Combatant combatent = state.AddObject<Combatant>(0, 0);
        combatent.SetIcon(state, "Rouge");
        combatent.SetFacing(state, Combatant.FacingDirection.South);
        combatent = state.AddObject<Combatant>(1, 0);
        combatent.SetIcon(state, "Knight");
        combatent.SetFacing(state, Combatant.FacingDirection.South);
        combatent = state.AddObject<Combatant>(2, 0);
        combatent.SetIcon(state, "Wizard");
        combatent.SetFacing(state, Combatant.FacingDirection.South);

    }
    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.Send();

        // Show results as text        
        Debug.Log(request.downloadHandler.text);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameEvent gameEvent in state.GameEvents.Queue)
        {
            HandleEvents(gameEvent);
        }
        state.GameEvents.Queue.Clear();

        HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        { // if LMB clicked
            bool objectHit = false;
            RaycastHit raycastHit = new RaycastHit(); // create new raycast hit info object
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit))
            { // create ray from screen's mouse position to world and store info in raycastHit object
                if (raycastHit.collider.gameObject.tag == "Player")
                    objectHit = true;
                else if (raycastHit.collider.gameObject.tag == "Map" && selectedObject != null)
                {
                    float gridX = Mathf.Floor(raycastHit.point.x / gridWidth);
                    float x = (gridX * gridWidth + gridWidth / 2);
                    float gridY = Mathf.Floor(raycastHit.point.z / gridHeight);
                    float y = (gridY * gridHeight + gridHeight / 2);
                    Player player = selectedObject.GetComponent<Player>();
                    player.MoveTo(x,y);
                }
            }

            Deselect(selectedObject); // deselect last hit object
            if (objectHit)
                Select(raycastHit.collider.gameObject); // select new cube
        }
    }

    private void HandleEvents(GameEvent gameEvent)
    {
        switch (gameEvent.Type)
        {
            case "Create":
                HandleCreateEvent(gameEvent as CreateEvent);
                break;
            case "SetIcon":
                HandleSetIcon(gameEvent as SetIconEvent);
                break;
        }
    }

    private void HandleSetIcon(SetIconEvent setIconEvent)
    {
        GameObject gameObject;
        if (combatants.TryGetValue(setIconEvent.Name, out gameObject))
        {
            Debug.Log(string.Format("Set object {0} icon to {1}", setIconEvent.Name, setIconEvent.Icon));
            SetIcon(gameObject, setIconEvent.Icon);
        }
        else
        {
            Debug.LogError(string.Format("No game object {0}", setIconEvent.Name));
        }
    }

    private void SetIcon(GameObject gameObject, string icon)
    {
        float x = 0.01f, y = 0.0f;
        float xBack = 0.01f, yBack = 0.0f;
        switch (icon)
        {
            case "Rouge":
                x = 0.01f;
                y = 0.0f;
                xBack = 0.01f;
                yBack = -0.01f;
                break;
            case "Knight":
                x = 0.18f;
                y = 0.02f;
                xBack = .18f;
                yBack = -.02f;
                break;
            case "Wizard":
                x = 0.68f;
                y = 0.02f;
                xBack = 0.68f;
                yBack = -0.02f;
                break;
        }
        Debug.Log(string.Format("tranform child cnt {0}", gameObject.transform.childCount));
        GameObject childObject = gameObject.transform.GetChild(0).gameObject;
        SetMaterialOffset(gameObject, x, y);
        SetMaterialOffset(childObject, xBack, yBack);
    }
    void SetMaterialOffset(GameObject gameObject, float x, float y)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        Material material = meshRenderer.materials[0];
        material.mainTextureOffset = new Vector2(x, y);

    }

    private void HandleCreateEvent(CreateEvent createEvent)
    {
        Debug.Log(string.Format("Got Create Event Type {0} of ClassName {1}", createEvent.Type, createEvent.ClassName));
        switch(createEvent.ClassName)
        {
            case "Combatant":
                AddCombatent(createEvent.Name, createEvent.X, createEvent.Y);
                break;
        }
    }

    private void AddCombatent(string name, int x, int y)
    {
        float gridX = (x * gridWidth + gridWidth / 2) + offsetX;
        float gridY = (y * gridHeight + gridHeight / 2) + offsetY;

        GameObject newGameObject = Instantiate(combatant, new Vector3(gridX, 0, gridY), Quaternion.Euler(90, 0, 0));
        combatants[name] = newGameObject;
    }

    private void Select(GameObject g)
    {
        selectedObject = g;
    }

    private void Deselect(GameObject g)
    {
        if (selectedObject != null)
        {
            selectedObject = null;
        }
    }
}

