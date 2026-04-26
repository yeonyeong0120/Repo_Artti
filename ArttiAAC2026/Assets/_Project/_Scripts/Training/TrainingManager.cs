// TrainingManager.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Artti.UI;

namespace Artti.Training
{
    /// <summary>
    /// 훈련모드의 Step 진행을 관리하는 상태 머신.
    /// Scene에 빈 GameObject를 만들고 이 컴포넌트를 부착한다.
    /// </summary>
    public class TrainingManager : MonoBehaviour
    {
        [Header("시나리오 설정")]
        [SerializeField] private string scenarioFileName = "scenario_convenience";

        private ScenarioData scenario;
        private int currentStepIndex;
        private CancellationTokenSource cts;

        // -- 외부 모듈 참조 (Inspector에서 연결) --
        [Header("UI 참조")]
        [SerializeField] private TrainingUIController uiController;

        [Header("LLM 서비스")]
        [SerializeField] private GeminiService geminiService;

        // -- 이벤트: 외부에서 구독 가능 --
        public event Action<StepData> OnStepStarted;
        public event Action OnScenarioCompleted;

        private void Awake()
        {
            cts = new CancellationTokenSource();
        }

        private void Start()
        {
            scenario = ScenarioLoader.Load(scenarioFileName);
            if (scenario == null) return;

            currentStepIndex = 0;
            StartStep(currentStepIndex).Forget();
        }

        private void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        // ============================================================
        //  Step 진행 흐름
        // ============================================================

        private async UniTaskVoid StartStep(int index)
        {
            if (index >= scenario.steps.Length)
            {
                CompleteScenario();
                return;
            }

            var step = scenario.steps[index];
            Debug.Log($"[Training] Step {step.stepId} 시작: {step.situation}");

            // UI에 상황 텍스트 + 카드 표시
            uiController.ShowStep(step);
            OnStepStarted?.Invoke(step);

            // 사용자의 카드 선택을 대기
            var selectedCard = await uiController.WaitForCardSelection(cts.Token);
            Debug.Log($"[Training] 카드 선택됨: {selectedCard.displayText}");

            if (step.requiresSpeech)
            {
                await HandleSpeechStep(step, selectedCard, cts.Token);
            }
            else
            {
                // Rule-based: 카드 터치만으로 진행. LLM 호출 없음.
                uiController.ShowNpcResponse(step.npcDefaultLine);
                await UniTask.Delay(1500, cancellationToken: cts.Token);
            }

            // 다음 Step으로
            currentStepIndex++;
            StartStep(currentStepIndex).Forget();
        }

        private async UniTask HandleSpeechStep(
            StepData step, CardInfo selectedCard, CancellationToken ct)
        {
            // -- 1차 시도 --
            string userSpeech = await GetUserSpeech(selectedCard, ct);
            var result = await geminiService.JudgeSpeech(
                step.stepId, step.expectedIntent, userSpeech, ct);

            if (result.pass)
            {
                uiController.ShowNpcResponse(result.npcResponse);
                await UniTask.Delay(1500, cancellationToken: ct);
                return;
            }

            // -- 1회 재시도 --
            Debug.Log("[Training] 1차 발화 부적절 -> 재시도 기회 제공");
            uiController.ShowRetryPrompt();

            string retrySpeech = await GetUserSpeech(selectedCard, ct);
            var retryResult = await geminiService.JudgeSpeech(
                step.stepId, step.expectedIntent, retrySpeech, ct);

            if (retryResult.pass)
            {
                uiController.ShowNpcResponse(retryResult.npcResponse);
            }
            else
            {
                // 재시도 실패 -> NPC가 유도성 응답 후 강제 진행
                Debug.Log("[Training] 재시도 실패 -> 강제 진행");
                uiController.ShowNpcResponse(step.npcDefaultLine);
            }

            await UniTask.Delay(1500, cancellationToken: ct);
        }

        /// <summary>
        /// 사용자 발화를 수집한다.
        /// MVP 단계에서는 STT 대신 텍스트 입력 필드로 Mock.
        /// 추후 STT 모듈로 교체한다.
        /// </summary>
        private async UniTask<string> GetUserSpeech(
            CardInfo selectedCard, CancellationToken ct)
        {
            // MVP Mock: 사용자가 텍스트 입력 필드에 직접 타이핑하거나,
            // 카드의 ttsText를 그대로 반환하는 방식.
            // 중간발표에서는 후자가 빠르다.
            string speech = await uiController.WaitForSpeechInput(ct);

            // 입력이 비어있으면 카드 텍스트를 기본값으로 사용
            if (string.IsNullOrWhiteSpace(speech))
            {
                speech = selectedCard.ttsText;
            }

            Debug.Log($"[Training] 사용자 발화: {speech}");
            return speech;
        }

        private void CompleteScenario()
        {
            Debug.Log("[Training] 시나리오 완료!");
            uiController.ShowCompletionScreen();
            OnScenarioCompleted?.Invoke();
        }
    }
}