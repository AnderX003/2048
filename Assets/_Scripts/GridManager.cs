using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private RectTransform MapGameObject;
        [SerializeField] private GameManager GameManager;
        [SerializeField] private UIDrawer drawer;
        [SerializeField] private List<Unit> startUnits;
        private Queue<Unit> unitsPool = new Queue<Unit>();

        public Unit[,] Units { get; private set; }
        public UIDrawer Drawer => drawer;
        private bool BestsWasChanged;
        private int Rows;
        private int Columns;
        public int SideLength { get; private set; }
        private int UnitCounter;
        public bool isActive { get; set; }
        public int GridState { get; set; } = 0; // 0 - Nothing, 1 - Game, 2 - Pause, 3 - Game over

        #region Start Game and Creating new units

        private void Start()
        {
            foreach (Unit unit in startUnits)
            {
                unitsPool.Enqueue(unit);
            }
        }

        public void InitializeGrid(int sideLength)
        {
            GameManager.GameOver = false;
            SideLength = sideLength;
            Data.Score = 0;
            GameManager.UpdateScoreText();
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            isActive = true;
            GridState = 1;
            Rows = Columns = sideLength;
            Units = new Unit[Rows, Columns];
            UnitCounter = 0;
            Drawer.SetMapImagesBySize(SideLength);
            CreateNewUnit(canCreateSeveralUnits: true);
            CreateNewUnit(canCreateSeveralUnits: true);
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];
            GameManager.CurrentLocalData.StateIsSaved[sideLength - 3] = true;
        }

        private void Update()
        {
            //Debug.Log(UnitCounter + "              "+unitsPool.Count);
        }

        public void InitializeGridFromLocalData(int sideLength)
        {
            SideLength = sideLength;
            GameManager.GameOver = false;
            Data.Score = GameManager.CurrentLocalData.LastScores[sideLength - 3];
            GameManager.UpdateScoreText();
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            isActive = true;
            GridState = 1;
            Rows = Columns = sideLength;
            Units = new Unit[Rows, Columns];
            Drawer.SetMapImagesBySize(SideLength);
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];

            UnitCounter = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (GameManager.CurrentLocalData.GridState[sideLength - 3][i][j] != 0)
                        CreateNewUnit(i, j, GameManager.CurrentLocalData.GridState[sideLength - 3][i][j], true);
                }
            }

            CanUndo = false;
        }

        [SerializeField] private GameObject UnitPrefab;

        private void CreateNewUnit(int posX = -1, int posY = -1, int value = -1, bool canCreateSeveralUnits = false)
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

            Unit unit = unitsPool.Count > 0
                ? unitsPool.Dequeue() 
                : Instantiate(UnitPrefab, MapGameObject).GetComponent<Unit>();

            Units[positionX, positionY] = unit;
            unit.PositionX = positionX;
            unit.PositionY = positionY;
            int[] arr = {2, 2, 2, 4};
            unit.Value = value == -1 ? arr[Random.Range(0, 4)] : value;
            UnitCounter++;
            Drawer.DrawNewUnit(unit);
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
                    ////Debug.Log("Game Over");
                    GameManager.GameOver = true;
                    isActive = false;
                    GridState = 3;
                }
            });
        }

        private bool CheckTheEnd(Unit startUnit, bool[,] checkedUnits)
        {
            int x = startUnit.PositionX;
            int y = startUnit.PositionY;
            ////Debug.Log(x + "\t" + y);
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

            GameManager.CurrentLocalData.LastScores[SideLength - 3] = Data.Score;
            if (Data.Score > GameManager.CurrentLocalData.BestScores[SideLength - 3])
            {
                GameManager.CurrentLocalData.BestScores[SideLength - 3] = Data.Score;
                BestsWasChanged = true;
            }
        }

        private bool MoveUnit(Unit unit, int side)
        {
            Unit nextUnit = null;
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
                Drawer.UpdateUnit(unit, true, unitsPool);
                nextUnit.Value *= 2;
                nextUnit.WasChanged = true;
                Drawer.IncrementUnit(nextUnit);
                Data.Score += nextUnit.Value;
                GameManager.UpdateScoreText();
                if (SideLength == 4) UnlockAchievement(nextUnit.Value);
                return true;
            }

            return false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus) SaveDataToCloudAndToLocalMemory();
        }

        public void SaveDataToCloudAndToLocalMemory()
        {
            //Debug.Log("SaveDataToCloudAndToLocalMemory");
            SaveGridStateToLocalData();
            LocalDataManager.WriteLocalData(GameManager.CurrentLocalData);
            if (!BestsWasChanged) return;
            //Debug.Log("Saving data to cloud");
            AddScoreToLeaderboard(SideLength, GameManager.CurrentLocalData.BestScores[SideLength - 3]);
            GPGSManager.WriteDataToCloud(GameManager.dataName,
                Converter.ToByteArray(GameManager.CurrentLocalData.BestScores));
        }

        private void SaveGridStateToLocalData()
        {
            for (int i = 0; i < SideLength; i++)
            {
                for (int j = 0; j < SideLength; j++)
                {
                    GameManager.CurrentLocalData.GridState[SideLength - 3][i][j] = Units[i, j]?.Value ?? 0;
                }
            }
        }

        private static void UnlockAchievement(int value)
        {
            string id = value switch
            {
                512 => GPGSIds.achievement_512,
                1024 => GPGSIds.achievement_1024,
                2048 => GPGSIds.achievement_2048,
                4096 => GPGSIds.achievement_4096,
                8192 => GPGSIds.achievement_8192,
                _ => null
            };
            GPGSManager.UnlockAchievement(id);
        }

        private static void AddScoreToLeaderboard(int side, int value)
        {
            string id = side switch
            {
                3 => GPGSIds.leaderboard_3x3_leaderboard,
                4 => GPGSIds.leaderboard_4x4_leaderboard,
                5 => GPGSIds.leaderboard_5x5_leaderboard,
                6 => GPGSIds.leaderboard_6x6_leaderboard,
                7 => GPGSIds.leaderboard_7x7_leaderboard,
                8 => GPGSIds.leaderboard_8x8_leaderboard,
                _ => null
            };
            GPGSManager.AddScoreToLeaderboard(id, value);
        }

        #endregion

        #region Undo logic

        private bool CanUndo { get; set; }
        private bool LastMove = true;
        
        private int LastScore, LastLastScore;
        private int[,] LastUnitsState;
        private int[,] LastLastUnitsState;

        private void SaveCellsState()
        {
            if (!LastMove) return;
            LastLastUnitsState = (int[,]) LastUnitsState.Clone();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    LastUnitsState[i, j] = Units[i, j] == null ? 0 : Units[i, j].Value;
                }
            }

            LastLastScore = LastScore;
            LastScore = Data.Score;
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var unitsState = LastMove ? LastUnitsState : LastLastUnitsState;
            ClearGrid();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (unitsState[i, j] != 0) CreateNewUnit(i, j, unitsState[i, j], true);
                }
            }

            Data.Score = LastScore;
            GameManager.UpdateScoreText();
            CanUndo = false;
        }

        public void ClearGrid()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Units[i, j] != null)
                    {
                        unitsPool.Enqueue(Units[i, j]);
                        drawer.HideUnit(Units[i, j]);
                        Units[i, j] = null;
                    } //Destroy(Units[i, j].gameObject);
                }
            }

            UnitCounter = 0;
        }

        #endregion
    }
}
