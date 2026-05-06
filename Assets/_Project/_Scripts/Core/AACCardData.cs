// AACCardData.cs
using UnityEngine;

namespace Artti.Core
{
    /// <summary>
    /// AAC 카드 한 장의 데이터.
    /// ScriptableObject로 만들어 Unity Inspector에서 편집 가능.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAACCard", menuName = "Artti/AAC Card")]
    public class AACCardData : ScriptableObject
    {
        [Header("식별")]
        [Tooltip("카드 고유 ID (예: conv_pay_card)")]
        public string cardId;

        [Header("표시")]
        [Tooltip("카드에 표시할 AAC 아이콘")]
        public Sprite icon;

        [Tooltip("카드에 표시할 텍스트 (짧고 명료하게)")]
        public string displayText;

        [Header("분류")]
        public PlaceCategory category;
        public string subcategory; // 주문하기, 계산하기 등

        [Header("음성")]
        [Tooltip("TTS로 출력할 문장 (displayText와 동일하거나 더 상세한 문장)")]
        public string ttsText;
    }

    public enum PlaceCategory
    {
        Convenience, // 편의점
        Pharmacy,    // 약국
        Restaurant   // 음식점
    }
}