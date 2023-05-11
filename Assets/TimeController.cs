using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


public class TimeController : MonoBehaviour
{
    private const string _webService = "http://worldtimeapi.org/api/timezone/Europe/Moscow";
    private const string _fileName = "/MoscowTime.html";
    private string _filePath;

    private void Awake()
    {
        _filePath = Application.persistentDataPath + _fileName;
    }
    
    public async void GetMoscowTime()
    {
        using var unityWebRequest = UnityWebRequest.Get(_webService);
        var operation = unityWebRequest.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (unityWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error retrieving Moscow time: " + unityWebRequest.error);
        }

        var jsonResponse = unityWebRequest.downloadHandler.text;
        
        try
        {
            var timeData = JsonConvert.DeserializeObject<TimeData>(jsonResponse);

            var formattedTime = FormatTime(timeData.DateTime);
            var htmlContent = GetPopUpWindowContent(formattedTime);

            await File.WriteAllTextAsync(_filePath, htmlContent);
            Application.OpenURL(_filePath);
        }
        catch (Exception e)
        {
            Debug.Log($"Could not parse {jsonResponse}. {e.Message}");
            throw;
        }
    }
    
    private string FormatTime(string dateTime)
    {
        var parts = dateTime.Split('T');
        var time = parts[1].Substring(0, 8);
        return time;
    }

    private string GetPopUpWindowContent(string content)
    {
        return $"<html><body><script>alert('Moscow time: {content}');</script></body></html>";
    }

    [Serializable]
    private class TimeData
    {
        public string DateTime;
    }
}

