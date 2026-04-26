// TrainingUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using Artti.Training;

namespace Artti.UI
{
    /// <summary>
    /// 훈련모드 화면 표시 및 사용자 입력 수집.
    /// MVP 단계: 모든 요소를 Unity UI (Canvas)로 구성.
    /// </summary>
    public class TrainingUIController : MonoBehaviour
    {
        [Header("상황 표시")]
        [SerializeField] private TMP_Text situationText;    // NPC 상황 설명

        [Header("카드 버튼 (최대 2개)")]
        [SerializeField] private Button cardButton0;
        [SerializeField] private TMP_Text cardText0;
        [SerializeField] private Button cardButton1;
        [SerializeField] private TMP_Text cardText1;

        [Header("발화 입력 (STT Mock)")]
        [SerializeField] private GameObject speechInputPanel;
        [SerializeField] private TMP_InputField speechInputField;
        [SerializeField] private Button speechSubmitButton;

        [Header("NPC 응답")]
        [SerializeField] private TMP_Text npcResponseText;

        [Header("완료 화면")]
        [SerializeField] private GameObject completionPanel;

        [Header("재시도 안내")]
        [SerializeField] private TMP_Text retryPromptText;

        // NPC 응답 화면의 "다음" 버튼
        [Header("다음 버튼")]
        [SerializeField] private Button nextButton;

        // -- 카드 선택 결과를 전달하기 위한 내부 상태 --
        private CardInfo[] currentCards;
        private int selectedCardIndex = -1;

        // -- 발화 입력 결과 --
        private string submittedSpeech;
        private bool speechSubmitted;

        // ============================================================
        //  Public API (TrainingManager가 호출)
        // ============================================================

        public void ShowStep(StepData step)
        {
            // 초기화
            npcResponseText.text = "";
            retryPromptText.text = "";
            speechInputPanel.SetActive(false);
            completionPanel.SetActive(false);
            selectedCardIndex = -1;

            // 다음버튼은 숨기기 (NPC 응답 후에만 활성화)
            nextButton.gameObject.SetActive(false);

            // 상황 텍스트
            situationText.text = step.situation;

            // 카드 표시
            currentCards = step.cards;
            SetupCardButton(cardButton0, cardText0, step.cards, 0);
            SetupCardButton(cardButton1, cardText1, step.cards, 1);
        }

        public async UniTask<CardInfo> WaitForCardSelection(CancellationToken ct)
        {
            selectedCardIndex = -1;

            // 사용자가 카드 버튼을 누를 때까지 대기
            await UniTask.WaitUntil(() => selectedCardIndex >= 0, cancellationToken: ct);

            return currentCards[selectedCardIndex];
        }

        public async UniTask<string> WaitForSpeechInput(CancellationToken ct)
        {
            speechSubmitted = false;
            submittedSpeech = "";
            speechInputField.text = "";
            speechInputPanel.SetActive(true);

            // 사용자가 제출 버튼을 누를 때까지 대기
            await UniTask.WaitUntil(() => speechSubmitted, cancellationToken: ct);

            speechInputPanel.SetActive(false);
            return submittedSpeech;
        }

        // NPC 응답 후 사용자가 "다음" 버튼을 누를 때까지 대기
        public async UniTask WaitForNextButton(CancellationToken ct)
        {
            nextPressed = false;
            nextButton.gameObject.SetActive(true);

            await UniTask.WaitUntil(() => nextPressed, cancellationToken: ct);

            nextButton.gameObject.SetActive(false);
        }

        // 내부 상태 + 핸들러 (다른 필드들과 같은 영역에 추가)
        private bool nextPressed;

        private void OnNextButtonClicked()
        {
            nextPressed = true;
        }

        public void ShowNpcResponse(string response)
        {
            npcResponseText.text = $"[직원] {response}";
        }

        public void ShowRetryPrompt()
        {
            retryPromptText.text = "다시 한번 말해볼까요?";
        }

        public void ShowCompletionScreen()
        {
            completionPanel.SetActive(true);
            situationText.text = "";
            cardButton0.gameObject.SetActive(false);
            cardButton1.gameObject.SetActive(false);
        }

        // ============================================================
        //  내부 구현
        // ============================================================

        private void Awake()
        {
            cardButton0.onClick.AddListener(() => OnCardClicked(0));
            cardButton1.onClick.AddListener(() => OnCardClicked(1));
            speechSubmitButton.onClick.AddListener(OnSpeechSubmit);
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        private void SetupCardButton(
            Button button, TMP_Text label, CardInfo[] cards, int index)
        {
            if (index < cards.Length)
            {
                button.gameObject.SetActive(true);
                label.text = cards[index].displayText;
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }

        private void OnCardClicked(int index)
        {
            selectedCardIndex = index;
            Debug.Log($"[UI] 카드 {index} 선택: {currentCards[index].displayText}");
        }

        private void OnSpeechSubmit()
        {
            submittedSpeech = speechInputField.text;
            speechSubmitted = true;
        }
    }
}