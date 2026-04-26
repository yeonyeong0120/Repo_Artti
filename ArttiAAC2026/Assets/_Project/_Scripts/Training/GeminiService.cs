// GeminiService.cs
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace Artti.Training
{
    /// <summary>
    /// Gemini API를 호출하여 발화 적절성을 판단하고 NPC 응답을 받는다.
    /// </summary>
    public class GeminiService : MonoBehaviour
    {
        [Header("API 설정")]
        [SerializeField] private string modelName = "gemini-2.5-flash-lite";
        private string apiKey;

        private string systemPrompt;
        private string cachedEndpointUrl;

        /// <summary>
        /// 시나리오 진입 시 호출. 시스템 프롬프트를 설정한다.
        /// 기획서 5.2절: "시스템 프롬프트는 시나리오 진입 시 한 번만 설정"
        /// </summary>
        public void Initialize(string scenarioSystemPrompt)
        {
            systemPrompt = scenarioSystemPrompt;
            apiKey = LoadApiKey();
            cachedEndpointUrl =
                $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("[Gemini] API 키를 찾을 수 없음. 4AAC_gemini_api_key.txt 파일을 확인하세요.");
                return;
            }

            Debug.Log("[Gemini] 초기화 완료 (API 키 로드됨)");
        }

        private string LoadApiKey()
        {
            // Resources.Load는 Editor와 빌드 모두에서 동작.
            // Resources/gemini_key.txt 파일을 읽는다.
            var keyAsset = Resources.Load<TextAsset>("gemini_key");

            if (keyAsset == null)
            {
                Debug.LogError(
                    "[Gemini] Resources/gemini_key.txt 파일 없음. " +
                    "Assets/_Project/Data/Resources/gemini_key.txt를 생성하세요.");
                return null;
            }

            string key = keyAsset.text.Trim();
            Debug.Log("[Gemini] API 키 Resources에서 로드 성공");
            return key;
        }

        /// <summary>
        /// 사용자 발화가 현재 Step에 적절한지 판단을 요청한다.
        /// </summary>
        public async UniTask<GeminiJudgment> JudgeSpeech(
            string stepId, string expectedIntent, string userSpeech,
            CancellationToken ct)
        {
            // 기획서 5.2절: "매 Step 호출 시에는 현재 Step 번호,
            // 이 Step에서 기대하는 발화 유형, 실제 사용자 발화 텍스트만 짧게 전달"
            string userMessage =
                $"[Step {stepId}]\n기대 발화 유형: {expectedIntent}\n실제 발화: \"{userSpeech}\"";

            string requestBody = BuildRequestJson(userMessage);

            try
            {
                string responseJson = await PostRequest(requestBody, ct);
                return ParseResponse(responseJson);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Gemini] API 호출 실패, 기본 응답 반환: {e.Message}");
                // API 실패 시 pass=true로 강제 진행 (사용자 좌절 방지)
                return new GeminiJudgment { pass = true, npcResponse = "(응답을 불러올 수 없습니다)" };
            }
        }

        // ============================================================
        //  내부 구현
        // ============================================================

        private string BuildRequestJson(string userMessage)
        {
            // JsonUtility는 중첩 배열 직렬화가 번거로우므로 string 조합으로 처리.
            // 프로덕션에서는 전용 DTO 클래스를 만드는 것이 바람직하다.
            string escaped_system = EscapeJson(systemPrompt);
            string escaped_user = EscapeJson(userMessage);

            return $@"{{
                      ""system_instruction"": {{
                        ""parts"": [{{ ""text"": ""{escaped_system}"" }}]
                      }},
                      ""contents"": [
                        {{
                          ""role"": ""user"",
                          ""parts"": [{{ ""text"": ""{escaped_user}"" }}]
                        }}
                      ],
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
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest().WithCancellation(ct);

            // === 진단용 로그 추가 ===
            Debug.Log($"[Gemini RAW] HTTP {request.responseCode}\n{request.downloadHandler.text}");
            // === 진단용 로그 끝 ===

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"HTTP {request.responseCode}: {request.error}");
            }

            return request.downloadHandler.text;
        }

        private GeminiJudgment ParseResponse(string responseJson)
        {
            // Gemini API 응답 구조:
            // { "candidates": [{ "content": { "parts": [{ "text": "..." }] } }] }
            // parts[0].text 안에 우리가 요청한 JSON이 들어있다.

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
    //  응답 파싱용 DTO
    // ============================================================

    /// <summary>
    /// LLM이 반환하는 판정 결과.
    /// 기획서 5.2절: { "pass": true/false, "npc_response": "..." }
    /// </summary>
    [Serializable]
    public class GeminiJudgment
    {
        public bool pass;

        // Gemini JSON 응답의 키가 snake_case이므로 SerializeField로 매핑
        [SerializeField] private string npc_response;

        public string npcResponse
        {
            get => npc_response;
            set => npc_response = value;
        }
    }

    // -- Gemini REST API 응답 구조 매핑 --
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