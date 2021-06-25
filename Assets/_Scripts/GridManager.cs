using System.Threading.Tasks;
using UnityEngine;

namespace _Scripts
{
    public class GridManager : MonoBehaviour
    {
        public GameObject MapGameObject;
        public GameManager GameManager;
        public UIDrawer Drawer;
        public Unit[,] Units { get; set; }
        private int Rows;
        private int Columns;
        private int SideLength;
        private int UnitCounter;
        public bool isActive { get; set; }

        #region Start Game and Creating new units
        
        
        public void InitializeGrid(int sideLength)
        {
            GameManager.GameOver = false;
            Data.Score = 0;
            GameManager.UpdateScoreText();
            SideLength = sideLength;
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            isActive = true;
            Rows = Columns = sideLength;
            Units = new Unit[Rows, Columns];
            UnitCounter = 0;
            Drawer.SetMapImagesBySize(SideLength);
            CreateNewUnit(canCreateSeveralUnits: true);
            CreateNewUnit(canCreateSeveralUnits: true);
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];
        }

        public GameObject UnitPrefab;

        public void CreateNewUnit(int posX = -1, int posY = -1, int value = -1, bool canCreateSeveralUnits = false)
        {
            if (UnitCounter == Rows * Columns) return;
            if (!canCreateSeveralUnits)
            {
                if (!CanCreateNewUnit) return;
                CanCreateNewUnit = false;
            }

            int positionX;
            int positionY;
            if (posX == -1 || posY == -1)
            {
                do
                {
                    int x = Random.Range(0, Data.MaxValue + 1);
                    int y = Random.Range(0, Data.MaxValue + 1);
                    positionX = Data.layout_simple[x, y, 0];
                    positionY = Data.layout_simple[x, y, 1];
                } while (Units[positionX, positionY] != null);
            }
            else
            {
                positionX = posX;
                positionY = posY;
            }

            GameObject obj = Instantiate(UnitPrefab, MapGameObject.transform);

            Unit unit = obj.gameObject.GetComponent<Unit>();
            Units[positionX, positionY] = unit;
            unit.PositionX = positionX;
            unit.PositionY = positionY;
            int[] arr = {2, 2, 2, 4};
            unit.Value = value == -1 ? arr[Random.Range(0, 4)] : value;
            UnitCounter++;
            Drawer.DrawNewUnit(unit);/*
            Data.Score += unit.Value;
            GameManager.UpdateScoreText();*/
        }

        #endregion
        

        #region Game Over Logic
        

        private async void CheckTheEnd()
        {
            await Task.Run(() =>
            {
                bool[,] checkedList = new bool[Rows, Columns];
                if (!CheckTheEnd(Units[1, 1], checkedList))
                {
                    //Debug.Log("Game Over");
                    GameManager.GameOver = true;
                    isActive = false;
                }
            });
        }

