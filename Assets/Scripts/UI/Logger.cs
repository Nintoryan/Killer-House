using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private TMP_Text _log;
    public static Logger Instance;
    private List<string> _Logs = new List<string>();
    [SerializeField] private int LogsSize = 4;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _log = GetComponentInChildren<TMP_Text>();
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
    
    
    
    public void Log(object LogData)
    {
        Debug.Log(LogData.ToString());
        _Logs.Add(LogData.ToString());
        if (_Logs.Count >= LogsSize)
        {
            _Logs.RemoveAt(0);
        }
        var fullLog = _Logs.Aggregate("", (current, log) => current + (log + "\n"));
        _log.text = fullLog;
    }
}
