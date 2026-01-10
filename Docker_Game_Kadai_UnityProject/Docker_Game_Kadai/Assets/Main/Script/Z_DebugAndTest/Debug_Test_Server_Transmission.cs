using UnityEngine;
using UnityEngine.InputSystem;
using ServerTransmission;

namespace DebugAndTest
{
    /// <summary>
    /// サーバー通信テスト用クラス.
    /// </summary>
    public class Debug_Test_Server_Transmission : MonoBehaviour
    {
        private void Update()
        {
            // Oキーが押された時にサーバーと通信してログを出力する.
            if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
            {
                TestServerTransmission();
            }
        }

        /// <summary>
        /// サーバー通信テストを実行する.
        /// </summary>
        private void TestServerTransmission()
        {
            Debug.Log("[Debug_Test] Oキーが押されました。サーバーに通信を開始します。");

            Server_Transmission_Manager.Instance().GetData(
                TransmissionDataType.Test,
                onSuccess: (response) =>
                {
                    Debug.Log($"[Debug_Test] サーバーからのレスポンス: {response}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"[Debug_Test] 通信エラー: {error}");
                }
            );
        }
    }
}