        private bool CheckTheEnd(Unit startUnit, bool[,] checkedUnits)
        {
            int x = startUnit.PositionX;
            int y = startUnit.PositionY;
            //Debug.Log(x + "\t" + y);
            if (x != Rows - 1 &&
                checkedUnits[x + 1, y] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX + 1, startCell.PositionY]) &&
                Units[x + 1, y].Value == startUnit.Value
                ||
                x != 0 &&
                checkedUnits[x - 1, y] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX - 1, startCell.PositionY])&&
                Units[x - 1, y].Value == startUnit.Value
                ||
                y != Columns - 1 &&
                checkedUnits[x, y + 1] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX, startCell.PositionY + 1])&&
                Units[x, y + 1].Value == startUnit.Value
                ||
                y != 0 &&
                checkedUnits[x, y - 1] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX, startCell.PositionY - 1])&&
                Units[x, y - 1].Value == startUnit.Value)
            {
                return true;
            }

            //CheckedList.Add(startCell);
            checkedUnits[x, y] = true;
            return
                x != Rows - 1 &&
                checkedUnits[x + 1, y] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX + 1, startCell.PositionY]) &&
                CheckTheEnd(Units[x + 1, y], checkedUnits)
                ||
                x != 0 &&
                checkedUnits[x - 1, y] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX - 1, startCell.PositionY]) &&
                CheckTheEnd(Units[x - 1, y], checkedUnits)
                ||
                y != Columns - 1 &&
                checkedUnits[x, y + 1] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX, startCell.PositionY + 1]) &&
                CheckTheEnd(Units[x, y + 1], checkedUnits)
                ||
                y != 0 &&
                checkedUnits[x, y - 1] == false &&
                //!Contains(CheckedList, Units[startCell.PositionX, startCell.PositionY - 1]) &&
                CheckTheEnd(Units[x, y - 1], checkedUnits);
        }

        private bool CanCreateNewUnit { get; set; }
        
        #endregion

        
        #region Turn logic


        public void Turn(int side)
        {
            CanCreateNewUnit = true;
            SaveCellsState();
            LastMove = false;
            switch (side)
            {
                case 0: //→
                    for (int x = Rows - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < Columns; y++)
                        {
                            if (Units[x, y] == null) continue;
                            Units[x, y].WasChanged = false;
                            LastMove |= MoveUnit(Units[x, y], side);
                        }
                    }

                    break;
                case 1: //↑
                    for (int y = Columns - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < Rows; x++)
                        {
                            if (Units[x, y] == null) continue;
                            Units[x, y].WasChanged = false;
                            LastMove |= MoveUnit(Units[x, y], side);
                        }
                    }

                    break;
                case 2: //←
                    for (int x = 0; x < Rows; x++)
                    {
                        for (int y = 0; y < Columns; y++)
                        {
                            if (Units[x, y] == null) continue;
                            Units[x, y].WasChanged = false;
                            LastMove |= MoveUnit(Units[x, y], side);
                        }
                    }

                    break;
                case 3: //↓
                    for (int y = 0; y < Columns; y++)
                    {
                        for (int x = 0; x < Rows; x++)
                        {
                            if (Units[x, y] == null) continue;
                            Units[x, y].WasChanged = false;
                            LastMove |= MoveUnit(Units[x, y], side);
                        }
                    }

                    break;
            }

            if (!LastMove) return;
            CanUndo = true;
            CreateNewUnit();
            if (UnitCounter == SideLength * SideLength) CheckTheEnd();
        }

        public bool MoveUnit(Unit unit, int side)
        {
            Unit nextUnit = null;
            /*switch (side)
            {
                case 0:
                    break;
                case 1 :
                    break;
                case 2 :
                    break;
                case 3 :
                    break;
            }*/

            switch (side)
            {
                case 0:
                    if (unit.PositionX == Rows - 1) return false;
                    for (int x = unit.PositionX + 1; x < Rows; x++)
                    {
                        if (Units[x, unit.PositionY] == null) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case 1:
                    if (unit.PositionY == Columns - 1) return false;
                    for (int y = unit.PositionY + 1; y < Columns; y++)
                    {
                        if (Units[unit.PositionX, y] == null) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
                case 2:
                    if (unit.PositionX == 0) return false;
                    for (int x = unit.PositionX - 1; x >= 0; x--)
                    {
                        if (Units[x, unit.PositionY] == null) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case 3:
                    if (unit.PositionY == 0) return false;
                    for (int y = unit.PositionY - 1; y >= 0; y--)
                    {
                        if (Units[unit.PositionX, y] == null) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
            }


            if (nextUnit == null)
            {
                Units[unit.PositionX, unit.PositionY] = null;
                switch (side)
                {
                    case 0:
                        unit.PositionX = Rows - 1;
                        break;
                    case 1:
                        unit.PositionY = Columns - 1;
                        break;
                    case 2:
                        unit.PositionX = 0;
                        break;
                    case 3:
                        unit.PositionY = 0;
                        break;
                }

                Units[unit.PositionX, unit.PositionY] = unit;
                Drawer.UpdateUnit(unit);
                return true;
            }

            if (nextUnit.Value != unit.Value || nextUnit.WasChanged && nextUnit.Value == unit.Value)
            {
                switch (side)
                {
                    case 0:
                        if (nextUnit.PositionX == unit.PositionX + 1) return false;
                        Units[unit.PositionX, unit.PositionY] = null;
                        unit.PositionX = nextUnit.PositionX - 1;
                        break;
                    case 1:
                        if (nextUnit.PositionY == unit.PositionY + 1) return false;
                        Units[unit.PositionX, unit.PositionY] = null;
                        unit.PositionY = nextUnit.PositionY - 1;
                        break;
                    case 2:
                        if (nextUnit.PositionX == unit.PositionX - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = null;
                        unit.PositionX = nextUnit.PositionX + 1;
                        break;
                    case 3:
                        if (nextUnit.PositionY == unit.PositionY - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = null;
                        unit.PositionY = nextUnit.PositionY + 1;
                        break;
                }

                Units[unit.PositionX, unit.PositionY] = unit;
                Drawer.UpdateUnit(unit);
                return true;
            }

            if (nextUnit.Value == unit.Value)
            {
                Units[unit.PositionX, unit.PositionY] = null;
                UnitCounter--;
                switch (side)
                {
                    case 0:
                    case 2:
                        unit.PositionX = nextUnit.PositionX;
                        break;
                    case 1:
                    case 3:
                        unit.PositionY = nextUnit.PositionY;
                        break;
                }

                //Units[unit.PositionX, unit.PositionY] = nextUnit;
                //startPos.Remove(this);
                Drawer.UpdateUnit(unit, true);
                nextUnit.Value *= 2;
                nextUnit.WasChanged = true;
                Drawer.IncrementUnit(nextUnit);
                Data.Score += nextUnit.Value;
                GameManager.UpdateScoreText();
                //MainMap.DeleteCells.Add(nextCell);
                //Destroy(nextCell.Parent);
                return true;
            }

            return false;
        }
        
        #endregion
        
        
        #region Undo logic

        public bool CanUndo { get; set; }

        private bool LastMove = true;

        //private readonly List<int[]> LastCellsState = new List<int[]>();
        //private readonly List<int[]> LastCellsState_2 = new List<int[]>();
        //private int[][] LastLastCellsState = { };
        private int LastScore, LastLastScore;

        private int[,]  LastUnitsState;
        private int[,] LastLastUnitsState;
        
        private void SaveCellsState()
        {
            if (LastMove)
            {
                //Debug.Log("Saved");
                /*int[][] i = new int[LastCellsState.Count][];
                LastCellsState.CopyTo(i);
                LastLastCellsState = i;
                LastLastScore = LastScore;
                LastScore = Data.Score;
                LastCellsState.Clear();
                foreach (Cell cell in Units)
                {
                    LastCellsState.Add(new[] {cell.Position[0], cell.Position[1], cell.Value});
                }*/

                //int[,] n = new int[Rows, Columns];
                LastLastUnitsState = (int[,])LastUnitsState.Clone();
                
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (Units[i, j] != null) LastUnitsState[i, j] = Units[i, j].Value;
                        else LastUnitsState[i, j] = 0;
                    }
                }
                LastLastScore = LastScore;
                LastScore = Data.Score;
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                //Debug.Log("Undo   " + LastMove);
                if (LastMove)
                {
                    ClearGrid();
                    /*foreach (int[] cell in LastCellsState)
                    {
                        CreateNewUnit(cell[0], cell[1], cell[2]);
                    }*/
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (LastUnitsState[i, j] != 0)  CreateNewUnit(i,j,LastUnitsState[i, j], true);
                        }
                    }

                    Data.Score = LastScore;
                    GameManager.UpdateScoreText();
                    CanUndo = false;
                }
                else
                {
                    ClearGrid();
                    /*foreach (int[] cell in LastLastCellsState)
                    {
                        CreateNewCell(cell[0], cell[1], cell[2]);
                    }*/
                    for (int i = 0; i < Rows; i++)
                    {
                        for (int j = 0; j < Columns; j++)
                        {
                            if (LastLastUnitsState[i, j] != 0) CreateNewUnit(i,j,LastLastUnitsState[i, j], true);
                        }
                    }

                    Data.Score = LastLastScore;
                    GameManager.UpdateScoreText();
                    CanUndo = false;
                }
            }
        }

        public void ClearGrid()
        {
            /*Cells.Clear();
            foreach (GameObject cell in CellParents)
            {
                //cell.gameObject.SetActive(false);
                Destroy(cell.gameObject);
                Destroy(cell);
            }

            CellParents.Clear();*/
            
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Units[i, j] != null) Destroy(Units[i,j].gameObject);
                }
            }

            UnitCounter = 0;
        }
        #endregion
        
    }
}
