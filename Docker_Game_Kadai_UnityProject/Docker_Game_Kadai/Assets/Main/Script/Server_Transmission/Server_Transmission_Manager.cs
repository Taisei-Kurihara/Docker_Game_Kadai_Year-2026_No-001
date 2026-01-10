using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Common;

namespace ServerTransmission
{
    /// <summary>
    /// 通信内容のenum.
    /// </summary>
    public enum TransmissionDataType
    {
        Test,       // テストデータ.
        Player,     // プレイヤーデータ.
        Game,       // ゲームデータ.
        Config      // 設定データ.
    }

    /// <summary>
    /// サーバー通信管理クラス.
    /// </summary>
    public class Server_Transmission_Manager : Singleton_MonoBehaviourBase<Server_Transmission_Manager>
    {
        private const string SERVER_URL = "http://localhost:5000";

        /// <summary>
        /// 指定したデータタイプのデータをサーバーから取得する.
        /// </summary>
        public void GetData(TransmissionDataType dataType, Action<string> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(GetDataCoroutine(dataType, onSuccess, onError));
        }

        private IEnumerator GetDataCoroutine(TransmissionDataType dataType, Action<string> onSuccess, Action<string> onError)
        {
            string endpoint = $"{SERVER_URL}/api/data/{dataType.ToString().ToLower()}";

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    string errorMessage = $"Error: {request.error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            }
        }

        /// <summary>
        /// サーバーにデータを送信する.
        /// </summary>
        public void SendData(string jsonData, Action<string> onSuccess, Action<string> onError = null)
        {
            StartCoroutine(SendDataCoroutine(jsonData, onSuccess, onError));
        }

        private IEnumerator SendDataCoroutine(string jsonData, Action<string> onSuccess, Action<string> onError)
        {
            string endpoint = $"{SERVER_URL}/api/data";

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    string errorMessage = $"Error: {request.error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            }
        }

        /// <summary>
        /// ヘルスチェックを行う.
        /// </summary>
        public void HealthCheck(Action<bool> onResult)
        {
            StartCoroutine(HealthCheckCoroutine(onResult));
        }

        private IEnumerator HealthCheckCoroutine(Action<bool> onResult)
        {
            string endpoint = $"{SERVER_URL}/api/health";

            using (UnityWebRequest request = UnityWebRequest.Get(endpoint))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();

                onResult?.Invoke(request.result == UnityWebRequest.Result.Success);
            }
        }
    }
}
