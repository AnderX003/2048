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

                public static void ShowAchievementsUI()
                {
                    Social.ShowAchievementsUI();
                }

                private static void UnlockAchievement(string id)
                {
                    if (id == null) return;
                    Social.ReportProgress(id, 100, success => { });
                }
                
                public static void UnlockAchievement(int value, int side)
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
                    UnlockAchievement(id);
                }

                #endregion

                #region Leaderboards

                public static void ShowLeaderboardUI()
                {
                    Social.ShowLeaderboardUI();
                }

                private static void AddScoreToLeaderboard(string id, long score)
                {
                    Social.ReportScore(score, id, success => { });
                }

                public static void AddScoreToLeaderboard(int side, int value)
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
                    AddScoreToLeaderboard(id, value);
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
