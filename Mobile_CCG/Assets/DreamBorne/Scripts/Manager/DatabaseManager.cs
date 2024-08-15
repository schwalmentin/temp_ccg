using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class DatabaseManager : Singleton<DatabaseManager>
{
    #region Variables

        [Header("Database")]
        private string fileName = "Cards";
        [SerializeField] private Card cardPrefab;
        private string jsonPath;

    #endregion

    #region Unity Methods

        protected override async void Awake()
        {
            base.Awake();

            this.jsonPath = this.GetJsonPath(this.fileName);
            await this.CopyJsonFromStreamingAssets(this.fileName, this.jsonPath);
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
            // Check if file exists
            if (!File.Exists(this.jsonPath))
            {
                Logger.LogError("Json file was not found!");
                throw new FileNotFoundException();
            }

            // Read json file
            string jsonData = File.ReadAllText(this.jsonPath);
            
            // Deserialize json data into dictionary
            var cards = JsonConvert.DeserializeObject<Dictionary<string, CardData>>(jsonData);

            // Get and return value
            if (cards.TryGetValue(cardId.ToString(), out CardData cardData))
            {
                return Instantiate(this.cardPrefab).Initialize(
                    cardData.id,
                    uniqueId,
                    cardData.name,
                    cardData.cost,
                    cardData.power,
                    cardData.description,
                    cardData.actionId);
            }
            
            // Card with given id does not exist
            Logger.LogError($"Card with id {cardId} does not exist!");
            throw new KeyNotFoundException();
        }

    #endregion

    #region DatabaseManager Methods

        /// <summary>
        /// Returns the system specific path of the provided file name.
        /// Supports Unity, Windows and Android.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>dbPath</returns>
        private string GetJsonPath(string fileName)
        {
            return Application.platform switch
            {
                // Android
                RuntimePlatform.Android => $"{Application.persistentDataPath}/{fileName}.json",
                
                // IOS
                RuntimePlatform.IPhonePlayer => $"...",
                
                // Unity Editor, Windows
                _ => $"{Application.streamingAssetsPath}/{fileName}.json"
            };
        }
        
        /// <summary>
        /// Copies the provided file from the streaming asset folder to a persistent data path, in case of Android or IOS
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="targetPath"></param>
        private async Task CopyJsonFromStreamingAssets(string fileName, string targetPath)
        {
            // Return if the platform is not mobile
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) return;
            
            // Define source path from streaming assets
            string sourcePath = $"{Application.streamingAssetsPath}/{fileName}.json";
            
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
                Debug.LogError("Failed to load json file: " + www.error);
                return;
            }

            // Save the database to target path
            await File.WriteAllBytesAsync(targetPath, www.downloadHandler.data);
        }

    #endregion
}

[System.Serializable]
public struct CardData
{
    public int id;
    public string name;
    public int cost;
    public int power;
    public string description;
    public string actionId;
}