using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Mono.Data.Sqlite;
using Unity.VisualScripting;
using UnityEngine;
using ArgumentException = System.ArgumentException;

public class DatabaseManager : Singleton<DatabaseManager>
{
    [SerializeField] private string dbName;
    [SerializeField] private Card cardPrefab;
    private int uniqueIdCounter = 0;

    public Card GetCardById(int cardId)
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
                card.Initialize(
                    reader.GetInt32(0),
                    this.uniqueIdCounter++,
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetInt32(3),
                    0);

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
    
    private string GetDatabasePath(string dbName)
    {
        #if UNITY_EDITOR
            return $"URI=file:{Application.dataPath}/Resources/{dbName}.db"; //Path to database
        #endif

        #pragma warning disable CS0162 // Unreachable code detected
        return "URI=file:" + Application.persistentDataPath + "/" + dbName + ".db"; //Path to database.
        #pragma warning restore CS0162 // Unreachable code detected
    }
}