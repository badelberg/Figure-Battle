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
using DDModel.Rule;

public class GameController : MonoBehaviour {

    public Text Pane;
    public Texture koboldTexture;
    private GameObject selectedObject;
    private string selectedName;
    public GameObject combatant;
    public GameObject ring;
    public GameObject arrow;
    public GameObject slash;
    public float cameraSpeed;
    public float gridWidth;
    public float gridHeight;
    public float offsetX;
    public float offsetY;
    public RawImage frame;
    public Canvas canvas;
    public Texture hiliteTexture;
    public Texture nonHiliteTexture;
    private Dictionary<string, GameObject> combatants;
    private Dictionary<string, RawImage> icons;
    public bool bAnimating = false;
    

    // Use this for initialization
    private Model.State state;
    DDModel.Rule.Commands commands = new DDModel.Rule.Commands();
    void Start() {
        Combatant combatant;
        state = new State("test");
        state.Map = new Map(40, 40);
        combatants = new Dictionary<string, GameObject>();
        combatant = commands.AddCombatent(state, "Rouge", "Heros", 15, 14, 9, 6, 14, Combatant.FacingDirection.North);
        AttackStats stats = new AttackStats { AttackBonus = 5, BonusDmg = 3, DiceDamage = 1, DiceType = 6, Reach = 80 };
        combatant.SetReadyAction(new AttackReadyAction { Name = "Short Bow", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Knight", "Heros", 14, 15, 11, 6, 18, Combatant.FacingDirection.North);
        stats = new AttackStats { AttackBonus = 5, BonusDmg = 3, DiceDamage = 1, DiceType = 10, Reach = 1 };
        combatant.SetReadyAction(new AttackReadyAction { Name = "Long Sword", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Wizard", "Heros", 14, 14, 7, 6, 12, Combatant.FacingDirection.North);
        stats = new AttackStats { AttackBonus = 5, BonusDmg = 3, DiceDamage = 1, DiceType = 10, Reach = 120 };
        combatant.SetReadyAction(new AttackReadyAction { Name = "Fire Bolt", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold1", "Monsters", 14, 22, 5, 6, 12, Combatant.FacingDirection.South);
        stats = new AttackStats { AttackBonus = 4, BonusDmg = 2, DiceDamage = 1, DiceType = 4, Reach = 1 };
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold2", "Monsters", 15, 21, 5, 6, 12, Combatant.FacingDirection.South);
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold3", "Monsters", 16, 22, 5, 6, 12, Combatant.FacingDirection.South);
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold4", "Monsters", 13, 23, 5, 6, 12, Combatant.FacingDirection.South);
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold5", "Monsters", 15, 23, 5, 6, 12, Combatant.FacingDirection.South);
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        combatant = commands.AddCombatent(state, "Kolbold6", "Monsters", 17, 23, 5, 6, 12, Combatant.FacingDirection.South);
        combatant.SetReadyAction(new AttackReadyAction { Name = "Dagger", AttackStats = stats }, true);
        icons = new Dictionary<string, RawImage>();
        commands.StartTurn(state);
 
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

        GameEvent gameEvent;
        while (!bAnimating && state.GameEvents.GetEvent(out gameEvent))
        {
            HandleEvents(gameEvent);
        }

        HandleMouseClick();
        HandleKeyBoard();
    }

    private void HandleKeyBoard()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Camera.main.transform.position = new Vector3(
            Camera.main.transform.position.x + moveHorizontal * cameraSpeed,
            Camera.main.transform.position.y,
            Camera.main.transform.position.z + moveVertical * cameraSpeed);
        if (Input.GetKeyDown(KeyCode.Return))
        {
            commands.EndTurn(state);
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        { // if LMB clicked
            bool objectHit = false;
            float lastHit = float.MaxValue;
            string currentCombatant = commands.GetCurrentComabatant();
            RaycastHit[] raycastHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
            RaycastHit raycastHit = new RaycastHit();
            if (raycastHits.Length > 0)
            {
                foreach(RaycastHit hit in raycastHits)
                {
                    if (hit.collider.gameObject.name != currentCombatant && hit.distance < lastHit)
                    {
                        lastHit = hit.distance;
                        raycastHit = hit;
                        objectHit = true;
                        Debug.Log(string.Format("Clicked {0} hit count {1}", hit.collider.gameObject.name, raycastHits.Length));
                        
                    }
                }
            }
            if (objectHit)
            { // create ray from screen's mouse position to world and store info in raycastHit object
                if (raycastHit.collider.gameObject.tag == "Player")
                {
                    string target = raycastHit.collider.gameObject.name;
                    Debug.Log(string.Format("Assign Action Target {0}  {1}", currentCombatant, target));
                    if(commands.AssignActionTarget(state, currentCombatant, target))
                        commands.EndTurn(state);

                }
                else if (raycastHit.collider.gameObject.tag == "Map" && selectedObject != null)
                {

                    int x;
                    int y;
                    ScreenToLogical(raycastHit.point.x, raycastHit.point.z, out x, out y);
                    Combatant combatant = state.GetObject<Combatant>(this.selectedName);
                    Debug.Log(string.Format("Executing Move Action start {0},{1} to {2}, {3}", combatant.LocationX, combatant.LocationY, x, y));
                    if(commands.AssignActionLocation(state, commands.GetCurrentComabatant(), x, y,true))
                        commands.EndTurn(state);
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
            case "Attack":
                HandleAttackEvent(gameEvent as AttackEvent);
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
            case "SetFacing":
                HandleSetFacing(gameEvent as SetFacingEvent);
                break;

            case "SetCondition":
                HandleSetCondition((gameEvent as SetConditionEvent));
                break;
            case "Error":
                HandleError(gameEvent as ErrorEvent);
                break;
        }
    }

    private void HandleSetFacing(SetFacingEvent setFacingEvent)
    {
        float YEular = 0;
        GameObject combatant;
        if (combatants.TryGetValue(setFacingEvent.Name, out combatant))
        {
            switch (setFacingEvent.Facing)
            {
                case "North":
                    YEular = 0;
                    break;
                case "South":
                    YEular = 180;
                    break;
                case "East":
                    YEular = 90;
                    break;
                case "West":
                    YEular = 270;
                    break;
                case "NorthEast":
                    YEular = 45;
                    break;
                case "NorthWest":
                    YEular = 315;
                    break;
                case "SouthEast":
                    break;
                case "SouthWest":
                    YEular = 225;
                    break;
            }
            Debug.Log(string.Format("Setting Facing of {0} to {1}:{2}", setFacingEvent.Name, setFacingEvent.Facing, YEular));
            Vector3 eular = combatant.transform.rotation.eulerAngles;
            eular.y = YEular;
            combatant.transform.rotation = Quaternion.Euler( eular);
        }
    }

    private void HandleSetCondition(SetConditionEvent setConditionEvent)
    {
        switch(setConditionEvent.Condition)
        {
            case "Unconscious":
                SetCombatantUnconscious(setConditionEvent.Name);
                break;
        }
    }

    private void SetCombatantUnconscious(string name)
    {
        GameObject combatant;
        if(combatants.TryGetValue(name, out combatant))
        {
            Player player = combatant.GetComponent<Player>();
            player.SetDying();
            SetFollowCamera(combatant);
            //Vector3 eularAngles = combatant.transform.rotation.eulerAngles;
            //combatant.transform.rotation = Quaternion.Euler(
            //    0,
            //    eularAngles.y,
            //    eularAngles.z);
            //combatant.transform.position = new Vector3(
            //    combatant.transform.position.x,
            //    -0.44f,
            //    combatant.transform.position.z);
        }
    }

    private void HandleAttackEvent(AttackEvent attackEvent)
    {
        
        if (attackEvent.Preview)
        {
            Debug.Log("Attack Event Preview");
        }
        else
        {
            Debug.Log("Attack Event");
            string message = string.Empty;
            foreach (string line in attackEvent.Results)
            {
                message += line + "\r\n";
            }
            Pane.text = message;
            SetAttackCamera(attackEvent.Name, attackEvent.Target);
        }
    }

    private void SetAttackCamera(string name, string targetName)
    {
        GameObject combatant;
        GameObject target;
        if(combatants.TryGetValue(name, out combatant) && combatants.TryGetValue(targetName, out target))
        {
            Vector3 dif = target.transform.position - combatant.transform.position;
            Vector3 mid = (dif) / 2 + combatant.transform.position;
           
            Vector3 offset = Quaternion.Euler(0, 90, 0) * dif;
            if (dif.magnitude >= gridWidth * 2)
            {
                offset = Quaternion.Euler(0, 90, 0) * dif;
                mid.y = Math.Min( 2f, dif.magnitude/ (gridWidth * 2));
                CreateArrowAnimation(combatant, target, dif);
            }
            else
            {
                offset = Quaternion.Euler(0, 90, 0) * (dif.normalized * gridWidth *3);
                mid.y = 0.8f;
                CreateSlashAnimation(combatant, target, dif);
            }
            Camera.main.transform.position = mid + offset;
            Camera.main.transform.LookAt(mid);
            bAnimating = true;
        }
    }
    private void CreateSlashAnimation(GameObject combatant, GameObject target, Vector3 dif)
    {
        GameObject slashObj = Instantiate(slash);
        Quaternion facing = combatant.transform.rotation;
        Vector3 offset = new Vector3(0, 0, gridWidth/2);
        Vector3 voffset = new Vector3(0, 0.3f, 0);
        slashObj.transform.position = combatant.transform.position + (facing*offset)+ voffset;
        slashObj.transform.rotation = Quaternion.Euler(0, combatant.transform.rotation.eulerAngles.y - 90, 0);
    }
    private void CreateArrowAnimation(GameObject combatant, GameObject target, Vector3 dif)
    {

        GameObject arrowObj = Instantiate(arrow);
        ShootArrow shootArrow = arrowObj.GetComponent<ShootArrow>();
        int x, z;
        ScreenToLogical(combatant.transform.position.x, combatant.transform.position.z, out x, out z);
        shootArrow.startX = x;
        shootArrow.startY = z;
        ScreenToLogical(target.transform.position.x, target.transform.position.z, out x, out z);
        shootArrow.targetX = x;
        shootArrow.targetY = z;
    }

    private void HandleError(ErrorEvent errorEvent)
    {
        Debug.Log(errorEvent.Message);
    }

    private void HandleMove(MoveEvent moveEvent)
    {
        GameObject playerObject;
        if (!moveEvent.Preview && combatants.TryGetValue(moveEvent.Name, out playerObject))
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
        if(selectedObject != null && icons.TryGetValue(selectedName, out frame))
        {
            frame.texture = nonHiliteTexture;
            GameObject ring = selectedObject.transform.Find("Ring(Clone)").gameObject;
            Destroy(ring);
        }
        Debug.Log(string.Format("Current Player is {0}", currentPlayerEvent.Name));
        if (combatants.TryGetValue(currentPlayerEvent.Name, out combatant))
        {
            Debug.Log(string.Format("Setting selected Object"));
            this.selectedObject = combatant;
            this.selectedName = currentPlayerEvent.Name;
            Instantiate(ring, combatant.transform);
            SetFollowCamera(combatant);
        }
        if(icons.TryGetValue(currentPlayerEvent.Name, out frame))
        {
            Debug.Log(string.Format("Hiliting Frame"));
            frame.texture = hiliteTexture;
        }
    }

    public void SetFollowCamera(GameObject combatant)
    {
        float x = combatant.transform.position.x + 3;
        float y = Camera.main.transform.position.y;
        float z = combatant.transform.position.z +10;
        Vector3 dif = new Vector3(0, 0, gridWidth * 3);
        //Debug.Log(string.Format("Current Player z rot {0} {1} {2}", 
        //    combatant.transform.rotation.eulerAngles.x,
        //    combatant.transform.rotation.eulerAngles.y,
        //    combatant.transform.rotation.eulerAngles.z
        //    ));
        Vector3 offset = Quaternion.Euler(0, combatant.transform.rotation.eulerAngles.y+180, 0) * dif;
        offset.y = 1;
        //Debug.Log(string.Format("dif {0} {1} {2}",
        //   dif.x,
        //   dif.y,
        //   dif.z
        //    ));
        //Debug.Log(string.Format("offset {0} {1} {2}",
        //   offset.x,
        //   offset.y,
        //   offset.z
        //    ));


        Camera.main.transform.position = combatant.transform.position+offset;
        Camera.main.transform.LookAt(combatant.transform);

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
        if (icon.Contains("Kolbold"))
        {
            SetTexture(gameObject, koboldTexture, 0.47f);
            SetTexture(childObject, koboldTexture, -0.47f);
        }
        SetMaterialOffset(gameObject, x, y);
        SetMaterialOffset(childObject, xBack, yBack);
    }

    private void SetTexture(GameObject gameObject, Texture koboldTexture, float yOffset)
    {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        Material material = meshRenderer.materials[0];
        material.mainTexture = koboldTexture;
        material.mainTextureScale = new Vector2(0.07f, yOffset);
    }

    private static void GetIconOffset(string icon, ref float x, ref float y, ref float xBack, ref float yBack)
    {
        switch (icon)
        {
            case "Kolbold1":
                x = 0.0f;
                y = 0.0f;
                xBack = 0.01f;
                yBack = -0.01f;
                break;
            case "Kolbold2":
                x = 0.076f;
                y = 0.0f;
                xBack = 0.076f;
                yBack = -0.01f;
                break;
            case "Kolbold3":
                x = 0.15f;
                y = 0.0f;
                xBack = 0.15f;
                yBack = -0.01f;
                break;
            case "Kolbold4":
                x = 0.225f;
                y = 0.0f;
                xBack = 0.225f;
                yBack = -0.01f;
                break;
            case "Kolbold5":
                x = 0.3f;
                y = 0.0f;
                xBack = 0.3f;
                yBack = -0.01f;
                break;
            case "Kolbold6":
                x = 0.38f;
                y = 0.0f;
                xBack = 0.38f;
                yBack = -0.01f;
                break;
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
            if (icon.Contains("Kolbold"))
            { 
                material.mainTexture = koboldTexture;
                material.mainTextureScale = new Vector2(0.07f, 0.47f);
            }
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

    public void LogicalToScreen(int x, int y, out float screenX, out float screenY)
    {
        screenX = (x * gridWidth + gridWidth / 2) + offsetX;
        screenY = (y * gridHeight + gridHeight / 2) + offsetY;
    }

    public void ScreenToLogical(float screenX, float screenY, out int x, out int y)
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
        newGameObject.name = name;
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

