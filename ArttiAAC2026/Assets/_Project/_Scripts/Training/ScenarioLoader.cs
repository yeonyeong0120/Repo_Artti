// ScenarioLoader.cs
using UnityEngine;

namespace Artti.Training
{
    /// <summary>
    /// Resources 또는 StreamingAssets에서 시나리오 JSON을 읽어
    /// ScenarioData 객체로 변환한다.
    /// </summary>
    public static class ScenarioLoader
    {
        /// <summary>
        /// Data/Scenarios 폴더의 JSON 파일을 로드한다.
        /// Resources.Load를 사용하므로 파일은 Assets/_Project/Data/Scenarios/ 아래에
        /// 배치하되, 경로 인자에는 확장자를 붙이지 않는다.
        /// </summary>
        /// <param name="scenarioFileName">
        /// 파일명 (확장자 제외). 예: "scenario_convenience"
        /// </param>
        public static ScenarioData Load(string scenarioFileName)
        {
            string path = $"Scenarios/{scenarioFileName}";
            var textAsset = Resources.Load<TextAsset>(path);

            if (textAsset == null)
            {
                Debug.LogError($"[ScenarioLoader] JSON 파일을 찾을 수 없음: {path}");
                return null;
            }

            var data = JsonUtility.FromJson<ScenarioData>(textAsset.text);
            Debug.Log($"[ScenarioLoader] 로드 완료: {data.scenarioName} ({data.steps.Length} Steps)");
            return data;
        }
    }
}