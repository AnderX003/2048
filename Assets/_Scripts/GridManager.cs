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

        private Unit[,] Units;
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
                WaitForFrames(() =>
                {
                    StartCoroutine(
                        DrawUnits());
                }, 5));
        }

        private IEnumerator DrawUnits()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    yield return null;
                    if (Units[i, j] != emptyUnit) Drawer.DrawNewUnit(Units[i, j]);
                }
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

            int positionX = -1;
            int positionY = -1;
            if (UnitCounter + 1 == SideLength * SideLength)
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (Units[i, j] != emptyUnit) continue;
                        positionX = i;
                        positionY = j;
                        break;
                    }
                }
            }
            else
            {
                int MaxPos = Data.MaxValue + 1;
                do
                {
                    positionX = Random.Range(0, MaxPos);
                    positionY = Random.Range(0, MaxPos);
                } while (!Units[positionX, positionY].IsEmpty);
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

            Units[positionX, positionY] = unit;
            unit.PositionX = positionX;
            unit.PositionY = positionY;
            unit.Value = Random.Range(0, 4) == 0 ? 4 : 2;
            UnitCounter++;
            MoveIsOver = false;
            Drawer.SetNewUnitPosNScale(unit);
            StartCoroutine(InvokeWithDelay(Drawer.MoveDuration * Drawer.AppearDelay,
                () =>
                {
                    Drawer.DrawNewUnit(unit, false);
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
        }


        private static IEnumerator WaitForFrames(Action action, int frames)
        {
            for (int i = 0; i < frames; i++)
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

        public void Turn(Side side)
        {
            CanCreateNewUnit = true;
            SaveCellsState();

            LastMove = false;
            switch (side)
            {
                case Side.R: //→
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
                case Side.Up: 
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
                case Side.L: 
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
                case Side.Down: 
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
            if (action == null)
                yield break;
            yield return new WaitForSeconds(delay);
            action();
        }

        private bool MoveUnit(Unit unit, Side side)
        {
            Unit nextUnit = emptyUnit;
            switch (side)
            {
                case Side.R:
                    if (unit.PositionX == Rows - 1) return false;
                    for (int x = unit.PositionX + 1; x < Rows; x++)
                    {
                        if (Units[x, unit.PositionY].IsEmpty) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case Side.Up:
                    if (unit.PositionY == Columns - 1) return false;
                    for (int y = unit.PositionY + 1; y < Columns; y++)
                    {
                        if (Units[unit.PositionX, y].IsEmpty) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
                case Side.L:
                    if (unit.PositionX == 0) return false;
                    for (int x = unit.PositionX - 1; x >= 0; x--)
                    {
                        if (Units[x, unit.PositionY].IsEmpty) continue;
                        nextUnit = Units[x, unit.PositionY];
                        break;
                    }

                    break;
                case Side.Down:
                    if (unit.PositionY == 0) return false;
                    for (int y = unit.PositionY - 1; y >= 0; y--)
                    {
                        if (Units[unit.PositionX, y].IsEmpty) continue;
                        nextUnit = Units[unit.PositionX, y];
                        break;
                    }

                    break;
            }

            if (nextUnit.IsEmpty)
            {
                Units[unit.PositionX, unit.PositionY] = emptyUnit;
                switch (side)
                {
                    case Side.R:
                        unit.PositionX = Rows - 1;
                        break;
                    case Side.Up:
                        unit.PositionY = Columns - 1;
                        break;
                    case Side.L:
                        unit.PositionX = 0;
                        break;
                    case Side.Down:
                        unit.PositionY = 0;
                        break;
                }

                Units[unit.PositionX, unit.PositionY] = unit;
                Drawer.UpdateUnitPosition(unit);
                return true;
            }

            if (nextUnit.Value != unit.Value || nextUnit.WasChanged)
            {
                switch (side)
                {
                    case Side.R:
                        if (nextUnit.PositionX == unit.PositionX + 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionX = nextUnit.PositionX - 1;
                        break;
                    case Side.Up:
                        if (nextUnit.PositionY == unit.PositionY + 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionY = nextUnit.PositionY - 1;
                        break;
                    case Side.L:
                        if (nextUnit.PositionX == unit.PositionX - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionX = nextUnit.PositionX + 1;
                        break;
                    case Side.Down:
                        if (nextUnit.PositionY == unit.PositionY - 1) return false;
                        Units[unit.PositionX, unit.PositionY] = emptyUnit;
                        unit.PositionY = nextUnit.PositionY + 1;
                        break;
                }

                Units[unit.PositionX, unit.PositionY] = unit;
                Drawer.UpdateUnitPosition(unit);
                return true;
            }

            if (nextUnit.Value == unit.Value)
            {
                Units[unit.PositionX, unit.PositionY] = emptyUnit;
                UnitCounter--;
                switch (side)
                {
                    case Side.R:
                    case Side.L:
                        unit.PositionX = nextUnit.PositionX;
                        break;
                    case Side.Up:
                    case Side.Down:
                        unit.PositionY = nextUnit.PositionY;
                        break;
                }

                Drawer.UpdateUnitPosition(unit, true, unitsPool);
                nextUnit.Value *= 2;
                nextUnit.WasChanged = true;
                Drawer.IncrementUnit(nextUnit);
                Data.Score += nextUnit.Value;
                GameManager.UpdateScoreText();
                UnlockAchievement(nextUnit.Value, SideLength);
                return true;
            }

            return false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveDataToCloudAndToLocalMemory();
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

        private static void UnlockAchievement(int value, int side)
        {
            string id = side switch
            {
                3 => value switch
                {
                    256 => GPGSIds.achievement_256_at_33,
                    512 => GPGSIds.achievement_512_at_33,
                    1024 => GPGSIds.achievement_1024_at_33,
                    _ => null
                },
                4 => value switch
                {
                    512 => GPGSIds.achievement_512_at_44,
                    1024 => GPGSIds.achievement_1024_at_44,
                    2048 => GPGSIds.achievement_2048_at_44,
                    4096 => GPGSIds.achievement_4096_at_44,
                    8192 => GPGSIds.achievement_8192_at_44,
                    16384 => GPGSIds.achievement_16384_at_44,
                    _ => null
                },
                5 => value switch
                {
                    2048 => GPGSIds.achievement_2048_at_55,
                    _ => null
                },
                6 => value switch
                {
                    2048 => GPGSIds.achievement_2048_at_66,
                    _ => null
                },
                7 => value switch
                {
                    2048 => GPGSIds.achievement_2048_at_77,
                    _ => null
                },
                8 => value switch
                {
                    2048 => GPGSIds.achievement_2048_at_88,
                    _ => null
                },
                _ => null
            };
            GPGSManager.UnlockAchievement(id);
        }

        private static void AddScoreToLeaderboard(int side, int value)
        {
            string id = side switch
            {
                3 => GPGSIds.leaderboard_33_leaderboard,
                4 => GPGSIds.leaderboard_44_leaderboard,
                5 => GPGSIds.leaderboard_55_leaderboard,
                6 => GPGSIds.leaderboard_66_leaderboard,
                7 => GPGSIds.leaderboard_77_leaderboard,
                8 => GPGSIds.leaderboard_88_leaderboard,
                _ => null
            };
            GPGSManager.AddScoreToLeaderboard(id, value);
        }

        #endregion

        #region Undo logic

        public bool CanUndo { get; set; }
        private bool LastMove = true;

        private int LastScore;
        private int[,] LastUnitsState;
        private int[,] LastLastUnitsState;

        private void SaveCellsState()
        {
            if (!LastMove) return;
            LastLastUnitsState = (int[,])LastUnitsState.Clone();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    LastUnitsState[i, j] = Units[i, j].IsEmpty ? 0 : Units[i, j].Value;
                }
            }

            LastScore = Data.Score;
        }

        public void Undo()
        {
            MoveIsOver = false;
            StartCoroutine(InvokeWithDelay(0.1f, () => { MoveIsOver = true; }));
            var unitsState = LastMove ? LastUnitsState : LastLastUnitsState;
            ClearGrid();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (unitsState[i, j] != 0) CreateNewUnit(i, j, unitsState[i, j], true);
                }
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Units[i, j] != emptyUnit) Drawer.DrawNewUnit(Units[i, j]);
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

    public enum Side
    {
        R,
        Up,
        L,
        Down
    }
}
