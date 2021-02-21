using UnityEngine;

public class QuestArrowRotator : MonoBehaviour
{
    public bool isTracking;
    [SerializeField] private GameObject Arrow;

    private void Update()
    {
        if (isTracking)
        {
            Arrow.SetActive(true);
            var mgs = GameManager.Instance.MyMinigames;
            var min = 1000000f;
            var minI = -1;
            for (var i = 0; i < mgs.Count; i++)
            {
                if(mgs[i].isComplete) continue;
                var dist = Vector3.Distance(mgs[i].transform.position, transform.position);
                if (!(dist < min)) continue;
                minI = i;
                min = dist;
            }

            if (minI == -1)
            {
                isTracking = false;
                return;
            }
            var targetPostition = new Vector3( 
                mgs[minI].transform.position.x, 
                transform.position.y, 
                mgs[minI].transform.position.z);
            transform.LookAt(targetPostition);
        }
        else
        {
            Arrow.SetActive(false);
        }
    }
}
