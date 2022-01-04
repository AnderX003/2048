using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class UIDrawer : MonoBehaviour
    {
        [SerializeField] private GridManager Grid;
        [SerializeField] private Vector3 hidePosition;
        [SerializeField] private Ease moveEase;
        [SerializeField] private Ease appearanceEase;
        [SerializeField] private Ease punchEase;
        [SerializeField] private float moveDuration;
        public float MoveDuration => moveDuration;
        #region Unit drawing

        
        public void DrawNewUnit(Unit unit)
        {
            //position
            unit.UnitRectTransform.localPosition = new Vector3(
                Data.CurrentLayout[unit.PositionX, unit.PositionY, 0],
                Data.CurrentLayout[unit.PositionX, unit.PositionY, 1]);
            
            //sizes
            //unit.UnitRectTransform.rect = 
            unit.UnitRectTransform.sizeDelta = new Vector3(0, 0, 1);
            float scale = Data.CellSizes[Data.MaxValue + 1];
            unit.UnitRectTransform.DOSizeDelta(new Vector3(scale, scale, 1), 0.5f).SetEase(appearanceEase);
            unit.UnitText.fontSize = Data.CellTextSizes[Data.MaxValue + 1];
            unit.TextObject.transform.localScale = Vector3.zero;
            unit.TextObject.transform.DOScale(Vector3.one, 0.5f).SetEase(appearanceEase/*Ease.OutQuint*/);
            
            //color and text
            unit.UnitText.text = unit.Value.ToString();
            unit.UnitImage.color = Data.GetColorByValue(unit.Value);
            if (Data.CurrentTheme == 2 && (unit.Value == 2 || unit.Value == 4))
                unit.UnitText.color = new Color32(119, 110, 101, 255);
            else unit.UnitText.color = Color.white;
        }

        public void UpdateUnit(Unit unit, bool beforeDeath = false, Queue<Unit> pool = null)
        {
            unit.transform.DOLocalMove(new Vector3(Data.CurrentLayout[unit.PositionX, unit.PositionY, 0],
                Data.CurrentLayout[unit.PositionX, unit.PositionY, 1]), moveDuration).SetEase(moveEase/*Ease.InSine*/).OnComplete(() =>
            {
                if (!beforeDeath) return;
                pool?.Enqueue(unit);
                HideUnit(unit);
            });
            unit.UnitText.text = unit.Value.ToString();
            if (Data.CurrentTheme == 2 && unit.Value == 8) unit.UnitText.color = new Color32(255, 255, 255, 255);
            unit.UnitImage.DOColor(Data.GetColorByValue(unit.Value), 1 / 3f);
        }

        public void IncrementUnit(Unit unit)
        {       
            unit.transform.SetAsLastSibling();
            unit.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 0, 0).SetEase(punchEase/*Ease.InOutExpo*/);
            UpdateUnit(unit);
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
            gridImage.sprite = gridSprites[side-3];
        }

        #endregion
        
        #region Change Theme

        [SerializeField] private Material[] Materials;

        public void ChangeTheme()
        {
            /*if (Grid.Units != null)
                foreach (Unit unit in Grid.Units)
                    if (!unit.IsEmpty)
                        unit.UnitImage.color = Data.GetColorByValue(unit.Value);*/

            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].DOColor(Data.UIColors[Data.CurrentTheme, i], 1 / 3f);
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
