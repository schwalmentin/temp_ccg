using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UnityEngine;
using UnityEngine.Networking;

public class DatabaseManager : Singleton<DatabaseManager>
{
    #region Variables

        [Header("Database")]
        [SerializeField] private string dbName = "DataBase";
        [SerializeField] private Card cardPrefab;
        private string dbPath;

    #endregion

    #region Unity Methods

        protected override async void Awake()
        {
            base.Awake();

            this.dbPath = this.GetDbPath(this.dbName);
            await this.CopyDbFromStreamingAssets(this.dbName, this.dbPath);
        }

    #endregion

    #region DataBase Public Methods

        /// <summary>
        /// Fetches the card properties from the DB and instantiates a new card object.
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="uniqueId"></param>
        /// <returns>Returns an instantiated card object.</returns>
        public Card GetCardById(int cardId, int uniqueId)
        {
            SqliteConnection connection = new SqliteConnection($"URI=file:{this.dbPath}");
                
            try
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM card WHERE id = {cardId}";

                IDataReader reader = command.ExecuteReader();
                    
                while (reader.Read())
                {
                    Card card = Instantiate(this.cardPrefab);
                    
                    string description;
                    try { description = reader.GetString(4); } catch { description = ""; }
                        
                    string actionId;
                    try { actionId = reader.GetString(5); } catch { actionId = ""; }

                    card.Initialize(
                        reader.GetInt32(0),
                        uniqueId,
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        description,
                        actionId);

                    connection.Close();
                    return card;
                }
            }
            catch (SqliteException sqliteException)
            {
                Debug.LogException(sqliteException);
                connection.Close();
            }

            Logger.LogError($"The card with the requested id {cardId} does not exist!");
            return null;
        }

    #endregion

    #region DatabaseManager Methods
        
        /// <summary>
        /// Returns the DB path of the current system.
        /// Supports Unity, Windows and Android.
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>dbPath</returns>
        private string GetDbPath(string dbName)
        {
            return Application.platform switch
            {
                // Android
                RuntimePlatform.Android => $"{Application.persistentDataPath}/{dbName}.db",
                
                // IOS
                RuntimePlatform.IPhonePlayer => $"...",
                
                // Unity Editor, Windows
                _ => $"{Application.streamingAssetsPath}/{dbName}.db"
            };
        }

        /// <summary>
        /// Copies the database file from the streaming asset folder to a persistent data path, in case of Android or IOS
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="targetPath"></param>
        private async Task CopyDbFromStreamingAssets(string dbName, string targetPath)
        {
            // Return if the platform is not mobile
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) return;
            
            // Define source path from streaming assets
            string sourcePath = $"{Application.streamingAssetsPath}/{dbName}.db";

            // Download database from source path
            using UnityWebRequest www = UnityWebRequest.Get(sourcePath);
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            
            // Check if download was successful
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load database: " + www.error);
                return;
            }

            // Save the database to target path
            await File.WriteAllBytesAsync(targetPath, www.downloadHandler.data);
        }

    #endregion
}