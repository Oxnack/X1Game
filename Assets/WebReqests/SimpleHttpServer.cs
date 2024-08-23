using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using Mirror;
using JetBrains.Annotations;

public class SimpleHttpServer : NetworkBehaviour
{
    private HttpListener httpListener;

    void Start()
    {
        if (isServer) // Убедитесь, что сервер запускается только на стороне сервера
        {
            StartServer();
        }
    }

    private void StartServer()
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://webreqest.oxnack.ru:8080/"); // Задайте URL и порт
        httpListener.Start();
        Debug.Log("Сервер запущен...");
        ListenForRequests();
    }

    private async void ListenForRequests()
    {
        while (true)
        {
            try
            {
                var context = await httpListener.GetContextAsync();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка при обработке запроса: " + ex.Message);
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        string responseString = "";

        if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/api/upload")
        {
            // Проверяем Content-Type для определения типа данных
            if (context.Request.ContentType == "application/json")
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    string jsonData = reader.ReadToEnd(); // Читаем тело запроса
                    Debug.Log("Полученные JSON-данные: " + jsonData);

                    MoveData cubeData = JsonConvert.DeserializeObject<MoveData>(jsonData);

                    Debug.Log(cubeData.username);

                    PlayerName[] names = FindObjectsOfType<PlayerName>();

                    GameObject player = null;

                    foreach (PlayerName name in names)
                    {
                        if (name.Name == cubeData.username)
                        {
                            player = name.gameObject;
                        }
                    }

                    if (player != null)
                    {
                        MoveRobotCmd(player.GetComponent<RoboMove>(), cubeData.time, cubeData.z, cubeData.x);
                        responseString = "{\"message\": \"JSON file received\"}";
                    }
                    else
                    {
                        responseString = "{\"message\": \"BadNickname\"}";
                    }


                }
            }
            else
            {
                responseString = "{\"error\": \"Invalid content type\"}";
            }
        }
        else
        {
            responseString = "{\"error\": \"404 Not Found\"}";
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    [Server] // Убедитесь, что этот метод вызывается только на сервере
    private void MoveRobotCmd(RoboMove roboMove, float time, int z, int x)
    {
        roboMove.Move(time, z, x);
    }

    private void OnApplicationQuit()
    {
        if (httpListener != null)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}

[System.Serializable]
public class MoveData
{
    public string username;
    public float time;
    public int z;
    public int x;
}
