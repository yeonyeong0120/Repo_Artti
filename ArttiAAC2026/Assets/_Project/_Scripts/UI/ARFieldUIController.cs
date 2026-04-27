using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Artti.Core;

namespace Artti.UI
{
    public class ARFieldUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject cameraPanel;
        [SerializeField] private GameObject resultConfirmPanel;
        [SerializeField] private GameObject manualSelectPanel;
        [SerializeField] private GameObject categoryPanel;
        [SerializeField] private GameObject subCategoryPanel;

        [Header("Manual Select Buttons")]
        [SerializeField] private Button convenienceBtn;
        [SerializeField] private Button pharmacyBtn;
        [SerializeField] private Button restaurantBtn;

        [Header("Category Card Slots (4개)")]
        [SerializeField] private AACCardUI[] categorySlots;

        [Header("SubCategory Card Slots (4개)")]
        [SerializeField] private AACCardUI[] subCategorySlots;

        public event Action<PlaceCategory> OnManualPlaceSelected;
        public event Action<AACCardData> OnCategorySelected;
        public event Action<AACCardData> OnSubCategorySelected;

        private void Awake()
        {
            convenienceBtn.onClick.AddListener(() => OnManualPlaceSelected?.Invoke(PlaceCategory.Convenience));
            pharmacyBtn.onClick.AddListener(() => OnManualPlaceSelected?.Invoke(PlaceCategory.Pharmacy));
            restaurantBtn.onClick.AddListener(() => OnManualPlaceSelected?.Invoke(PlaceCategory.Restaurant));
        }

        public void ShowCamera() => SetActiveOnly(cameraPanel);
        public void ShowResultConfirm() => SetActiveOnly(resultConfirmPanel);
        public void ShowManualSelect() => SetActiveOnly(manualSelectPanel);

        public void ShowCategory(IReadOnlyList<AACCardData> cards)
        {
            SetActiveOnly(categoryPanel);
            RefreshSlots(categorySlots, cards, OnCategorySelected);
        }

        public void ShowSubCategory(IReadOnlyList<AACCardData> cards)
        {
            SetActiveOnly(subCategoryPanel);
            RefreshSlots(subCategorySlots, cards, OnSubCategorySelected);
        }

        private void SetActiveOnly(GameObject target)
        {
            cameraPanel.SetActive(cameraPanel == target);
            resultConfirmPanel.SetActive(resultConfirmPanel == target);
            manualSelectPanel.SetActive(manualSelectPanel == target);
            categoryPanel.SetActive(categoryPanel == target);
            subCategoryPanel.SetActive(subCategoryPanel == target);
        }

        private void RefreshSlots(AACCardUI[] slots, IReadOnlyList<AACCardData> cards, Action<AACCardData> callback)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                bool active = i < cards.Count;
                slots[i].gameObject.SetActive(active);
                if (active)
                    slots[i].Setup(cards[i], callback);
            }
        }
    }
}
