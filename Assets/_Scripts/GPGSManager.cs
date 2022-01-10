        using System;
        using System.Globalization;
        using GooglePlayGames;
        using GooglePlayGames.BasicApi;
        using GooglePlayGames.BasicApi.SavedGame;
        using UnityEngine;

        namespace _Scripts
        {
            public class GPGSManager : MonoBehaviour
            {
                #region Initializing and Signing in/out

                public static void Initialize()
                {
                    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                        .EnableSavedGames()
                        .Build();

                    PlayGamesPlatform.InitializeInstance(config);
                    PlayGamesPlatform.Activate();
                }

                public static void SignIn(Action actionWithButton, string dataName, Action<byte[]> actionAfterReading)
                {
                    if(isProcessing) return;
                    isProcessing = true;
                    Social.localUser.Authenticate(success =>
                    {
                        isProcessing = false;
                        actionWithButton();
                        if (success && PlayerPrefs.GetInt("DataWasReadFromCloud") == 0)
                            ReadDataFromCloud(dataName, actionAfterReading);
                    });
                }

                public static void SignOut()
                {
                    if (!Social.localUser.authenticated) return;
                    PlayGamesPlatform.Instance.SignOut();
                    PlayerPrefs.SetInt("DataWasReadFromCloud", 0);
                    GameManager.Instance.CurrentLocalData = new LocalData();
                    LocalDataManager.WriteLocalData(GameManager.Instance.CurrentLocalData);
                }

                public static bool Authenticated()
                {
                    return Social.localUser.authenticated;
                }

                #endregion

                #region Achievements

                public static void UnlockAchievement(string id)
                {
                    if (id == null) return;
                    Social.ReportProgress(id, 100, success => { });
                }

                public static void ShowAchievementsUI()
                {
                    Social.ShowAchievementsUI();
                }

                #endregion

                #region Leaderboards

                public static void AddScoreToLeaderboard(string id, long score)
                {
                    Social.ReportScore(score, id, success => { });
                }

                public static void ShowLeaderboardUI()
                {
                    Social.ShowLeaderboardUI();
                }

                #endregion

                #region Saved Games

                private static bool isProcessing;
                private static byte[] dataToWrite;
                private static Action<byte[]> actionAfterRead;

                public static void WriteDataToCloud(string name, byte[] data)
                {
                    dataToWrite = data;
                    OpenConnection(name, WriteOnConnectionOpen);
                }

                private static void ReadDataFromCloud(string name, Action<byte[]> action)
                {
                    actionAfterRead = action;
                    OpenConnection(name, ReadOnConnectionOpen);
                }

                private static void OpenConnection(string name,
                    Action<SavedGameRequestStatus, ISavedGameMetadata> onConnectionOpen)
                {
                    if (isProcessing || !Social.localUser.authenticated) return;
                    isProcessing = true;
                    PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(name,
                        DataSource.ReadCacheOrNetwork,
                        ConflictResolutionStrategy.UseMostRecentlySaved, onConnectionOpen);
                }

                private static void WriteOnConnectionOpen(SavedGameRequestStatus status, ISavedGameMetadata metadata)
                {
                    if (status != SavedGameRequestStatus.Success) return;
                    SavedGameMetadataUpdate updatedMetadata = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Saved at " + DateTime.Now.ToString(CultureInfo.InvariantCulture))
                        .Build();

                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(metadata, updatedMetadata, dataToWrite,
                        (savedGameRequestStatus, savedGameMetadata) =>
                        {
                            isProcessing = false;
                        });
                }

                private static void ReadOnConnectionOpen(SavedGameRequestStatus status, ISavedGameMetadata metadata)
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(metadata,
                        (savedGameRequestStatus, data) =>
                        {
                            isProcessing = false;
                            if (status != SavedGameRequestStatus.Success) return;
                            PlayerPrefs.SetInt("DataWasReadFromCloud", 1);
                            actionAfterRead(data);
                        });
                }


                #endregion
            }
        }
