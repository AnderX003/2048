using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class UnitsDrawer : MonoBehaviour
    {
        [SerializeField] private Vector3 hidePosition;
        [SerializeField] private Ease moveEase;
        [SerializeField] private Ease appearanceEase;
        [SerializeField] private Ease punchEase;
        [SerializeField] private float moveDuration;
        [SerializeField] private float appearDuration;
        [SerializeField] private float appearDelay;
        [SerializeField] private float colorChangingDuration;
        [SerializeField] private float punchDuration;
        [SerializeField] private float punchRatio;
        [SerializeField] private float themeChangingDuration;
        public float MoveDuration => moveDuration;
        public float AppearDelay => appearDelay;

        #region Unit drawing

        public void SetNewUnitPosNScale(Unit unit)
        {
            unit.transform.localScale = Vector3.zero;
            unit.UnitRectTransform.localPosition = new Vector3(
                Data.CurrentLayout[unit.Position.x, unit.Position.y, 0],
                Data.CurrentLayout[unit.Position.x, unit.Position.y, 1]);
        }

        public void DrawNewUnit(Unit unit, bool setNewUnitPosNScale = true)
        {
            //sizes
            if (setNewUnitPosNScale)
            {
                unit.transform.localScale = Vector3.zero;
                //position
                unit.UnitRectTransform.localPosition = new Vector3(
                    Data.CurrentLayout[unit.Position.x, unit.Position.y, 0],
                    Data.CurrentLayout[unit.Position.x, unit.Position.y, 1]);
            }
            
            float scale = Data.CellSizes[Data.MaxValue + 1];
            unit.UnitRectTransform.sizeDelta = new Vector3(scale, scale, 1);
            unit.UnitText.fontSize = Data.CellTextSizes[Data.MaxValue + 1];

            //color and text
            unit.UnitText.text = unit.Value.ToString();
            unit.UnitImage.color = Data.GetColorByValue(unit.Value);
            if (Data.CurrentTheme == 2 && (unit.Value == 2 || unit.Value == 4))
                unit.UnitText.color = new Color32(119, 110, 101, 255);
            else unit.UnitText.color = Color.white;

            //animation
            //DOTween.To(()=> unit.transform.localScale, x=> unit.transform.localScale = x, Vector3.one, appearDuration).SetEase(appearanceEase); 
            unit.AnimationQueue.AddAnimation(
                () => DOTween.To(
                        () => unit.transform.localScale,
                        x => unit.transform.localScale = x,
                        Vector3.one, appearDuration)
                    .SetEase(appearanceEase)
                    .OnComplete(() => unit.AnimationQueue.Callback()));
        }

        public void UpdateUnitPosition(Unit unit, Action onComplete = null)
        {
            DOTween.To(
                () => unit.transform.localPosition,
                x => unit.transform.localPosition = x,
                new Vector3(
                    Data.CurrentLayout[unit.Position.x, unit.Position.y, 0],
                    Data.CurrentLayout[unit.Position.x, unit.Position.y, 1]),
                moveDuration).SetEase(moveEase).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void UpdateUnitColorAndText(Unit unit)
        {
            unit.UnitText.text = unit.Value.ToString();
            if (Data.CurrentTheme == 2 && unit.Value == 8) unit.UnitText.color = new Color32(255, 255, 255, 255);
            DOTween.To(
                () => unit.UnitImage.color, 
                x => unit.UnitImage.color = x, 
                Data.GetColorByValue(unit.Value), 
                colorChangingDuration);
        }

        public void IncrementUnit(Unit unit)
        {
            unit.transform.SetAsLastSibling();

            /*unit.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 0, 0).SetEase(punchEase);*/
            unit.AnimationQueue.AddAnimation(
                () => unit.transform
                    .DOPunchScale(Vector3.one * punchRatio, punchDuration, 0, 0)
                    .SetEase(punchEase)
                    .OnComplete(() => unit.AnimationQueue.Callback()));

            UpdateUnitColorAndText(unit);
        }

        public void HideUnit(Unit unit)
        {
            unit.transform.position = hidePosition;
        }

        #endregion

        #region Changing Images by size

        [SerializeField] private Image gridImage;
        [SerializeField] private Sprite[] gridSprites;

        public void SetMapImagesBySize(int side)
        {
            gridImage.sprite = gridSprites[side - 3];
        }

        #endregion

        #region Change Theme

        [SerializeField] private Material[] Materials;

        public void ChangeTheme()
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].DOColor(Data.UIColors[Data.CurrentTheme, i], themeChangingDuration);
            }
        }
        
        public void SetTheme()
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].color = Data.UIColors[Data.CurrentTheme, i];
            }
        }

        #endregion

    }
}
