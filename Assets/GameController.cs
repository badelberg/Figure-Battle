using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Model;
using DDModel;
using Model.Events;
using System;
using DDModel.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject selectedObject;
    private string selectedName;
    public GameObject combatant;
    public float gridWidth;
    public float gridHeight;
    public float offsetX;
    public float offsetY;
    public RawImage frame;
    public Canvas canvas;
    public Texture hiliteTexture;
    private Dictionary<string, GameObject> combatants;
    private Dictionary<string, RawImage> icons;

    // Use this for initialization
    private Model.State state;
    DDModel.Entities.InititativeQueue initiative;
    void Start() {
        state = new State("test");
        state.Map = new Map(40, 40);
        initiative = state.AddObject<DDModel.Entities.InititativeQueue>(0,0);
        combatants = new Dictionary<string, GameObject>();
        AddCombatent("Rouge", 0, 0, 8, 6, 12, Combatant.FacingDirection.South);
        AddCombatent("Knight", 1, 0, 12, 6, 13, Combatant.FacingDirection.South);
        AddCombatent( "Wizard", 2, 0, 6, 6, 10, Combatant.FacingDirection.South);
        icons = new Dictionary<string, RawImage>();
        initiative.StartGameTurn(state);
 
    }

    private void AddCombatent(string type, int x, int y, int hp, int move, int ac, Combatant.FacingDirection facing)
    {
        Combatant combatent = state.AddObject<Combatant>(x, y);
        combatent.SetIcon(state, type);
        combatent.SetFacing(state, facing);
        combatent.SetStats(state, hp, move, ac);
        this.initiative.AddCombatant(combatent.Name);
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
                    
                    int x;
                    int y;
                    ScreenToLogical(raycastHit.point.x, raycastHit.point.z, out x, out y);
                    Combatant combatant = state.GetObject<Combatant>(this.selectedName);
                    Debug.Log(string.Format("Executing Move Action start {0},{1} to {2}, {3}", combatant.LocationX, combatant.LocationY, x, y));
                    state.AssignAction<DDModel.Actions.Move>(this.selectedName, x, y);
                    state.ExectuteActions();
                    Debug.Log(string.Format("Event Queue Cnt {0}", state.GameEvents.Queue.Count)); 
                }
            }

            Deselect(selectedObject); // deselect last hit object
            if (objectHit)
                Select(raycastHit.collider.gameObject); // select new cube
        }
    }

    private void HandleEvents(GameEvent gameEvent)
    {
        Debug.Log(string.Format("Handling Event {0}", gameEvent.Type));
        switch (gameEvent.Type)
        {
            case "Create":
                HandleCreateEvent(gameEvent as CreateEvent);
                break;
            case "SetIcon":
                HandleSetIcon(gameEvent as SetIconEvent);
                break;
            case "SetStats":
                 HandleSetStats(gameEvent as SetStatsEvent);
                break;
            case "CurrentPlayer":
                HandleCurrentPlayer(gameEvent as CurrentPlayerEvent);
                break;
            case "Move":
                HandleMove(gameEvent as MoveEvent);
                break;
            case "Error":
                HandleError(gameEvent as ErrorEvent);
                break;
        }
    }

    private void HandleError(ErrorEvent errorEvent)
    {
        Debug.Log(errorEvent.Message);
    }

    private void HandleMove(MoveEvent moveEvent)
    {
        GameObject playerObject;
        if (combatants.TryGetValue(moveEvent.Name, out playerObject))
        {

            Player player = playerObject.GetComponent<Player>();
            float gridX;
            float gridY;
            List<Vector2> path = new List<Vector2>();
            
            foreach(Model.Tuple<int,int> step in moveEvent.Path)
            {
                LogicalToScreen((int)step.Item1, (int)step.Item2, out gridX, out gridY);
                path.Add(new Vector2(gridX, gridY));
            }
            Debug.Log(string.Format("Path Length {0}", path.Count));
            player.MoveTo(path);
        }
    }

    private void HandleCurrentPlayer(CurrentPlayerEvent currentPlayerEvent)
    {
        GameObject combatent = null;
        RawImage frame = null;
        Debug.Log(string.Format("Current Player is {0}", currentPlayerEvent.Name));
        if (combatants.TryGetValue(currentPlayerEvent.Name, out combatant))
        {
            Debug.Log(string.Format("Setting selected Object"));
            this.selectedObject = combatant;
            this.selectedName = currentPlayerEvent.Name;
        }
        if(icons.TryGetValue(currentPlayerEvent.Name, out frame))
        {
            Debug.Log(string.Format("Hiliting Frame"));
            frame.texture = hiliteTexture;
        }
    }

    private void HandleSetStats(SetStatsEvent setStatsEvent)
    {
        if(setStatsEvent.HP != null)
        {
            Text text = GetChildUI<Text>(setStatsEvent.Name, "HP");
            text.text = setStatsEvent.HP.ToString();
        }
        if (setStatsEvent.Move != null)
        {
            Text text = GetChildUI<Text>(setStatsEvent.Name, "Move");
            text.text = setStatsEvent.Move.ToString();
        }
        if (setStatsEvent.AC != null)
        {
            Text text = GetChildUI<Text>(setStatsEvent.Name, "AC");
            text.text = setStatsEvent.AC.ToString();
        }

    }

    private void HandleSetIcon(SetIconEvent setIconEvent)
    {
        GameObject gameObject;
        if (combatants.TryGetValue(setIconEvent.Name, out gameObject))
        {
            Debug.Log(string.Format("Set object {0} icon to {1}", setIconEvent.Name, setIconEvent.Icon));
            SetFigure(gameObject, setIconEvent.Icon);
            SetIcon(setIconEvent.Name, setIconEvent.Icon);
        }
        else
        {
            Debug.LogError(string.Format("No game object {0}", setIconEvent.Name));
        }
    }

   
    private void SetFigure(GameObject gameObject, string icon)
    {
        float x = 0.01f, y = 0.0f;
        float xBack = 0.01f, yBack = 0.0f;
        GetIconOffset(icon, ref x, ref y, ref xBack, ref yBack);
        Debug.Log(string.Format("tranform child cnt {0}", gameObject.transform.childCount));
        GameObject childObject = gameObject.transform.GetChild(0).gameObject;
        SetMaterialOffset(gameObject, x, y);
        SetMaterialOffset(childObject, xBack, yBack);
    }

    private static void GetIconOffset(string icon, ref float x, ref float y, ref float xBack, ref float yBack)
    {
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
    }

    private T GetChildUI<T>(string name, string child) where T:Component
    {
        T result= null;
        RawImage frame = null;
        if (icons.TryGetValue(name, out frame))
        {
            Transform iconTransform = frame.transform.Find(child);
            if (iconTransform != null)
            {
                result = iconTransform.GetComponentInParent<T>();
            }
        }
        return result;
    }
    private void SetIcon(string name, string icon)
    {
        RawImage iconImage = GetChildUI<RawImage>(name, "Icon");

        if(iconImage != null)
        {
            Debug.Log("Found icon Image");
            float x = 0.01f, y = 0.0f;
            float xBack = 0.01f, yBack = 0.0f;
            GetIconOffset(icon, ref x, ref y, ref xBack, ref yBack);
            Material material = Instantiate(iconImage.material);
            material.mainTextureOffset = new Vector2(x, y);
            iconImage.material = material;
        }

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

    private void LogicalToScreen(int x, int y, out float screenX, out float screenY)
    {
        screenX = (x * gridWidth + gridWidth / 2) + offsetX;
        screenY = (y * gridHeight + gridHeight / 2) + offsetY;
    }

    private void ScreenToLogical(float screenX, float screenY, out int x, out int y)
    {
        x = Mathf.FloorToInt((screenX - offsetX) / gridWidth);
        y = Mathf.FloorToInt((screenY - offsetY) / gridHeight);
    }
    private void AddCombatent(string name, int x, int y)
    {
        float gridX;
        float gridY;
        LogicalToScreen(x, y, out gridX, out gridY);

        GameObject newGameObject = Instantiate(combatant, new Vector3(gridX, 0, gridY), Quaternion.Euler(90, 0, 0));
        combatants[name] = newGameObject;
        AddUI(name, combatants.Count);
    }

    private void AddUI(string name, int count)
    {
        RawImage UI = Instantiate<RawImage>(frame);
        icons[name] = UI;
        UI.transform.SetParent(canvas.transform);
        RectTransform rect = UI.GetComponent<RectTransform>();
        float x = -490 + (count - 1) * 90;
        float y = -218;
        rect.localPosition = new Vector3(x, y);


    }

    private void Select(GameObject g)
    {
    }

    private void Deselect(GameObject g)
    {
    }
}

