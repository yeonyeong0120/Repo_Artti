// GeminiService.cs
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Artti.Training
{
    public class GeminiService : MonoBehaviour
    {
        [Header("API 설정")]
        [SerializeField] private string modelName = "gemini-2.5-flash-lite";

        private string apiKey;
        private string systemPrompt;
        private string cachedEndpointUrl;

        public void Initialize(string scenarioSystemPrompt)
        {
            systemPrompt = scenarioSystemPrompt;
            apiKey = LoadApiKey();

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("[Gemini] API 키 없음. Resources/gemini_key.txt 확인.");
                return;
            }

            cachedEndpointUrl =
                $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

            Debug.Log("[Gemini] 초기화 완료");
        }

        private string LoadApiKey()
        {
            var keyAsset = Resources.Load<TextAsset>("gemini_key");
            if (keyAsset == null)
            {
                Debug.LogError("[Gemini] Resources/gemini_key.txt 없음.");
                return null;
            }
            return keyAsset.text.Trim();
        }

        public async UniTask<GeminiJudgment> JudgeSpeech(
            string stepId, string expectedIntent, string userSpeech, CancellationToken ct)
        {
            string userMessage =
                $"[Step {stepId}]\n기대 발화 유형: {expectedIntent}\n실제 발화: \"{userSpeech}\"";

            string requestBody = BuildRequestJson(userMessage);

            try
            {
                string responseJson = await PostRequest(requestBody, ct);
                return ParseResponse(responseJson);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                Debug.LogWarning($"[Gemini] 판정 실패, 기본 응답 반환: {e.Message}");
                return new GeminiJudgment { pass = true, npcResponse = "(응답을 불러올 수 없습니다)" };
            }
        }

        private string BuildRequestJson(string userMessage)
        {
            string escapedSystem = EscapeJson(systemPrompt);
            string escapedUser   = EscapeJson(userMessage);

            return $@"{{
                ""system_instruction"": {{
                    ""parts"": [{{ ""text"": ""{escapedSystem}"" }}]
                }},
                ""contents"": [{{
                    ""role"": ""user"",
                    ""parts"": [{{ ""text"": ""{escapedUser}"" }}]
                }}],
                ""generationConfig"": {{
                    ""temperature"": 0.3,
                    ""maxOutputTokens"": 512,
                    ""responseMimeType"": ""application/json"",
                    ""thinkingConfig"": {{
                        ""thinkingBudget"": 0
                    }}
                }}
            }}";
        }

        private async UniTask<string> PostRequest(string body, CancellationToken ct)
        {
            using var request = new UnityWebRequest(cachedEndpointUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().WithCancellation(ct);

            Debug.Log($"[Gemini RAW] HTTP {request.responseCode}\n{request.downloadHandler.text}");

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP {request.responseCode}: {request.downloadHandler.text}");

            return request.downloadHandler.text;
        }

        private GeminiJudgment ParseResponse(string responseJson)
        {
            var wrapper = JsonUtility.FromJson<GeminiResponseWrapper>(responseJson);

            if (wrapper?.candidates == null || wrapper.candidates.Length == 0)
            {
                Debug.LogWarning("[Gemini] 빈 응답");
                return new GeminiJudgment { pass = true, npcResponse = "" };
            }

            string innerJson = wrapper.candidates[0].content.parts[0].text;
            var judgment = JsonUtility.FromJson<GeminiJudgment>(innerJson);

            Debug.Log($"[Gemini] 판정: pass={judgment.pass}, npc=\"{judgment.npcResponse}\"");
            return judgment;
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }
    }

    // ============================================================
    //  DTO
    // ============================================================

    [Serializable]
    public class GeminiJudgment
    {
        public bool pass;

        [SerializeField] private string npc_response;

        public string npcResponse
        {
            get => npc_response;
            set => npc_response = value;
        }
    }

    [Serializable]
    public class GeminiResponseWrapper
    {
        public GeminiCandidate[] candidates;
    }

    [Serializable]
    public class GeminiCandidate
    {
        public GeminiContent content;
    }

    [Serializable]
    public class GeminiContent
    {
        public GeminiPart[] parts;
    }

    [Serializable]
    public class GeminiPart
    {
        public string text;
    }
}
