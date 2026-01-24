using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace DebugAndTest
{
    /// <summary>
    /// サーバーからのガチャ結果データ.
    /// </summary>
    [Serializable]
    public class GachaPullResponse
    {
        public List<GachaCharacterData> results;
        public Dictionary<string, float> weights;
    }

    /// <summary>
    /// サーバーからのキャラクターデータ.
    /// </summary>
    [Serializable]
    public class GachaCharacterData
    {
        public int masternumber;
        public int rarity;
        public string name;
        public int type;
    }

    /// <summary>
    /// サーバーからの重みデータ.
    /// </summary>
    [Serializable]
    public class GachaWeightsResponse
    {
        public Dictionary<string, float> weights;
    }

    /// <summary>
    /// サーバーからのキャラクターリストデータ.
    /// </summary>
    [Serializable]
    public class GachaCharactersResponse
    {
        public List<GachaCharacterData> characters;
    }

    /// <summary>
    /// ガチャテスト用クラス (test用).
    /// </summary>
    public class Debug_Test_Gacha : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _displayText;

        private const string SERVER_URL = "http://localhost:5000";

        // キャッシュ用.
        private Dictionary<int, float> _cachedWeights;
        private List<GachaCharacterData> _cachedCharacters;

        // ガチャ結果のレアリティ集計用.
        private Dictionary<int, int> _pulledRarityCounts = new Dictionary<int, int>();
        private int _totalPullCount = 0;

        private void Update()
        {
            if (Keyboard.current == null) return;

            // Xキー: ガチャを引いて結果をログ.
            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                StartCoroutine(PullGachaFromServer());
            }

            // Cキー: レアリティごとの確率をログ.
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                StartCoroutine(LogRarityProbabilitiesFromServer());
            }

            // Vキー: キャラごとの確率をログ.
            if (Keyboard.current.vKey.wasPressedThisFrame)
            {
                StartCoroutine(LogCharacterProbabilitiesFromServer());
            }

            // Bキー: ガチャ結果のレアリティ集計から重みづけ計算結果を表示.
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                LogPulledRarityWeights();
            }
        }

        /// <summary>
        /// サーバーからガチャを10回引く
        /// </summary>
        private IEnumerator PullGachaFromServer()
        {
            SetDisplayText("[Gacha] サーバーに通信中...");

            string json = "{\"count\": 10}";
            using (UnityWebRequest request = new UnityWebRequest($"{SERVER_URL}/api/gacha/pull", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetDisplayText($"[Gacha] 通信エラー: {request.error}");
                    yield break;
                }

                string responseJson = request.downloadHandler.text;
                var response = JsonUtility.FromJson<GachaPullResponseWrapper>(responseJson);

                if (response == null || response.results == null)
                {
                    SetDisplayText("[Gacha] レスポンス解析エラー");
                    yield break;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Gacha] 10連ガチャ結果:");

                for (int i = 0; i < response.results.Count; i++)
                {
                    var character = response.results[i];
                    sb.AppendLine($"  {i + 1}回目: {character.name} (レアリティ{character.rarity})");

                    // レアリティ集計に追加.
                    if (!_pulledRarityCounts.ContainsKey(character.rarity))
                    {
                        _pulledRarityCounts[character.rarity] = 0;
                    }
                    _pulledRarityCounts[character.rarity]++;
                    _totalPullCount++;
                }

                SetDisplayText(sb.ToString());
            }
        }

        /// <summary>
        /// サーバーからレアリティごとの確率を取得
        /// </summary>
        private IEnumerator LogRarityProbabilitiesFromServer()
        {
            SetDisplayText("[Gacha] サーバーに通信中...");

            using (UnityWebRequest request = UnityWebRequest.Get($"{SERVER_URL}/api/gacha/weights"))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetDisplayText($"[Gacha] 通信エラー: {request.error}");
                    yield break;
                }

                string responseJson = request.downloadHandler.text;
                var weights = ParseWeightsFromJson(responseJson);

                if (weights == null || weights.Count == 0)
                {
                    SetDisplayText("[Gacha] 重みデータ取得エラー");
                    yield break;
                }

                _cachedWeights = weights;
                float totalWeight = weights.Values.Sum();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Gacha] レアリティごとの確率:");

                foreach (var kvp in weights.OrderBy(x => x.Key))
                {
                    float probability = (kvp.Value / totalWeight) * 100f;
                    sb.AppendLine($"  レアリティ{kvp.Key}: {probability:F2}%");
                }

                SetDisplayText(sb.ToString());
            }
        }

        /// <summary>
        /// サーバーからキャラごとの確率を取得
        /// </summary>
        private IEnumerator LogCharacterProbabilitiesFromServer()
        {
            SetDisplayText("[Gacha] サーバーに通信中...");

            // 重みを取得.
            using (UnityWebRequest request = UnityWebRequest.Get($"{SERVER_URL}/api/gacha/weights"))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetDisplayText($"[Gacha] 通信エラー: {request.error}");
                    yield break;
                }

                _cachedWeights = ParseWeightsFromJson(request.downloadHandler.text);
            }

            // キャラリストを取得.
            using (UnityWebRequest request = UnityWebRequest.Get($"{SERVER_URL}/api/gacha/characters"))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    SetDisplayText($"[Gacha] 通信エラー: {request.error}");
                    yield break;
                }

                string responseJson = request.downloadHandler.text;
                var response = JsonUtility.FromJson<GachaCharactersResponseWrapper>(responseJson);

                if (response == null || response.characters == null)
                {
                    SetDisplayText("[Gacha] キャラデータ取得エラー");
                    yield break;
                }

                _cachedCharacters = response.characters;
            }

            if (_cachedWeights == null || _cachedCharacters == null)
            {
                SetDisplayText("[Gacha] データ取得エラー");
                yield break;
            }

            float totalWeight = _cachedWeights.Values.Sum();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Gacha] キャラごとの確率:");

            foreach (var rarityGroup in _cachedCharacters.GroupBy(c => c.rarity).OrderBy(g => g.Key))
            {
                int rarity = rarityGroup.Key;
                int charCount = rarityGroup.Count();

                if (!_cachedWeights.TryGetValue(rarity, out float rarityWeight))
                {
                    continue;
                }

                float rarityProbability = (rarityWeight / totalWeight) * 100f;
                float charProbability = rarityProbability / charCount;

                sb.AppendLine($"レアリティ{rarity} (レアリティ排出率{rarityProbability:F2}% / このレアリティのキャラ数{charCount} = 特定キャラ入手確率{charProbability:F4}%)");
                sb.AppendLine(string.Join(" ", rarityGroup.OrderBy(c => c.masternumber).Select(c => c.name)));
            }

            SetDisplayText(sb.ToString());
        }

        /// <summary>
        /// JSONから重みデータを解析する.
        /// </summary>
        private Dictionary<int, float> ParseWeightsFromJson(string json)
        {
            var result = new Dictionary<int, float>();
            try
            {
                // シンプルなパース ({"weights": {"1": 40.0, "2": 30.0, ...}}).
                int weightsStart = json.IndexOf("\"weights\"");
                if (weightsStart < 0) return result;

                int braceStart = json.IndexOf('{', weightsStart);
                int braceEnd = json.IndexOf('}', braceStart);
                string weightsJson = json.Substring(braceStart + 1, braceEnd - braceStart - 1);

                string[] pairs = weightsJson.Split(',');
                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim().Trim('"');
                        string value = keyValue[1].Trim();
                        if (int.TryParse(key, out int rarity) && float.TryParse(value, out float weight))
                        {
                            result[rarity] = weight;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Weight parse error: {e.Message}");
            }
            return result;
        }

        /// <summary>
        /// テキストを表示する.
        /// </summary>
        private void SetDisplayText(string text)
        {
            if (_displayText != null)
            {
                _displayText.text = text;
            }
            Debug.Log(text);
        }

        /// <summary>
        /// ガチャ結果のレアリティ集計から重みづけ計算結果を表示する.
        /// </summary>
        private void LogPulledRarityWeights()
        {
            if (_totalPullCount == 0)
            {
                SetDisplayText("[Gacha] まだガチャを引いていません (Xキーでガチャを引いてください)");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Gacha] ガチャ結果からの重みづけ計算 (総回数: {_totalPullCount}回):");
            sb.AppendLine();

            // 実測値から計算した確率.
            sb.AppendLine("【実測確率】");
            foreach (var kvp in _pulledRarityCounts.OrderBy(x => x.Key))
            {
                float actualProbability = (float)kvp.Value / _totalPullCount * 100f;
                sb.AppendLine($"  レアリティ{kvp.Key}: {kvp.Value}回 / {_totalPullCount}回 = {actualProbability:F2}%");
            }

            // サーバー設定値との比較.
            if (_cachedWeights != null && _cachedWeights.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("【サーバー設定確率との比較】");
                float totalWeight = _cachedWeights.Values.Sum();

                foreach (var kvp in _pulledRarityCounts.OrderBy(x => x.Key))
                {
                    float actualProbability = (float)kvp.Value / _totalPullCount * 100f;
                    float expectedProbability = 0f;

                    if (_cachedWeights.TryGetValue(kvp.Key, out float weight))
                    {
                        expectedProbability = (weight / totalWeight) * 100f;
                    }

                    float diff = actualProbability - expectedProbability;
                    string diffSign = diff >= 0 ? "+" : "";
                    sb.AppendLine($"  レアリティ{kvp.Key}: 実測{actualProbability:F2}% / 設定{expectedProbability:F2}% (差{diffSign}{diff:F2}%)");
                }
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("※サーバー設定確率と比較するには先にCキーで確率を取得してください");
            }

            SetDisplayText(sb.ToString());
        }
    }

    // JsonUtility用のラッパークラス.
    [Serializable]
    public class GachaPullResponseWrapper
    {
        public List<GachaCharacterData> results;
    }

    [Serializable]
    public class GachaCharactersResponseWrapper
    {
        public List<GachaCharacterData> characters;
    }
}
