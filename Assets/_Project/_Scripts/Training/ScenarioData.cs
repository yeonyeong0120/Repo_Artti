// ScenarioData.cs
using System;

namespace Artti.Training
{
    /// <summary>
    /// 하나의 시나리오(편의점/약국/음식점) 전체를 표현하는 데이터 구조.
    /// JsonUtility로 파싱 가능하도록 [Serializable] 적용.
    /// </summary>
    [Serializable]
    public class ScenarioData
    {
        public string scenarioId;   // "convenience"
        public string scenarioName; // "편의점"
        public string systemPrompt; // Gemini 시스템 프롬프트
        public StepData[] steps;
    }

    [Serializable]
    public class StepData
    {
        public int stepIndex;
        public string stepId;           // "1", "2A", "2B" 등
        public string situation;        // 이 Step의 상황 설명 (NPC 대사 등)
        public string expectedIntent;   // LLM에 전달할 기대 발화 유형
        public bool requiresSpeech;     // true: LLM 판정 필요 / false: 카드 터치만으로 진행
        public CardInfo[] cards;        // 이 Step에서 표시할 카드 (최대 2개)
        public string npcDefaultLine;   // LLM 실패 시 또는 Rule-based Step의 NPC 기본 대사
    }

    [Serializable]
    public class CardInfo
    {
        public string cardId;
        public string displayText;
        public string ttsText;
        public bool isCorrect; // 훈련모드에서 정답 카드 여부
    }
}