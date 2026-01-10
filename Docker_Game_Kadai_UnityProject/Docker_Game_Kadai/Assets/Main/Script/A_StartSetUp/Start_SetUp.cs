using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DebugAndTest;

public class Start_SetUp : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Debug.Log("実行開始時に呼ばれた (AfterSceneLoad)");

        // デバッグ用サーバー通信テストオブジェクトを作成.
        CreateDebugServerTransmissionObject();

        // StartCoroutine/UniTaskで非同期呼び出し.
        _ = InitAsync();
    }

    /// <summary>
    /// デバッグ用サーバー通信テストオブジェクトを作成する.
    /// </summary>
    static void CreateDebugServerTransmissionObject()
    {
        var debugObj = new GameObject("Debug_Test_Server_Transmission");
        debugObj.AddComponent<Debug_Test_Server_Transmission>();
        DontDestroyOnLoad(debugObj);
        Debug.Log("[Start_SetUp] Debug_Test_Server_Transmission オブジェクトを作成しました.");
    }

    static async UniTaskVoid InitAsync()
    {
        await UniTask.Yield();
    }

}
