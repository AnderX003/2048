namespace _Scripts
{
    [System.Serializable]
    public class LocalData
    {
        public int LastGameMode = 4;
        public int[] BestScores = {0, 0, 0, 0, 0, 0};
        public int[] LastScores = {0, 0, 0, 0, 0, 0};
        public bool[] StateIsSaved = {false, false, false, false, false, false};

        public int[][][] GridState =
        {
            new[] {new[] {0, 0, 0}, new[] {0, 0, 0}, new[] {0, 0, 0}},
            new[] {new[] {0, 0, 0,0}, new[] {0, 0, 0,0}, new[] {0, 0, 0,0}, new[] {0, 0, 0,0}},
            new[] {new[] {0, 0, 0,0, 0}, new[] {0, 0, 0,0, 0}, new[] {0, 0, 0,0, 0}, new[] {0, 0, 0,0, 0}, new[] {0, 0, 0,0, 0}},
            new[] {new[] {0, 0, 0,0, 0,0}, new[] {0, 0, 0,0, 0,0}, new[] {0, 0, 0,0, 0,0}, new[] {0, 0, 0,0, 0,0}, new[] {0, 0, 0,0, 0,0}, new[] {0, 0, 0,0, 0,0}},
            new[] {new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}, new[] {0, 0, 0,0, 0,0,0}},
            new[] {new[] {0, 0, 0,0, 0,0,0,0}, new[] {0, 0, 0,0,0, 0,0,0}, new[] {0, 0, 0,0,0, 0,0,0}, new[] {0, 0,0, 0,0, 0,0,0}, new[] {0, 0,0, 0,0, 0,0,0}, new[] {0, 0, 0,0,0, 0,0,0}, new[] {0, 0,0, 0,0, 0,0,0}, new[] {0, 0, 0,0,0, 0,0,0}}
        };
        public bool AudioState = false;
    }
}
