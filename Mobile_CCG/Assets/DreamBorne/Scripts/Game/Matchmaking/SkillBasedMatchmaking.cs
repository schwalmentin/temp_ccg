using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;

public class SkillBasedMatchmaking
{

    // Glicko-2 coonstants
    private const double Tau = 0.5;
    private const double PhiPrime = 173.7178;

    public const double winOutcome = 1.0;
    public const double lossOutcome = 0.0;
    public const double drawOutcome = 0.5;

    // Player structure
    public class Player
    {
        public double Rating { get; set; }
        public double RD { get; set; }
        public double Volatility { get; set; }
        public Rank Rank { get; set; }

        // Player Initialization using default Glicko-2 values: http://www.glicko.net/glicko/glicko2.pdf

        public Player()
        {
            Rating = 1500;
            RD = 350;
            Volatility = 0.06;
            Rank = Rank.GOLD;
        }
    }


    public double getWinOutcome()
    {
        return winOutcome;
    }

    public double getLossOutcome()
    {
        return winOutcome;
    }

    public double getDrawOutcome()
    {
        return winOutcome;
    }

    public async Task UpdateRatingAsync(string playerId, string enemyId, double outcome)
    {
        Player player = await LoadData(playerId);

        Player enemy = await LoadData(enemyId);

        double ratingDifference = enemy.Rating - player.Rating;
        double RDCombined = Math.Sqrt(player.RD * player.RD + enemy.RD * enemy.RD);

        double expectedMatchOutcome = 1 / (1 + Math.Pow(10, -ratingDifference / (PhiPrime * RDCombined)));

        // Calculate new rating
        double g = 1 / Math.Sqrt(1 + 3 * Math.Pow(enemy.RD / PhiPrime, 2));
        double E = 1 / (1 + Math.Exp(-g * (outcome - expectedMatchOutcome)));

        double newRD = 1 / Math.Sqrt(1.0 / (player.RD * player.RD) + 1.0 / (PhiPrime * PhiPrime));
        double newVolatility = Math.Sqrt(1 / (1 / player.Volatility / player.Volatility + 1 / PhiPrime / PhiPrime));

        // Player update
        player.Rating += PhiPrime * PhiPrime * g * (outcome - E);
        player.RD = newRD;
        player.Volatility = newVolatility;

        //Clamp ratings to get reasonable values

        player.Rating = Mathf.Clamp((float)player.Rating, 0.0f, 3000.0f);
        player.RD = Mathf.Clamp((float)player.RD, 30.0f, 350.0f);
        player.Volatility = Mathf.Clamp((float)player.Volatility, 0.005f, 0.1f);
        player.Rank = GetRank(player.Rating);


    }

    private async void SaveData(string playerId, Player player)
    {
        var playerData = new Dictionary<string, object>()
        {
            {playerId.ToString(), player }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log("Saved playerId " + playerId + " data: " + playerData);

    }

    public async Task<Player> LoadData(string playerId)
    {
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { playerId.ToString() });

            if (playerData.TryGetValue(playerId.ToString(), out var loadedPlayerObject) && loadedPlayerObject is Item item)
            {
                if (item.Value is Player loadedPlayer)
                {
                    Debug.Log(loadedPlayer);
                    return loadedPlayer;
                }

            }
        } catch (Exception ex) {
            Debug.Log(ex.ToString());
        }

        return new Player();
    }

    public static Rank GetRank(double rating)
    {
        return rating switch
        {
            double r when r >= 0 && r < 1000 => Rank.COPPER,
            double r when r >= 1000 && r < 1500 => Rank.SILVER,
            double r when r >= 1500 && r < 2000 => Rank.GOLD,
            double r when r >= 2000 && r < 2500 => Rank.PLATINUM,
            double r when r >= 2500 && r < 3000 => Rank.DIAMOND,
            double r when r >= 3000 && r < 3500 => Rank.CHALLENGER,
            double r when r >= 3500 && r < 4000 => Rank.CHAMPION,
            double r when r >= 4000 => Rank.LEGEND,
            _ => throw new ArgumentOutOfRangeException(nameof(rating), "Invalid rank value"),
        };
    }

}

public enum Rank{
    COPPER,
    SILVER,
    GOLD,
    PLATINUM,
    DIAMOND,
    CHALLENGER,
    CHAMPION,
    LEGEND
}