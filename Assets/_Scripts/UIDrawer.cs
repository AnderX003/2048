using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts
{
    public class UIDrawer : MonoBehaviour
    {
        public GridManager Grid;

        #region Unit drawing

        
        public void DrawNewUnit(Unit unit)
        {
            float scale = Data.CellSizes[Data.MaxValue + 1];
            unit.UnitRectTransform.localPosition = new Vector2(Data.CurrentLayout[unit.PositionX, unit.PositionY, 0],
                Data.CurrentLayout[unit.PositionX, unit.PositionY, 1]);
            unit.UnitRectTransform.sizeDelta = new Vector3(0, 0, 1);
            unit.UnitRectTransform.DOSizeDelta(new Vector3(scale, scale, 1), 0.5f).SetEase(Ease.OutQuint);
            unit.UnitText.fontSize = Data.CellTextSizes[Data.MaxValue + 1];
            unit.TextObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuint);

            UpdateUnit(unit);
        }

        public void UpdateUnit(Unit unit, bool beforeDeath = false, bool createNewUnit = false)
        {
            unit.transform.DOLocalMove(new Vector3(Data.CurrentLayout[unit.PositionX, unit.PositionY, 0],
                Data.CurrentLayout[unit.PositionX, unit.PositionY, 1], 1), 0.1f).SetEase(Ease.InSine).OnComplete(() =>
            {
                //Manager.CellsMovesIsEnd = true;
                if (beforeDeath) Destroy(unit.gameObject);
                if (createNewUnit)
                {
                    //Grid.CanUndo = true;
                    Grid.CreateNewUnit();
                    
                }
            });
            unit.UnitText.text = unit.Value.ToString();
            unit.UnitImage.DOColor(Data.GetColorByValue(unit.Value), 1 / 3f);
        }
        
        public void IncrementUnit(Unit unit)
        {       
            unit.transform.SetAsLastSibling();
            unit.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 0, 0).SetEase(Ease.InOutExpo);
            //MainMap.DeleteCells.Add(dying);
            UpdateUnit(unit);
            //StartCoroutine(WaitForCellToBeReadyToDeath(dying));
        }
        
        #endregion
        
        #region Changing Images by size

        public Image ForegroundImage;
        public RectTransform BackgroundImageTransform;

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
            Foreground8;/*,
            CellImage3,
            CellImage4,
            CellImage5,
            CellImage6,
            CellImage7,
            CellImage8;*/

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

            //ForegroundImage.sprite = Foregroungs[side];
            switch (side)
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
            }
        }

        #endregion
        
        #region Change Theme

        public Material MaterialBackground, MaterialForeground, MaterialText, MaterialUI, MaterialCellText;

        public void ChangeTheme()
        {
            if (Grid.Units != null)
                foreach (Unit unit in Grid.Units)
                    if (unit != null)
                        unit.UnitImage.color = Data.GetColorByValue(unit.Value);

            MaterialBackground.DOColor(Data.UIColors[Data.CurrentTheme, 0], 1 / 3f);
            MaterialForeground.DOColor(Data.UIColors[Data.CurrentTheme, 1], 1 / 3f);
            MaterialText.DOColor(Data.UIColors[Data.CurrentTheme, 2], 1 / 3f);
            MaterialUI.DOColor(Data.UIColors[Data.CurrentTheme, 3], 1 / 3f);
            MaterialCellText.DOColor(Data.UIColors[Data.CurrentTheme, 4], 1 / 3f);
        }

        #endregion

    }
}
