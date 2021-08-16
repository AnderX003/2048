using System;
using System.Collections;
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
        [SerializeField] private Unit emptyUnit;
        private readonly Queue<Unit> unitsPool = new Queue<Unit>();

        public Unit[,] Units { get; private set; }
        public UIDrawer Drawer => drawer;
        private bool BestsWasChanged;
        private int Rows;
        private int Columns;
        public int SideLength { get; private set; }
        private int UnitCounter;
        public bool IsActive { get; set; }
        public bool MoveIsOver { get; private set; } = true;
        public GridState State { get; set; }

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
            SideLength = sideLength;
            Data.Score = 0;
            GameManager.UpdateScoreText();
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            IsActive = true;
            State = GridState.Game;
            Rows = Columns = sideLength;
            Units = new Unit[Rows, Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Units[i, j] = emptyUnit;
                }
            }
            UnitCounter = 0;
            Drawer.SetMapImagesBySize(SideLength);
            CreateNewUnit(true);
            CreateNewUnit(true);
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];
            GameManager.CurrentLocalData.StateIsSaved[sideLength - 3] = true;
        }
        
        private readonly Queue<Unit> unitsToDraw = new Queue<Unit>();

        public void InitializeGridFromLocalData(int sideLength)
        {
            SideLength = sideLength;
            Data.Score = GameManager.CurrentLocalData.LastScores[sideLength - 3];
            GameManager.UpdateScoreText();
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
                //IsActive = true;
            State = GridState.Game;
            Rows = Columns = sideLength;
            Units = new Unit[Rows, Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Units[i, j] = emptyUnit;
                }
            }
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
            StartCoroutine(
                WaitForFrames(() => {
                StartCoroutine(
                    DrawUnits()); }, 5));
        }

        private IEnumerator DrawUnits()
        {
            int count = unitsToDraw.Count;
            for (int i = 0; i < count; i++)
            {
                yield return null;
                Drawer.DrawNewUnit(unitsToDraw.Dequeue());
            }
            IsActive = true;
        }

        [SerializeField] private GameObject UnitPrefab;

        private void CreateNewUnit(bool canCreateSeveralUnits = false)
        {
            if (UnitCounter == Rows * Columns) return;
            if (!canCreateSeveralUnits)
            {
                if (!CanCreateNewUnit) return;
                CanCreateNewUnit = false;
            }

            int positionX;
            int positionY;
            int MaxPos = Data.MaxValue + 1;
            do
            {
                positionX = Random.Range(0, MaxPos);
                positionY = Random.Range(0, MaxPos);
            } while (!Units[positionX, positionY].IsEmpty);

            Unit unit;
            if (unitsPool.Count > 0)
            {
                unit = unitsPool.Dequeue();
            }
            else
            {
                unit = Instantiate(UnitPrefab, MapGameObject).GetComponent<Unit>();
                Drawer.HideUnit(unit);
            }

            Units[positionX, positionY] = unit;
            unit.PositionX = positionX;
            unit.PositionY = positionY;
            unit.Value = Random.Range(0, 4) == 0 ? 4 : 2;
            UnitCounter++;
            MoveIsOver = false;
            StartCoroutine(InvokeWithDelay(Drawer.MoveDuration, () =>
            {
                Drawer.DrawNewUnit(unit);
                MoveIsOver = true;
            }));
        }

        private void CreateNewUnit(int posX, int posY, int value, bool canCreateSeveralUnits = false)
        {
            if (UnitCounter == Rows * Columns) return;
            if (!canCreateSeveralUnits)
            {
                if (!CanCreateNewUnit) return;
                CanCreateNewUnit = false;
            }

            Unit unit;
            if (unitsPool.Count > 0)
            {
                unit = unitsPool.Dequeue();
            }
            else
            {
                unit = Instantiate(UnitPrefab, MapGameObject).GetComponent<Unit>();
                Drawer.HideUnit(unit);
            }

            Units[posX, posY] = unit;
            unit.PositionX = posX;
            unit.PositionY = posY;
            unit.Value = value;
            UnitCounter++;
            unitsToDraw.Enqueue(unit);
            //StartCoroutine(WaitOneFrame(() => Drawer.DrawNewUnit(unit), 10));
        }

        
        private static IEnumerator WaitForFrames(Action action, int frames)
        {
            for (int i=0; i < frames; i++)
            {
                yield return null;
            }
            action();
        }
        
        #endregion

        #region Game Over Logic

        private async void CheckTheEnd()
        {
            bool gameOver = await Task.Run(() =>
            {
                bool[,] checkedList = new bool[Rows, Columns];
                if (!CheckTheEnd(Units[1, 1], checkedList))
                {
                    IsActive = false;
                    State = GridState.GameOver;
                    return true;
                }

                return false;
            });
            if (gameOver) GameManager.GameOverMethod();
        }

        private bool CheckTheEnd(Unit startUnit, bool[,] checkedUnits)
        {
            int x = startUnit.PositionX;
            int y = startUnit.PositionY;
            if (x != Rows - 1 &&
                checkedUnits[x + 1, y] == false &&
                Units[x + 1, y].Value == startUnit.Value
                ||
                x != 0 &&
                checkedUnits[x - 1, y] == false &&
                Units[x - 1, y].Value == startUnit.Value
                ||
                y != Columns - 1 &&
                checkedUnits[x, y + 1] == false &&
                Units[x, y + 1].Value == startUnit.Value
                ||
                y != 0 &&
                checkedUnits[x, y - 1] == false &&
                Units[x, y - 1].Value == startUnit.Value)
            {
                return true;
            }

            checkedUnits[x, y] = true;
            return
                x != Rows - 1 &&
                checkedUnits[x + 1, y] == false &&
                CheckTheEnd(Units[x + 1, y], checkedUnits)
                ||
                x != 0 &&
                checkedUnits[x - 1, y] == false &&
                CheckTheEnd(Units[x - 1, y], checkedUnits)
                ||
                y != Columns - 1 &&
                checkedUnits[x, y + 1] == false &&
                CheckTheEnd(Units[x, y + 1], checkedUnits)
                ||
                y != 0 &&
                checkedUnits[x, y - 1] == false &&
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
                            if (Units[x, y].IsEmpty) continue;
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
                            if (Units[x, y].IsEmpty) continue;
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
                            if (Units[x, y].IsEmpty) continue;
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
                            if (Units[x, y].IsEmpty) continue;
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
        
        public static IEnumerator InvokeWithDelay(float delay, Action action)
        {
            if(action == null)
                yield break;
            yield return new WaitForSeconds(delay);
            action();
        }

        private bool MoveUnit(Unit unit, int side)
        {
            Unit nextUnit = emptyUnit;
            switch (side)
            {
                case 0:
                    if (unit.PositionX == Rows - 1) return false;
                    for (int x = unit.PositionX + 1; x < Rows; x++)
                    {
                        if (Units[x, unit.PositionY].IsEmpty) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case 1:
                    if (unit.PositionY == Columns - 1) return false;
                    for (int y = unit.PositionY + 1; y < Columns; y++)
                    {
                        if (Units[unit.PositionX, y].IsEmpty) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
                case 2:
                    if (unit.PositionX == 0) return false;
                    for (int x = unit.PositionX - 1; x >= 0; x--)
                    {
                        if (Units[x, unit.PositionY] .IsEmpty) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case 3:
                    if (unit.PositionY == 0) return false;
                    for (int y = unit.PositionY - 1; y >= 0; y--)
                    {
                        if (Units[unit.PositionX, y] .IsEmpty) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
            }

            if (nextUnit .IsEmpty)
            {
                Units[unit.PositionX, unit.PositionY] = emptyUnit;
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
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionX = nextUnit.PositionX - 1;
                        break;
                    case 1:
                        if (nextUnit.PositionY == unit.PositionY + 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionY = nextUnit.PositionY - 1;
                        break;
                    case 2:
                        if (nextUnit.PositionX == unit.PositionX - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionX = nextUnit.PositionX + 1;
                        break;
                    case 3:
                        if (nextUnit.PositionY == unit.PositionY - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionY = nextUnit.PositionY + 1;
                        break;
                }

                Units[unit.PositionX, unit.PositionY] = unit;
                Drawer.UpdateUnit(unit);
                return true;
            }

            if (nextUnit.Value == unit.Value)
            {
                Units[unit.PositionX, unit.PositionY] = emptyUnit;
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
            SaveGridStateToLocalData();
            LocalDataManager.WriteLocalData(GameManager.CurrentLocalData);
            if (!BestsWasChanged) return;
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
                16384 => GPGSIds.achievement_16384,
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
        
        private int LastScore;
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
                    LastUnitsState[i, j] = Units[i, j] .IsEmpty ? 0 : Units[i, j].Value;
                }
            }

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

            StartCoroutine(
                WaitForFrames(
                    () =>
                    {
                        int count = unitsToDraw.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Drawer.DrawNewUnit(unitsToDraw.Dequeue());
                        }
                    }, 2));

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
                    if (!Units[i, j].IsEmpty)
                    {
                        unitsPool.Enqueue(Units[i, j]);
                        drawer.HideUnit(Units[i, j]);
                        Units[i, j] = emptyUnit;
                    }
                }
            }

            UnitCounter = 0;
        }

        #endregion
    }

    public enum GridState
    {
        Nothing,
        Game,
        Pause,
        GameOver,
        Quitting
    }
}
