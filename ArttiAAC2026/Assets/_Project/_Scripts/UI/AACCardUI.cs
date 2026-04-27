using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Artti.Core;

namespace Artti.UI
{
    public class AACCardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Button button;

        private AACCardData data;
        private Action<AACCardData> onClicked;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Setup(AACCardData cardData, Action<AACCardData> callback)
        {
            data = cardData;
            onClicked = callback;
            iconImage.sprite = cardData.icon;
            labelText.text = cardData.displayText;
        }

        private void OnButtonClicked()
        {
            onClicked?.Invoke(data);
        }
    }
}
