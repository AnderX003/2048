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

        public Image ForegroundImage;
        public RectTransform BackgroundImageTransform;
        public RectTransform BackgroundGameOverImageTransform;

        public Sprite
            Background3,
            Background4,
            Background5,
            Background6,
            Background7,
            Background8,
            Foreground3,
            Foreground4,
            Foreground5,
            Foreground6,
            Foreground7,
            Foreground8;

        public void SetMapImagesBySize(int side)
        {
            //BackgroundImage.sprite = Backgroungs[side];
            switch (side)
            {
                case 3:
                    ForegroundImage.sprite = Foreground3;
                    //BackgroundImage.sprite = Background3;
                    BackgroundImageTransform.sizeDelta = new Vector2(1013.7063f, 1013.7063f);
                    break;
                case 4:
                    ForegroundImage.sprite = Foreground4;
                    BackgroundImageTransform.sizeDelta = new Vector2(1000, 1000);
                    //BackgroundImage.sprite = Background4;
                    break;
                case 5:
                    ForegroundImage.sprite = Foreground5;
                    BackgroundImageTransform.sizeDelta = new Vector2(991.8672f, 991.8672f);
                    //BackgroundImage.sprite = Background5;
                    break;
                case 6:
                    ForegroundImage.sprite = Foreground6;
                    BackgroundImageTransform.sizeDelta = new Vector2(986.4828f, 986.4828f);
                    //BackgroundImage.sprite = Background6;
                    break;
                case 7:
                    ForegroundImage.sprite = Foreground7;
                    BackgroundImageTransform.sizeDelta = new Vector2(982.6549f, 982.6549f);
                    //BackgroundImage.sprite = Background7;
                    break;
                case 8:
                    ForegroundImage.sprite = Foreground8;
                    BackgroundImageTransform.sizeDelta = new Vector2(979.7938f, 979.7938f);
                    //BackgroundImage.sprite = Background8;
                    break;
            }

            BackgroundGameOverImageTransform.sizeDelta = BackgroundImageTransform.sizeDelta;

            //ForegroundImage.sprite = Foregroungs[side];
            /*switch (side)
            {
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
            }*/
        }

        #endregion
        
        #region Change Theme

        [SerializeField] private List<Material> Materials;

        public void ChangeTheme()
        {
            if (Grid.Units != null)
                foreach (Unit unit in Grid.Units)
                    if (!unit.IsEmpty)
                        unit.UnitImage.color = Data.GetColorByValue(unit.Value);

            for (int i = 0; i < Materials.Count; i++)
            {
                Materials[i].DOColor(Data.UIColors[Data.CurrentTheme, i], 1 / 3f);
            }
        }

        #endregion

    }
}
