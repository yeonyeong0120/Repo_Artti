using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Artti.Core;

namespace Artti.UI
{
    public class ResultConfirmUI : MonoBehaviour
    {
        [Header("캡처 이미지")]
        [SerializeField] private RawImage capturedImage;

        [Header("결과 카드")]
        [SerializeField] private Image aacIcon;
        [SerializeField] private TMP_Text categoryLabel;
        [SerializeField] private TMP_Text descriptionText;

        [Header("재시도")]
        [SerializeField] private Button retryButton;

        public event Action OnConfirmed;
        public event Action OnRetried;

        private void Awake()
        {
            retryButton.onClick.AddListener(() => OnRetried?.Invoke());
        }

        public void Setup(Texture2D captured, AACCardData matchedCard)
        {
            capturedImage.texture = captured;
            aacIcon.sprite = matchedCard.icon;
            categoryLabel.text = matchedCard.category.ToString();
            descriptionText.text = matchedCard.displayText;
        }
    }
}
