using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace _Scripts
{
    public class GridManager : MonoBehaviour
    {
        #region FieldsAndProrerties
        
        public GridState State { get; set; }
        public UnitsDrawer Drawer => drawer;
        public int SideLength { get; private set; }
        public bool IsActive { get; set; }
        public bool MoveIsOver { get; private set; } = true;
        public bool CanUndo { get; private set; }
        
        [SerializeField] private RectTransform MapGameObject;
        [SerializeField] private GameManager GameManager;
        [SerializeField] private UnitsDrawer drawer;
        [SerializeField] private Unit[] startUnits;
        [SerializeField] private Unit emptyUnit;
        [SerializeField] private GameObject UnitPrefab;
        

        private readonly Queue<Unit> unitsPool = new Queue<Unit>(70);
        private readonly List<Vector2Int> freePositions = new List<Vector2Int>(33);
        private Unit[,] Units;
        private int Rows;
        private int Columns;
        private int UnitCounter;
        private int[,] LastUnitsState;
        private int[,] LastLastUnitsState;
        private int LastScore;
        private bool BestsWasChanged;
        private bool LastMoveWasDone = true;
        
        #endregion

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
            Rows = Columns = SideLength;
            Units = GetNewEmptyGrid();
            UnitCounter = 0;
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            Data.Score = 0;
            State = GridState.Game;
            IsActive = true;
            CanUndo = false;

            CreateNewUnit();
            CreateNewUnit();
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];
            GameManager.CurrentLocalData.StateIsSaved[SideLength - 3] = true;
            
            drawer.SetGridImageBySize(SideLength);
            GameManager.UpdateScoreText();
        }

        public void InitializeGridFromLocalData(int sideLength)
        {
            SideLength = sideLength;
            Rows = Columns = SideLength;
            Units = GetNewEmptyGrid();
            UnitCounter = 0;
            Data.MaxValue = SideLength - 1;
            Data.CurrentLayout = Data.layouts[SideLength];
            Data.Score = GameManager.CurrentLocalData.LastScores[SideLength - 3];
            State = GridState.Game;
            CanUndo = false;
            
            CreateUnitsFromState(GameManager.CurrentLocalData.GridState[SideLength - 3]);
            LastUnitsState = new int[Rows, Columns];
            LastLastUnitsState = new int[Rows, Columns];
            
            drawer.SetGridImageBySize(SideLength);

            Wait.ForFrames(Drawer.FramesBeforeDrawingUnits, () =>
            {
                StartCoroutine(DrawUnitsWithDelay());
            });
            
            GameManager.UpdateScoreText();
        }

        private Unit[,] GetNewEmptyGrid()
        {
            var units = new Unit[Rows, Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    units[i, j] = emptyUnit;
                }
            }

            return units;
        }

        private void CreateUnitsFromState(int[][] state)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (state[i][j] == 0) continue;
                    CreateNewUnit(new Vector2Int(i, j), state[i][j]);
                }
            }
        }

        private IEnumerator DrawUnitsWithDelay()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    yield return null;
                    if (Units[i, j] != emptyUnit) drawer.DrawNewUnit(Units[i, j]);
                }
            }
            
            IsActive = true;
        }
        
        private void CreateNewUnit()
        {
            Unit unit = GetUnitFromPool();
            unit.Position = GenerateRandomFreePosition();
            unit.Value = Random.Range(0, 4) == 0 ? 4 : 2;
            Units[unit.Position.x, unit.Position.y] = unit;
            UnitCounter++;
            
            drawer.SetNewUnitPosNScale(unit);
            MoveIsOver = false;
            Wait.ForSeconds(drawer.MoveDuration * Drawer.AppearDelay, () =>
            {
                drawer.DrawNewUnit(unit, false);
                MoveIsOver = true;
            });
        }

        private Vector2Int GenerateRandomFreePosition()
        {
            Vector2Int position = default;
            int MaxPos = Data.MaxValue + 1;
            
            if (UnitCounter > Rows * Columns / 2)
            {
                for (int i = 0; i < MaxPos; i++)
                {
                    for (int j = 0; j < MaxPos; j++)
                    {
                        if (!Units[i, j].IsEmpty) continue;
                        freePositions.Add(new Vector2Int(i, j));
                    }
                }

                position = freePositions[Random.Range(0, freePositions.Count)];
                freePositions.Clear();
            }
            else
            {
                do
                {
                    position.x = Random.Range(0, MaxPos);
                    position.y = Random.Range(0, MaxPos);
                } while (!Units[position.x, position.y].IsEmpty);
            }

            return position;
        }

        private Unit GetUnitFromPool()
        {
            if (unitsPool.Count > 0)
            {
                return unitsPool.Dequeue();
            }
            else
            {
                Unit unit = Instantiate(UnitPrefab, MapGameObject).GetComponent<Unit>();
                drawer.HideUnit(unit);
                return unit;
            }
        }
        
        private void CreateNewUnit(Vector2Int position, int value)
        {
            Unit unit = GetUnitFromPool();
            unit.Position = position;
            unit.Value = value;
            Units[unit.Position.x, unit.Position.y] = unit;
            UnitCounter++;
        }

        #endregion

        public void ClearGrid()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Units[i, j].IsEmpty) continue;
                    unitsPool.Enqueue(Units[i, j]);
                    drawer.HideUnit(Units[i, j]);
                    Units[i, j] = emptyUnit;
                }
            }

            UnitCounter = 0;
        }
        
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
            int x = startUnit.Position.x;
            int y = startUnit.Position.y;
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

        #endregion

        #region Turn logic

        public void Turn(Side side)
        {
            SaveCellsState();

            LastMoveWasDone = MoveUnitsInSideOrder(side);

            if (!LastMoveWasDone) return;
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

        private bool MoveUnitsInSideOrder(Side side)
        {
            bool lastMoveWasDone = false;
            switch (side)
            {
                case Side.R:
                    for (int x = Rows - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < Columns; y++)
                        {
                            if (Units[x, y].IsEmpty) continue;
                            Units[x, y].WasChanged = false;
                            lastMoveWasDone |= MoveUnit(Units[x, y], side);
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
                            lastMoveWasDone |= MoveUnit(Units[x, y], side);
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
                            lastMoveWasDone |= MoveUnit(Units[x, y], side);
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
                            lastMoveWasDone |= MoveUnit(Units[x, y], side);
                        }
                    }

                    break;
            }

            return lastMoveWasDone;
        }
        
        private bool MoveUnit(Unit unit, Side side)
        {
            if (CheckIfAtExtremePosition(unit, side)) return false;
            Unit nextUnit = FindNextUnit(unit, side);

            if (nextUnit.IsEmpty) 
            {
                MoveToExtremePosition(unit, side);
                drawer.UpdateUnitPosition(unit);
                return true;
            }

            if (nextUnit.Value != unit.Value || nextUnit.WasChanged)
            {
                if (CheckIfCanPushToNextUnit(unit, nextUnit, side))
                {
                    PushToNextUnit(unit, nextUnit, side);
                    drawer.UpdateUnitPosition(unit);
                    return true;
                }

                return false;
            }

            if (nextUnit.Value == unit.Value)
            {
                JoinUnits(unit, nextUnit, side);

                drawer.UpdateUnitPosition(unit, () =>
                {
                    unitsPool.Enqueue(unit);
                    drawer.HideUnit(unit);
                });

                drawer.IncrementUnit(nextUnit);

                Data.Score += nextUnit.Value;
                GameManager.UpdateScoreText();
                UnlockAchievement(nextUnit.Value, SideLength);

                return true;
            }

            return false;
        }

        private bool CheckIfAtExtremePosition(Unit unit, Side side)
        {
            switch (side)
            {
                case Side.R:
                    if (unit.Position.x == Rows - 1) return true;
                    break;
                case Side.Up:
                    if (unit.Position.y == Columns - 1) return true;
                    break;
                case Side.L:
                    if (unit.Position.x == 0) return true;
                    break;
                case Side.Down:
                    if (unit.Position.y == 0) return true;
                    break;
            }

            return false;
        }
        
        private Unit FindNextUnit(Unit unit, Side side)
        {
            switch (side)
            {
                case Side.R:
                    for (int x = unit.Position.x + 1; x < Rows; x++)
                    {
                        if (Units[x, unit.Position.y].IsEmpty) continue;
                        return Units[x, unit.Position.y];
                    }

                    break;
                case Side.Up:
                    for (int y = unit.Position.y + 1; y < Columns; y++)
                    {
                        if (Units[unit.Position.x, y].IsEmpty) continue;
                        return Units[unit.Position.x, y];
                    }

                    break;
                case Side.L:
                    for (int x = unit.Position.x - 1; x >= 0; x--)
                    {
                        if (Units[x, unit.Position.y].IsEmpty) continue;
                        return Units[x, unit.Position.y];
                    }

                    break;
                case Side.Down:
                    for (int y = unit.Position.y - 1; y >= 0; y--)
                    {
                        if (Units[unit.Position.x, y].IsEmpty) continue;
                        return Units[unit.Position.x, y];
                    }

                    break;
            }

            return emptyUnit;
        }

        private void MoveToExtremePosition(Unit unit, Side side)
        {
            Units[unit.Position.x, unit.Position.y] = emptyUnit;
            switch (side)
            {
                case Side.R:
                    unit.Position = new Vector2Int(Rows - 1, unit.Position.y);
                    break;
                case Side.Up:
                    unit.Position = new Vector2Int(unit.Position.x, Columns - 1);
                    break;
                case Side.L:
                    unit.Position = new Vector2Int(0, unit.Position.y);
                    break;
                case Side.Down:
                    unit.Position = new Vector2Int(unit.Position.x, 0);
                    break;
            }

            Units[unit.Position.x, unit.Position.y] = unit;
        }

        private bool CheckIfCanPushToNextUnit(Unit unit, Unit nextUnit, Side side)
        {
            switch (side)
            {
                case Side.R:
                    if (nextUnit.Position.x == unit.Position.x + 1) return false;
                    break;
                case Side.Up:
                    if (nextUnit.Position.y == unit.Position.y + 1) return false;
                    break;
                case Side.L:
                    if (nextUnit.Position.x == unit.Position.x - 1) return false;
                    break;
                case Side.Down:
                    if (nextUnit.Position.y == unit.Position.y - 1) return false;
                    break;
            }

            return true;
        }

        private void PushToNextUnit(Unit unit, Unit nextUnit, Side side)
        {
            switch (side)
            {
                case Side.R:
                    Units[unit.Position.x, unit.Position.y] = emptyUnit;
                    unit.Position = new Vector2Int(nextUnit.Position.x - 1, unit.Position.y);
                    break;
                case Side.Up:
                    Units[unit.Position.x, unit.Position.y] = emptyUnit;
                    unit.Position = new Vector2Int(unit.Position.x, nextUnit.Position.y - 1);
                    break;
                case Side.L:
                    Units[unit.Position.x, unit.Position.y] = emptyUnit;
                    unit.Position = new Vector2Int(nextUnit.Position.x + 1, unit.Position.y);
                    break;
                case Side.Down:
                    Units[unit.Position.x, unit.Position.y] = emptyUnit;
                    unit.Position = new Vector2Int(unit.Position.x, nextUnit.Position.y + 1);
                    break;
            }

            Units[unit.Position.x, unit.Position.y] = unit;
        }

        private void JoinUnits(Unit unit, Unit nextUnit, Side side)
        {
            Units[unit.Position.x, unit.Position.y] = emptyUnit;
            UnitCounter--;
            switch (side)
            {
                case Side.R:
                case Side.L:
                    unit.Position = new Vector2Int(nextUnit.Position.x, unit.Position.y);
                    break;
                case Side.Up:
                case Side.Down:
                    unit.Position = new Vector2Int(unit.Position.x, nextUnit.Position.y);
                    break;
            }
                
            nextUnit.Value *= 2;
            nextUnit.WasChanged = true;
        }
        
        #endregion
        
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

        #region Undo logic

        private void SaveCellsState()
        {
            if (!LastMoveWasDone) return;
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
            var unitsState = LastMoveWasDone ? LastUnitsState : LastLastUnitsState;
            
            ClearGrid();
            CreateUnitsFromState(unitsState);
            DrawUnits();
            
            Data.Score = LastScore;
            GameManager.UpdateScoreText();
            CanUndo = false;
            Wait.ForSeconds(Drawer.DelayAfterUndo, () => MoveIsOver = true);
        }

        private void CreateUnitsFromState(int[,] state)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (state[i, j] == 0) continue;
                    CreateNewUnit(new Vector2Int(i, j), state[i, j]);
                }
            }
        }

        private void DrawUnits()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Units[i, j] == emptyUnit) continue;
                    drawer.DrawNewUnit(Units[i, j]);
                }
            }
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
