using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KevinIglesias {
    
    public class MeleeWarriorUnitScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

        public MeleeWarriorPlayableScene coreScript;
        public int owner;
        public int guid;
        public int id;
        public bool isQueueButton;
        public bool isMultipleSelection;

        //When mouse is clicked down (map)
        public void OnMouseDown()
        {
                if(!EventSystem.current.IsPointerOverGameObject())
                {
                    if(owner == -1)
                    {
                        coreScript.Unclick();
                        return;
                    }
                
                    coreScript.ChangeCursor(false);

                    coreScript.ClickUnit(guid, owner);
                }
        }
        
        //When mouse is over something (map)
        public void OnMouseOver()
        {

           if(!EventSystem.current.IsPointerOverGameObject())
           {
            coreScript.descriptionInfo.SetActive(false); 
           }
            
            if(owner != -1)
            {
                if(coreScript.control != -1)
                {
                    if(coreScript.control != owner)
                    {
                        if(coreScript.selectedUnits.Count > 0)
                        {
                            if(coreScript.deployedUnits[coreScript.selectedUnits[0]].canAttack)
                            {
                                coreScript.ChangeCursor(true);
                                
                                if(Input.GetMouseButtonDown(1))
                                {
                                    coreScript.PrepareAttack(guid);
                                }
                            }
                        }
                    }else{
                        if(Input.GetMouseButtonDown(1))
                        {
                            coreScript.MoveUnit(guid, coreScript.control);
                        }
                    }
                }
            }else{
                if(Input.GetMouseButtonDown(1))
                {
                    coreScript.MoveUnit(guid, coreScript.control);
                }
            }
        }
        
        //When mouse is exits something (map)
        public void OnMouseExit()
        {
            coreScript.ChangeCursor(false);
        }
        
        //When mouse is over something (UI)
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!isMultipleSelection && !isQueueButton)
            {
                coreScript.ShowUnitInfo(id);
            }
        }
        
        //When mouse exits something (UI)
        public void OnPointerExit(PointerEventData eventData)
        {
            coreScript.descriptionInfo.SetActive(false);
        }
        
        //When mouse is clicked down (UI)
        public void OnPointerDown(PointerEventData eventData) 
        {
            if(Input.GetMouseButtonDown(0))
            {
            
                if(isMultipleSelection)
                {
                    transform.localScale = new Vector3(0.975f, 0.975f, 0.975f);
                    return;
                }
                
                if(isQueueButton)
                {
                    transform.localScale = new Vector3(0.975f, 0.975f, 0.975f);
                    return;
                }
                
                if(coreScript.CheckIfDeployable(id, owner))
                {
                    transform.localScale = new Vector3(0.975f, 0.975f, 0.975f);
                }
            }
        }
        
        //When mouse click released (UI)
        public void OnPointerUp(PointerEventData eventData) 
        {
            if(Input.GetMouseButtonUp(0))
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            
                if(isMultipleSelection)
                {
                    
                    coreScript.ClickUnit(id, owner);
                    
                }else{
                
                    if(isQueueButton)
                    {
                        if(id == 0)
                        {
                            coreScript.abortDeploy[owner] = true;
                        }else{
                            coreScript.AbortDeploy(guid, owner);
                        }
                        return;
                    }
                    
                    if(coreScript.CheckIfDeployable(id, owner))
                    {
                        coreScript.DeployUnit(id, owner);
                    }
                }
            }
        }
        
        //When animator script sends attack
        public void Attack()
        {
           if(coreScript.deployedUnits.Count > guid)
           {
               int targetGuid = coreScript.deployedUnits[guid].target;

                coreScript.TakeHP(targetGuid, guid);
           }  
        }
    }
}
