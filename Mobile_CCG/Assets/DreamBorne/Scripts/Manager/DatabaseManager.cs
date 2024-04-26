using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    #region Variables

        [Header("Database")]
        [SerializeField] private string dbName;
        [SerializeField] private Card cardPrefab;

    #endregion

    #region DatabaseManager Methods

        /// <summary>
        /// Gets the card properties from the DB and instantiates a new card object.
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="uniqueId"></param>
        /// <returns>Returns an instantiated card object.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Card GetCardById(int cardId, int uniqueId)
        {
            SqliteConnection connection = new SqliteConnection(this.GetDatabasePath(this.dbName));
            
            try
            {
                connection.Open();

                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM cards WHERE id = {cardId}";

                IDataReader reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    Card card = Instantiate(this.cardPrefab);
                    
                    string actionId;
                    try { actionId = reader.GetString(4) == null ? "" : reader.GetString(4); }
                    catch { actionId = ""; }
                    
                    card.Initialize(
                        reader.GetInt32(0),
                        uniqueId,
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
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

            throw new KeyNotFoundException($"The card with the requested id {cardId} does not exist!");
        }
        
        /// <summary>
        /// Returns the DB path of the current system.
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>DB path.</returns>
        private string GetDatabasePath(string dbName)
        {
            #if UNITY_EDITOR
            return $"URI=file:{Application.dataPath}/Resources/{dbName}.db"; //Path to database
            #endif

            #pragma warning disable CS0162 // Unreachable code detected
            return "URI=file:" + Application.persistentDataPath + "/" + dbName + ".db"; //Path to database.
            #pragma warning restore CS0162 // Unreachable code detected
        }

    #endregion
}