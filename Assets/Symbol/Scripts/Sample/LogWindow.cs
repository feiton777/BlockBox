using System.Text;
using UnityEngine;
using SB;

public class LogWindow : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text LogText;

    string LogString;

    StringBuilder LogStringBuilder = new StringBuilder();

    private void Awake()
    {
        Application.logMessageReceived += OnReceiveLog;
    }

    //ÉçÉOÇéÛÇØéÊÇ¡ÇΩ
    private void OnReceiveLog( string logText, string stackTrace, LogType logType )
    {
        if(logText.Contains( $"{SymbolCommonManager.SymbolLogKey}" ))
        {
            string addText = logText.Replace( $"{SymbolCommonManager.SymbolLogKey}", "" );
            LogString = $"{addText}\n{LogString}";
            //LogString = $"\n================\nlogText\n{logText}\n\nLogType\n{logType}\n\nstackTrace\n{stackTrace}\n{LogString}";
            LogText.text = LogString;
        }
    }

}
