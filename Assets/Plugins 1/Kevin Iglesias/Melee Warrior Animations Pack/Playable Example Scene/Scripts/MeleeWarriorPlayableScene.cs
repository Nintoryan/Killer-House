using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace KevinIglesias {
    
    [System.Serializable]
    public class UnitDatabase
    {
        public int id;
        public string unitName;
        public int maxHP;
        public float hpBarHeight;
        public int attack;
        public int defense;
        
        public int goldCost;
        public int woodCost;
        public int foodCost;
        
        public int selectionSize;
        public bool canAttack;
        public string description;
        public float deploySpeed;
        public Sprite[] icon;
        public GameObject[] unitPrefab;
        public float attackRange;
    }
    
    [System.Serializable]
    public class UnitInstance
    {
        public int guid;
        public int id;
        public string unitName;
        public int currentHP;
        public int maxHP;
        public int attack;
        public int defense;
        public int owner;
        public GameObject selectionObject;
        public GameObject HPObject;
        public bool canAttack;
        public GameObject unitObject;
        public NavMeshAgent navAgent;
        public IEnumerator pathFinding;
        public Vector3 destination;
        public bool isMoving;
        public Animator anim;    
        public float attackRange;
        public bool isAttacking;
        public int target;
        public bool selected;
        
        public UnitInstance(int newGuid, int newOwner, GameObject newUnitObject, NavMeshAgent newAgent, Animator newAnim, UnitDatabase newUnit)
        {
            guid = newGuid;
            owner = newOwner;
            unitObject = newUnitObject;
            navAgent = newAgent;
            anim = newAnim;
            
            id = newUnit.id;
            unitName = newUnit.unitName;
            currentHP = newUnit.maxHP;
            maxHP = newUnit.maxHP;
            attack = newUnit.attack;
            defense = newUnit.defense;
            canAttack = newUnit.canAttack;
            attackRange = newUnit.attackRange;
            target = -1;
        }
    }
    
    [System.Serializable]
    public class Player
    {
        public int gold;
        public int wood;
        public int food;
        
        public List<UnitQueue> queueUnits = new List<UnitQueue>();
    }

    public class UnitQueue
    {
        public int id;
        public int queueValue;
        
        public UnitQueue(int newId)
        {
            id = newId;
        }
    }
    
    ///Main Script
    public class MeleeWarriorPlayableScene : MonoBehaviour {

        [Header("Databases")]
        public List<UnitDatabase> units = new List<UnitDatabase>();
        public List<UnitInstance> deployedUnits = new List<UnitInstance>();
        public List<int> selectedUnits = new List<int>();
        public List<Player> players = new List<Player>();
    
        [Header("Game Control")]
        public int control = -1; //0 = blue, 1 = red
        public int selectedUnit = -1;
        public int clickedTimes = 0;
        
        public GameObject selectionPrefab;
        public GameObject HPPrefab;
        public GameObject[] barracks;
        public GameObject[] pathFlags;
        
        [Header("UI Elements")]
        public Texture2D cursor;
        public Texture2D attackCursor;
        
        public RectTransform topPanel;
        public RectTransform bottomPanel;
        
        public Image currentPlayerColor;
        public Image selectedUnitIcon;
        
        public Text selectedUnitText;
        public Text selectedUnitHP;
        
        public GameObject multipleSelectionInfo;
        public GameObject singleSelectionInfo;
        
        public GameObject attackUI;
        public Text attackText;
        public GameObject defenseUI;
        public Text defenseText;
        
        public GameObject[] deployMenu;
        public GameObject deployInfo;
        public Text deployValue;
        public Image deployBar;
        public bool[] abortDeploy;

        public GameObject descriptionInfo;
        public Text descriptionUnitName;
        
        public Text goldCost;
        public Text woodCost;
        public Text foodCost;
        
        public Text descriptionText;
        public Text totalGold;
        public Text totalWood;
        public Text totalFood;
        
        Camera cam;
        float minFov = 15f;
        float maxFov = 45f;
        float sensitivity = 15f;
        
        int topPanelHiddenY = 55;
        int bottomPanelHiddenY = -155;
        
        Vector3 newPosition;
        
        int attackCOguid = -1;
        IEnumerator attackCo;

        GameObject flagPath;
        
        void Start()
        {
            cam = Camera.main;
            
            topPanel.anchoredPosition3D = new Vector3(topPanel.anchoredPosition3D.x, topPanelHiddenY, topPanel.anchoredPosition3D.z);
            bottomPanel.anchoredPosition3D = new Vector3(bottomPanel.anchoredPosition3D.x, bottomPanelHiddenY, bottomPanel.anchoredPosition3D.z);
        
            deployedUnits = new List<UnitInstance>();
            selectedUnits = new List<int>();
             
            deployedUnits.Add(new UnitInstance(0, 0, barracks[0], null, null, units[0]));
            deployedUnits.Add(new UnitInstance(1, 1, barracks[1], null, null, units[0]));
            
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
            
            StartCoroutine(MovePointer(barracks[0].transform.GetChild(1)));
            StartCoroutine(MovePointer(barracks[1].transform.GetChild(1)));
        }
         
        //Camera Zoom
        void Update () {
           float fov = cam.fieldOfView;
           fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
           fov = Mathf.Clamp(fov, minFov, maxFov);
           Camera.main.fieldOfView = fov;
        }
        
        //Starting barracks arrows animation
        IEnumerator MovePointer(Transform pointer)
        {
            Vector3 pos = pointer.localPosition;
            
            pointer.localPosition = new Vector3(pos.x, pos.y+0.1f, pos.z);
            
            yield return new WaitForSeconds(0.25f);
            
            pointer.localPosition = pos;
            
            yield return new WaitForSeconds(0.25f);
            
            pointer.localPosition = new Vector3(pos.x, pos.y+0.1f, pos.z);
            
            yield return new WaitForSeconds(0.25f);
            
            pointer.localPosition = pos;
            
            if(selectedUnit == -1)
            {
                StartCoroutine(MovePointer(pointer));
            }
        }
        
        //Unselect all units
        public void Unselect()
        {
            for(int i = 0; i < selectedUnits.Count; i++)
            {
                
                if(deployedUnits[selectedUnits[i]].selectionObject != null)
                {
                    deployedUnits[selectedUnits[i]].selectionObject.SetActive(false);
                }
                
                if(deployedUnits[selectedUnits[i]].HPObject != null)
                {
                    deployedUnits[selectedUnits[i]].HPObject.SetActive(false);
                }
                
                deployedUnits[selectedUnits[i]].selected = false;
            }
            selectedUnits = new List<int>();
        }

        //Click unit in map
        public void ClickUnit(int unitGuid, int player)
        {
            multipleSelectionInfo.SetActive(false);
            singleSelectionInfo.SetActive(true);

            Unselect();
            
            clickedTimes++;
            
            selectedUnits.Add(unitGuid);
            StartCoroutine(DoubleClickCheck(unitGuid, selectedUnit, player));
            
            selectedUnit = unitGuid;

            if(control == -1)
            {
                barracks[0].transform.GetChild(1).gameObject.SetActive(false);
                barracks[1].transform.GetChild(1).gameObject.SetActive(false);
                
                control = player;
                StartCoroutine(ShowMenus(player));
            }
            
            if(control != player)
            {
                control = player;
                
                StartCoroutine(ShowMenus(player));
            }
            
            SelectUnit(unitGuid);
            LoadUnitUI(unitGuid, player);
            
        }
        
        //Change color (top panel)
        void ChangeColor(int owner)
        {
            if(owner == 0)
            {
                currentPlayerColor.color = Color.blue;
            }else{
                currentPlayerColor.color = Color.red;
            }
        }
        
        //Refresh current player resource values
        void UpdateResources(int owner)
        {
            totalGold.text = players[owner].gold.ToString();
            totalWood.text = players[owner].wood.ToString();
            totalFood.text = players[owner].food.ToString();
        }

        //Show menus (when switching players)
        IEnumerator ShowMenus(int player)
        {
            ChangeColor(player);
            
            UpdateResources(player);
            
            UpdateQueuePanel(player);
            UpdateDeployBar(player);
            
            float i = 0;
            while(i < 1)
            {
                i += Time.deltaTime * 4;
                topPanel.anchoredPosition3D = new Vector3(topPanel.anchoredPosition3D.x, Mathf.Lerp(topPanelHiddenY, 0, i),topPanel.anchoredPosition3D.z);
                bottomPanel.anchoredPosition3D = new Vector3(bottomPanel.anchoredPosition3D.x, Mathf.Lerp(bottomPanelHiddenY, 0, i),bottomPanel.anchoredPosition3D.z);
                yield return 0;
            }
        }
        
        //Select clicked unit
        public void SelectUnit(int unitGuid)
        {
            if(flagPath != null)
            {
                Destroy(flagPath);
            }
            
            UnitInstance currentUnit = deployedUnits[unitGuid];
            
            currentUnit.selected = true;
            
            if(attackCo != null)
            {
                if(attackCOguid != -1)
                {
                    if(!deployedUnits[attackCOguid].selected)
                    {
                        deployedUnits[attackCOguid].selectionObject.SetActive(false);
                    }
                    deployedUnits[attackCOguid].selectionObject.GetComponent<Renderer>().material.color = Color.white;
                }
                StopCoroutine(attackCo);
            }
            
            if(currentUnit.selectionObject == null)
            {
                currentUnit.selectionObject = Instantiate(selectionPrefab);
                
                currentUnit.selectionObject.GetComponent<Renderer>().material.color = Color.white;
                
                currentUnit.selectionObject.transform.SetParent(currentUnit.unitObject.transform);
                currentUnit.selectionObject.transform.localScale = new Vector3(units[currentUnit.id].selectionSize, units[currentUnit.id].selectionSize, units[currentUnit.id].selectionSize);
                currentUnit.selectionObject.transform.localPosition = new Vector3(0, 0.05f, 0);
                currentUnit.selectionObject.transform.localEulerAngles = Vector3.zero;
                currentUnit.selectionObject.SetActive(true);
            }else{
                currentUnit.selectionObject.SetActive(true);
            }
            
            if(currentUnit.HPObject == null)
            {
                currentUnit.HPObject = Instantiate(HPPrefab);
                currentUnit.HPObject.transform.SetParent(currentUnit.unitObject.transform);
                currentUnit.HPObject.transform.localPosition = new Vector3(0, units[currentUnit.id].hpBarHeight, 0);
                currentUnit.HPObject.SetActive(true);
            }else{
                currentUnit.HPObject.SetActive(true);
                
            }
            
            if(currentUnit.isMoving)
            {
                if(!currentUnit.isAttacking)
                {
                    CreateFlagPath(currentUnit.destination, currentUnit.owner);
                }
            }
        }
        
        //Load unit info panel (bottom left panel)
        public void LoadUnitUI(int unitGuid, int player)
        {
            UnitInstance currentUnit = deployedUnits[unitGuid];
            
            selectedUnitText.text = currentUnit.unitName;
            
            selectedUnitIcon.sprite = units[currentUnit.id].icon[currentUnit.owner];
            
            selectedUnitHP.text = currentUnit.currentHP+"/"+currentUnit.maxHP;
            
            selectedUnitHP.transform.parent.GetChild(0).localScale = new Vector3((currentUnit.currentHP/ (float)currentUnit.maxHP), 1, 1);
            
            //Check if Unit is Barracks
            if(!currentUnit.canAttack)
            {
                attackUI.SetActive(false);
                defenseUI.SetActive(false);
                LoadDeployableUnits(currentUnit.owner);
                
                if(players[player].queueUnits != null)
                {
                    if(players[player].queueUnits.Count >= 1)
                    {
                        deployInfo.SetActive(true);
                    }else{
                        deployInfo.SetActive(false);
                    }
                }else{
                    deployInfo.SetActive(false);
                }
                
            }else{
                attackUI.SetActive(true);
                attackText.text = currentUnit.attack.ToString();
                defenseUI.SetActive(true);
                defenseText.text = currentUnit.defense.ToString();
                deployMenu[0].SetActive(false);
                deployMenu[1].SetActive(false);
                deployInfo.SetActive(false);
            }
        }
        
        //Show deployable menu (bottom mid panel)
        public void LoadDeployableUnits(int owner)
        {
            if(owner == 0)
            {
                deployMenu[0].SetActive(true);
                deployMenu[1].SetActive(false);
            }else{
                deployMenu[0].SetActive(false);
                deployMenu[1].SetActive(true);
            }
        }
        
        //Show unit info when cursor is over (bottom right panel)
        public void ShowUnitInfo(int unitId)
        {
            UnitDatabase currentUnit = units[unitId];
            
            descriptionInfo.SetActive(true);
            
            descriptionUnitName.text = currentUnit.unitName;
            
            goldCost.text = currentUnit.goldCost.ToString();
            woodCost.text = currentUnit.woodCost.ToString();
            foodCost.text = currentUnit.foodCost.ToString();
            
            descriptionText.text = currentUnit.description;
        }
        
        //Check if unit is deployable (available resources or queue is not full)
        public bool CheckIfDeployable(int unitId, int player)
        {
            if(players[player].gold >= units[unitId].goldCost && players[player].wood >= units[unitId].woodCost && players[player].food >= units[unitId].foodCost)
            {
                if(players[player].queueUnits.Count == 5)
                {
                    return false;
                }else{
                    return true;
                }
            }else{
                return false;
            }
        }
        
        //Refresh deployable units buttons (bottom mid panel)
        public void UpdateDeployableFrames(int player)
        {
            for(int i = 0; i < deployMenu[player].transform.childCount; i++)
            {
                MeleeWarriorUnitScript unit_mWUS = deployMenu[player].transform.GetChild(i).GetComponent<MeleeWarriorUnitScript>();
                Color frameColor = unit_mWUS.transform.GetChild(0).GetComponent<Image>().color;
            
                if(CheckIfDeployable(unit_mWUS.id, player))
                {
                    frameColor = new Color(frameColor.r, frameColor.g, frameColor.b, 1f);
                }else{
                    frameColor = new Color(frameColor.r, frameColor.g, frameColor.b, 0.5f);
                }
                unit_mWUS.transform.GetChild(0).GetComponent<Image>().color = frameColor;
            }
        }
        
        //Start creating unit (bottom mid and bottom left panel)
        public void DeployUnit(int unitId, int player)
        {
            players[player].gold -= units[unitId].goldCost;
            players[player].wood -= units[unitId].woodCost;
            players[player].food -= units[unitId].foodCost;
            
            UpdateResources(player);
            deployInfo.SetActive(true);
            
            if(players[player].queueUnits == null)
            {
                players[player].queueUnits = new List<UnitQueue>(); 
            }
            
            players[player].queueUnits.Add(new UnitQueue(unitId));
           
            UpdateDeployableFrames(player);
            UpdateQueuePanel(player);
            
            if(players[player].queueUnits.Count == 1)
            {
                StartCoroutine(QueueUnit(unitId, player));
            }
        }
        
        //Start unit queue for creating unit (bottom left panel)
        IEnumerator QueueUnit(int unitId, int player)
        {
            float i = 0;
            while(i < 1 && !abortDeploy[player])
            {
                if(players[player].queueUnits[0].queueValue % 4f == 0)
                {
                    UpdateDeployBar(player);
                }
                i += Time.deltaTime * units[unitId].deploySpeed;
                players[player].queueUnits[0].queueValue = (int)Mathf.Lerp(0, 100, i);
                yield return 0;
            }
            
            if(!abortDeploy[player])
            {
                SpawnUnit(unitId, player);
                players[player].queueUnits.RemoveAt(0);
            }else{
                
                AbortDeploy(player, 0);
            }

            UpdateDeployableFrames(player);
            UpdateQueuePanel(player);
                
            if(players[player].queueUnits.Count >= 1)
            {
                StartCoroutine(QueueUnit(players[player].queueUnits[0].id, player));
            }     
        }
        
        //Create unit in barracks
        public void SpawnUnit(int unitId, int player)
        {
            GameObject newObject = null;
            
            deployedUnits.Add(new UnitInstance(deployedUnits.Count, player, newObject = Instantiate(units[unitId].unitPrefab[player], barracks[player].transform.position+(barracks[player].transform.forward*3f), Quaternion.identity), newObject.AddComponent<NavMeshAgent>(), newObject.GetComponent<Animator>(), units[unitId]));
            
            UnitInstance newUnit = deployedUnits[deployedUnits.Count-1];
            
            MeleeWarriorUnitScript unit_mWUS = newObject.AddComponent<KevinIglesias.MeleeWarriorUnitScript>();

            unit_mWUS.coreScript = this;
            
            unit_mWUS.guid = deployedUnits.Count-1;
            unit_mWUS.id = unitId;
            unit_mWUS.owner = player;
            
            newUnit.navAgent.baseOffset = -0.04f;
            newUnit.navAgent.radius = 0.075f;
            newUnit.navAgent.speed = 2.75f;
            newUnit.navAgent.acceleration = 60f;
            newUnit.navAgent.stoppingDistance = 0.5f;
            newUnit.navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
        }

        //Abort unit creation (bottom left panel)
        public void AbortDeploy(int queueId, int player)
        {
            UnitDatabase unitData = units[players[player].queueUnits[queueId].id];
            
            players[player].gold += unitData.goldCost;
            players[player].wood += unitData.woodCost;
            players[player].food += unitData.foodCost;
            
            UpdateResources(player);

            players[player].queueUnits.RemoveAt(queueId);
            abortDeploy[player] = false;
            
            UpdateQueuePanel(player);
            UpdateDeployableFrames(player);
        }
        
        //Refresh unit creation queue (bottom left panel)
        void UpdateQueuePanel(int player)
        {
            if(selectedUnits.Count == 0)
            {
                return;
            }
            
            //Barracks
            if(!deployedUnits[selectedUnits[0]].canAttack)
            {     
                if(control == player)
                {
                    for(int i = 0; i < 4; i++)
                    {
                       deployInfo.transform.GetChild(3).GetChild(i).gameObject.SetActive(false);
                    }

                    for(int i = 1; i < players[player].queueUnits.Count; i++)
                    {
                       deployInfo.transform.GetChild(3).GetChild(i-1).gameObject.SetActive(true);
                       
                       MeleeWarriorUnitScript unit_mWUS = deployInfo.transform.GetChild(3).GetChild(i-1).GetChild(0).GetComponent<MeleeWarriorUnitScript>();
                       unit_mWUS.owner = player;
                       unit_mWUS.id = i;
                       unit_mWUS.GetComponent<Image>().sprite = units[players[player].queueUnits[i].id].icon[player];
                    }
                    
                    if(players[player].queueUnits.Count >= 1)
                    {
                        deployInfo.SetActive(true);
                        MeleeWarriorUnitScript unit_mWUS = deployInfo.transform.GetChild(2).GetChild(0).GetComponent<MeleeWarriorUnitScript>();
                        unit_mWUS.owner = player;
                        unit_mWUS.GetComponent<Image>().sprite = units[players[player].queueUnits[0].id].icon[player];
                    }else{
                        deployInfo.SetActive(false);
                    }
                }
            }else{
                deployInfo.SetActive(false);
            }
        }

        //Refresh unit creation bar (bottom left panel)
        void UpdateDeployBar(int player)
        {
            if(players[player].queueUnits.Count >= 1)
            {
                if(control == player)
                {
                    deployValue.text = players[player].queueUnits[0].queueValue+"%";
                    deployBar.GetComponent<RectTransform>().localScale = new Vector3(players[player].queueUnits[0].queueValue/100f, 1, 1);   
                }
            }
        }
       
        //Double click check for multiple selection (only same type of unit)
        IEnumerator DoubleClickCheck(int unitGuid, int previousSelectedGuid, int player)
        {
           float i = 0;
           while(i < 1)
           {
               i += Time.deltaTime * 3f;
               
               if(unitGuid == previousSelectedGuid && clickedTimes == 2)
               {
                   MultipleSelection(unitGuid, player);
                   break;
               }
               
               yield return 0;
           }
           clickedTimes--;
        }
       
        //Multiple selection of same type unit
        void MultipleSelection(int unitGuid, int player)
        {
            for(int i = 0; i < deployedUnits.Count; i++)
            {
                if(i != unitGuid)
                {
                    if(deployedUnits[i].owner == player && deployedUnits[i].id == deployedUnits[unitGuid].id)
                    {
                        if(!deployedUnits[i].selected)
                        {
                           selectedUnits.Add(deployedUnits[i].guid);
                           SelectUnit(deployedUnits[i].guid);
                        }
                    }
                }    
            }     
           
            if(selectedUnits.Count > 1)
            {
                multipleSelectionInfo.SetActive(true);
                singleSelectionInfo.SetActive(false);
                
                LoadMultipleFrames(player);
                
            }else{
                singleSelectionInfo.SetActive(true);
                multipleSelectionInfo.SetActive(false);
            } 
        }

        //Load unit info of selected units (bottom left panel)
        void LoadMultipleFrames(int player)
        {
            for(int i = 0; i < 18; i++)
            {
               multipleSelectionInfo.transform.GetChild(i).gameObject.SetActive(false);
            }
           
            for(int i = 0; i < selectedUnits.Count; i++)
            {
               MeleeWarriorUnitScript unit_mWUS = multipleSelectionInfo.transform.GetChild(i).GetChild(0).GetComponent<MeleeWarriorUnitScript>();
               unit_mWUS.id = selectedUnits[i];
               multipleSelectionInfo.transform.GetChild(i).gameObject.SetActive(true);
               multipleSelectionInfo.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = units[deployedUnits[selectedUnits[i]].id].icon[player];
               multipleSelectionInfo.transform.GetChild(i).GetChild(0).GetChild(0).GetChild(0).localScale = new Vector3((deployedUnits[selectedUnits[i]].currentHP/ (float)deployedUnits[selectedUnits[i]].maxHP), 1, 1);
            }  
        }
       
        //Change game cursor (arrow = default, sword = attack possible)
        public void ChangeCursor(bool attack)
        {
           if(attack)
           {
               Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
           }else{
               Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
           }
        }
       
        //Unclick selected units
        public void Unclick()
        {
            Unselect();
           
            multipleSelectionInfo.SetActive(false);
            singleSelectionInfo.SetActive(false);
            deployMenu[1].SetActive(false);
            deployMenu[0].SetActive(false);
           
            if(flagPath != null)
            {
                Destroy(flagPath);
            }
        }
       
        //Move selected units
        public void MoveUnit(int unitGuid, int player)
        {
            bool clickInBuilding = false;
            if(selectedUnits.Count > 0)
            {
                if(deployedUnits[selectedUnits[0]].canAttack)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        newPosition = hit.point;
             
                        //Not click in Ground (Ground Guid = -1)
                        if(unitGuid != -1)
                        {
                            if(!deployedUnits[unitGuid].canAttack)
                            {
                                clickInBuilding = true;
                            }
                        }

                        for(int i = 0; i < deployedUnits.Count; i++)
                        {
                            if(deployedUnits[i].selected)
                            {
                                deployedUnits[i].navAgent.enabled = true;
                                
                                if(clickInBuilding)
                                {
                                    //Find Closest Point to Building
                                    Vector3 heading = deployedUnits[unitGuid].unitObject.transform.position - deployedUnits[i].unitObject.transform.position;

                                    Collider c = deployedUnits[unitGuid].unitObject.GetComponent<Collider>();

                                    newPosition = c.ClosestPoint(deployedUnits[i].unitObject.transform.position);
                                }
                                
                                deployedUnits[i].navAgent.SetDestination(new Vector3(newPosition.x, 0, newPosition.z));

                                deployedUnits[i].isMoving = true;
                                deployedUnits[i].destination = new Vector3(newPosition.x, 0, newPosition.z);
                                
                                deployedUnits[i].anim.SetBool("Walk", true);

                                if(deployedUnits[i].pathFinding != null)
                                {
                                    StopCoroutine(deployedUnits[i].pathFinding);
                                }
                                
                                if(unitGuid != selectedUnit)
                                {
                                    LookAtClamp(deployedUnits[i].unitObject.transform, deployedUnits[i].destination);
                                }
                                deployedUnits[i].pathFinding = CheckDestination(deployedUnits[i], clickInBuilding);
                                StartCoroutine(deployedUnits[i].pathFinding);
                            }
                        }
                       CreateFlagPath(new Vector3(newPosition.x, 0, newPosition.z), player);
                    }
                }
            }
        }
        
        //Try to reach unit move destination
        IEnumerator CheckDestination(UnitInstance unitToMove, bool buildingHit)
        {
            unitToMove.target = -1;
            unitToMove.anim.SetBool("Attack", false);
            
            if(unitToMove.isAttacking)
            {
                unitToMove.anim.SetTrigger("Quit");
            }
            unitToMove.isAttacking = false;

            float stopDistance = 0.5f;
            
            if(selectedUnits.Count > 1)
            {
                stopDistance = 1f;
            }
            
            if(buildingHit)
            {
                stopDistance = stopDistance + 0.5f;
            }
  
            while(Vector3.Distance(unitToMove.destination, unitToMove.unitObject.transform.position) > stopDistance)
            {

                yield return 0;
            }
            
            unitToMove.anim.SetBool("Walk", false);
            
            unitToMove.isMoving = false;

            unitToMove.navAgent.enabled = false;
            
            yield return new WaitForSeconds(0.15f);
            
            if(flagPath != null)
            {
                if(selectedUnit == unitToMove.guid)
                {
                    Destroy(flagPath);
                }
            }
        }

        //Create flag of moving unit at its destination
        public void CreateFlagPath(Vector3 position, int player)
        {
            if(flagPath != null)
            {
                Destroy(flagPath);
            }
            flagPath = Instantiate(pathFlags[player], position, Quaternion.identity);
        }
        
        //Prepare unit attack (movement to reach target)
        public void PrepareAttack(int targetGuid)
        {
            if(flagPath != null)
            {
                Destroy(flagPath);
            }
            
            if(attackCo != null)
            {
                if(attackCOguid != -1)
                {
                    if(!deployedUnits[attackCOguid].selected)
                    {
                        deployedUnits[attackCOguid].selectionObject.SetActive(false);
                    }
                    deployedUnits[attackCOguid].selectionObject.GetComponent<Renderer>().material.color = Color.white;
                }
                StopCoroutine(attackCo);
            }
            
            //Attack red indicator
            attackCo = ShowAttackIndicator(targetGuid);
            StartCoroutine(attackCo);
            
            for(int i = 0; i < deployedUnits.Count; i++)
            {
                if(deployedUnits[i].selected)
                {
                    Attack(deployedUnits[i], targetGuid);
                }
            }
        }
        
        //Try to move unit to target
        void Attack(UnitInstance unit, int targetGuid)
        {
            unit.navAgent.enabled = true;
            unit.isMoving = true;
            
            unit.anim.SetBool("Walk", true);
            
            unit.isAttacking = true;
            unit.target = targetGuid;
            
            if(unit.pathFinding != null)
            {
                StopCoroutine(unit.pathFinding);           
            }                 
            unit.pathFinding = MoveAttackingUnit(unit);
            StartCoroutine(unit.pathFinding);
        }

        //Check if target position is reached and attack if so
        IEnumerator MoveAttackingUnit(UnitInstance attackerUnit)
        {
            while(attackerUnit.isAttacking)
            {
                if(deployedUnits[attackerUnit.target].canAttack)
                {
                    if(Vector3.Distance(deployedUnits[attackerUnit.target].unitObject.transform.position, attackerUnit.unitObject.transform.position) > attackerUnit.attackRange)
                    {
                        if(!attackerUnit.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack 01") && !attackerUnit.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack 02"))
                        {
                            LookAtClamp(attackerUnit.unitObject.transform, deployedUnits[attackerUnit.target].unitObject.transform.position);
                            attackerUnit.isMoving = true;
                            attackerUnit.anim.SetBool("Walk", true);
                            attackerUnit.navAgent.enabled = true;

                            Vector3 closestPoint = deployedUnits[attackerUnit.target].unitObject.GetComponent<Collider>().ClosestPointOnBounds(attackerUnit.unitObject.transform.position);
                            
                            attackerUnit.navAgent.SetDestination(new Vector3(closestPoint.x, 0, closestPoint.z));
                        }else{
                            attackerUnit.anim.SetBool("Attack", false);
                        }
                    }else{
                        attackerUnit.isMoving = false;
                        LookAtClamp(attackerUnit.unitObject.transform, deployedUnits[attackerUnit.target].unitObject.transform.position);
                        attackerUnit.navAgent.enabled = false;
                        attackerUnit.anim.SetBool("Walk", false);
                        attackerUnit.anim.SetBool("Attack", true);
                        
                        //Counterattack
                        if(!deployedUnits[attackerUnit.target].isMoving && !deployedUnits[attackerUnit.target].isAttacking)
                        {
                            yield return new WaitForSeconds(0.1f);
                            Attack(deployedUnits[attackerUnit.target], attackerUnit.guid);
                        }
                    }
                }else{
                    //Find Closest Point to Building
                    Vector3 heading = deployedUnits[attackerUnit.target].unitObject.transform.position - attackerUnit.unitObject.transform.position;

                    Collider c = deployedUnits[attackerUnit.target].unitObject.GetComponent<Collider>();

                    Vector3 buildingClosestPoint = c.ClosestPoint(attackerUnit.unitObject.transform.position);
                    
                    if(Vector3.Distance(buildingClosestPoint, attackerUnit.unitObject.transform.position) > (attackerUnit.attackRange *0.75f))
                    {
                        
                        if(!attackerUnit.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack 01") && !attackerUnit.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack 02"))
                        {
                            LookAtClamp(attackerUnit.unitObject.transform, deployedUnits[attackerUnit.target].unitObject.transform.position);
                            attackerUnit.isMoving = true;
                            attackerUnit.anim.SetBool("Walk", true);
                            attackerUnit.navAgent.enabled = true;

      
                            attackerUnit.navAgent.SetDestination(new Vector3(buildingClosestPoint.x, 0, buildingClosestPoint.z));
                        }else{
                            attackerUnit.anim.SetBool("Attack", false);
                        }
                    }else{
                        attackerUnit.isMoving = false;
                        LookAtClamp(attackerUnit.unitObject.transform, deployedUnits[attackerUnit.target].unitObject.transform.position);
                        attackerUnit.navAgent.enabled = false;
                        attackerUnit.anim.SetBool("Walk", false);
                        attackerUnit.anim.SetBool("Attack", true);
                    }
                }
                yield return 0;
            }
        }
        
        //Red attack indicator (square target ground selection)
        IEnumerator ShowAttackIndicator(int targetGuid)
        {
            attackCOguid = targetGuid;
            
            if(deployedUnits[attackCOguid].selectionObject == null)
            {
               deployedUnits[attackCOguid].selectionObject = Instantiate(selectionPrefab);
               deployedUnits[attackCOguid].selectionObject = deployedUnits[attackCOguid].selectionObject;
               deployedUnits[attackCOguid].selectionObject.transform.SetParent(deployedUnits[attackCOguid].unitObject.transform);
               deployedUnits[attackCOguid].selectionObject.transform.localScale = new Vector3(units[deployedUnits[attackCOguid].id].selectionSize, units[deployedUnits[attackCOguid].id].selectionSize, units[deployedUnits[attackCOguid].id].selectionSize);
               deployedUnits[attackCOguid].selectionObject.transform.localPosition = new Vector3(0, 0.05f, 0);
               deployedUnits[attackCOguid].selectionObject.transform.localEulerAngles = Vector3.zero;
            }
            
            Material mat = deployedUnits[attackCOguid].selectionObject.GetComponent<Renderer>().material;
            
            mat.color = Color.red;

            deployedUnits[attackCOguid].selectionObject.SetActive(true);
            
            for(int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(0.15f);
                
                deployedUnits[attackCOguid].selectionObject.SetActive(false);
                
                yield return new WaitForSeconds(0.15f);
            
                deployedUnits[attackCOguid].selectionObject.SetActive(true);
                
            }
            yield return new WaitForSeconds(0.2f);
            
            deployedUnits[attackCOguid].selectionObject.SetActive(false);
            
            mat.color = Color.white;
            
            attackCOguid = -1;
        }

        //Take HP from target when attacking
        public void TakeHP(int targetGuid, int attackerGuid)
        {
            if(!deployedUnits[attackerGuid].isAttacking)
            {
                return;
            }
                        
            int attack = deployedUnits[attackerGuid].attack;
            int defense = deployedUnits[targetGuid].defense;
            int hpTaken = attack - defense;
            
            if(hpTaken <= 0)
            {
                hpTaken = 1;
            }
            
            deployedUnits[targetGuid].currentHP -= hpTaken;

            if(deployedUnits[targetGuid].currentHP <= 0)
            {
                deployedUnits[targetGuid].currentHP = 0;
                KillUnit(targetGuid, attackerGuid);
                return;
            }
            
            if(deployedUnits[targetGuid].selected)
            {
                
                if(selectedUnits.Count > 1)
                {
                    LoadMultipleFrames(deployedUnits[targetGuid].owner);
                }else{
                    selectedUnitHP.text = deployedUnits[targetGuid].currentHP+"/"+deployedUnits[targetGuid].maxHP;

                }
            }

            if(deployedUnits[targetGuid].HPObject == null)
            {
                deployedUnits[targetGuid].HPObject = Instantiate(HPPrefab);
                deployedUnits[targetGuid].HPObject.transform.SetParent(deployedUnits[targetGuid].unitObject.transform);
                deployedUnits[targetGuid].HPObject.transform.localPosition = new Vector3(0, units[deployedUnits[targetGuid].id].hpBarHeight, 0);
                deployedUnits[targetGuid].HPObject.SetActive(false);
            }
            
            //Map HP bar (over unit HP bar)
            Transform t = deployedUnits[targetGuid].HPObject.transform.GetChild(0);

            t.localScale = new Vector3((deployedUnits[targetGuid].currentHP/ (float)deployedUnits[targetGuid].maxHP), 1, 1);

            if(selectedUnit == targetGuid)
            {
                //UI HP Bar (bottom left panel)
                selectedUnitHP.transform.parent.GetChild(0).localScale = new Vector3((deployedUnits[targetGuid].currentHP / (float)deployedUnits[targetGuid].maxHP), 1, 1);
            }
        }
        
        //Kill target when HP is 0
        public void KillUnit(int targetGuid, int attackerGuid)
        {
            bool multipleSelection = false;
            if(selectedUnits.Count > 1)
            {
                multipleSelection = true;
                for(int i = 0; i < selectedUnits.Count; i++)
                {
                    if(selectedUnits[i] == targetGuid)
                    {
                        selectedUnits.RemoveAt(i);
                    }
                    
                    if(selectedUnits[i] > targetGuid)
                    {
                        selectedUnits[i]--;
                    }   

                }
 
                if(selectedUnit >= selectedUnits.Count)
                {
                    selectedUnit = selectedUnits.Count-1;
                }

                deployedUnits[targetGuid].selectionObject.SetActive(false);
                deployedUnits[targetGuid].HPObject.SetActive(false);
                 
                if(selectedUnits.Count == 1)
                {
                    multipleSelectionInfo.SetActive(false);
                    singleSelectionInfo.SetActive(true);
                    LoadUnitUI(selectedUnits[0], deployedUnits[selectedUnits[0]].owner);
                }
                
            }else{
                
                if(selectedUnit == targetGuid)
                {
                    selectedUnit = -1;
                    Unclick();
                }else{
                    
                    if(targetGuid < attackerGuid)
                    {
                        selectedUnit--;
                    }
                    
                    if(selectedUnits.Count > 0)
                    {
                        if(selectedUnits[0] > targetGuid)
                        {
                            selectedUnits[0]--;
                        } 
                    }
                }
            }
            
            if(attackCOguid == targetGuid)
            {
                deployedUnits[targetGuid].selectionObject.SetActive(false);
                if(attackCo != null)
                {
                    StopCoroutine(attackCo);
                }
                attackCOguid = -1;
            }
            
            if(attackCOguid > targetGuid)
            {
                attackCOguid--;
            }
            
            for(int i = 0; i < deployedUnits.Count; i++)
            {
                if(deployedUnits[i].target == targetGuid)
                {
                    if(deployedUnits[i].pathFinding != null)
                    {
                        StopCoroutine(deployedUnits[i].pathFinding);
 
                    }
                    deployedUnits[i].isAttacking = false;
                    deployedUnits[i].anim.SetBool("Attack", false);
                    deployedUnits[i].anim.SetBool("Walk", false);
                }
            }
            
            if(deployedUnits[targetGuid].pathFinding != null)
            {
                StopCoroutine(deployedUnits[targetGuid].pathFinding);
            }
            
            deployedUnits[targetGuid].unitObject.GetComponent<Collider>().enabled = false;
            
            if(deployedUnits[targetGuid].canAttack)
            {
                StartCoroutine(DestroyBody(deployedUnits[targetGuid].unitObject));
                
                deployedUnits[targetGuid].navAgent.enabled = false;
            
                deployedUnits[targetGuid].anim.SetBool("Death", true);
            }else{
                StartCoroutine(DestroyBuilding(deployedUnits[targetGuid].unitObject));
            }
            
            deployedUnits[attackerGuid].target = -1;
            RelocateDatabase(targetGuid);
            deployedUnits.RemoveAt(targetGuid);
            
            if(multipleSelection)
            {
                LoadMultipleFrames(control);
            }
        }
        
        //Destroy unit body
        IEnumerator DestroyBody(GameObject unit)
        {
            Transform t = unit.transform;
            float initPos = t.localPosition.y;
            float i = 0;
            yield return new WaitForSeconds(4f);
            while(i < 1)
            {
                i += Time.deltaTime * 0.75f;
                
                t.localPosition = new Vector3(t.localPosition.x, Mathf.Lerp(initPos, -1, i), t.localPosition.z);
                
                yield return 0;
            }
            Destroy(unit);
        }
        
        //Destroy building unit
        IEnumerator DestroyBuilding(GameObject unit)
        {
            Transform t = unit.transform;
            float initPos = t.localPosition.y;
            for(int i = 0; i < 5; i++)
            {
                t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, t.localEulerAngles.z-5);
                
                yield return new WaitForSeconds(0.1f);
                
                t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, t.localEulerAngles.z+5);
                
                yield return new WaitForSeconds(0.1f);
            }
            float j = 0;
            while(j < 1)
            {
                j += Time.deltaTime * 2;
                t.localPosition = new Vector3(t.localPosition.x, Mathf.Lerp(initPos, -5, j), t.localPosition.z);
                yield return 0;
            }
            Destroy(unit);
        }
        
        //Needed when some unit dies, change indexes of deployed units database
        public void RelocateDatabase(int targetGuid)
        {
            for(int i = 0; i < deployedUnits.Count; i++)
            {
                if(i > targetGuid)
                {
                    deployedUnits[i].guid--;
                    deployedUnits[i].unitObject.GetComponent<MeleeWarriorUnitScript>().guid--;
                }
                   
                if(deployedUnits[i].target > targetGuid)
                {
                    deployedUnits[i].target--;
                }
            }
        }

        //Look at target when moving to path or attacking
        public void LookAtClamp(Transform t, Vector3 point)
        {
            t.LookAt(point);
            t.localEulerAngles = new Vector3(0, t.localEulerAngles.y, t.localEulerAngles.z);
        }
    }
}