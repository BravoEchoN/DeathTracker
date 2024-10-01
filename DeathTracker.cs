using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("DeathTracker", "BravoEchoN", "1.1.0")]
    [Description("Tracks player deaths, allows for death/life checking, and manages bans and economic penalties.")]

    public class DeathTracker : CovalencePlugin
    {
        [PluginReference]
        private Plugin Economics;

        private ConfigData configData;
        private Dictionary<string, int> deathCounts = new Dictionary<string, int>();
        private List<string> bannedPlayers = new List<string>();

        private void Init()
        {
            permission.RegisterPermission("deathtracker.morelives", this);
            LoadConfigData();
            LoadData();
        }

        private void OnServerSave() => SaveData();
        private void OnServerShutdown() => SaveData();
        private void Unload() => SaveData();

        // Handles player death event
        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (!(entity is IPlayer player)) return;

            var playerId = player.Id;
            if (!deathCounts.ContainsKey(playerId))
                deathCounts[playerId] = 0;

            deathCounts[playerId]++;

            if (deathCounts[playerId] > configData.DefaultLives)
            {
                if (Economics.Call<bool>("Withdraw", playerId, configData.PenaltyAmount))
                {
                    player.Message($"You have been penalized {configData.PenaltyAmount} for exceeding death limits.");
                }
                else
                {
                    bannedPlayers.Add(playerId);
                    player.Ban(configData.BanMessage);
                    player.Kick(configData.BanMessage);
                    player.Message(configData.BanMessage);
                }
            }

            SaveData();
        }

        // Player command to check lives left
        [ChatCommand("lives")]
        private void CmdLives(IPlayer player, string command, string[] args)
        {
            var playerId = player.Id;
            int livesLeft = configData.DefaultLives - (deathCounts.ContainsKey(playerId) ? deathCounts[playerId] : 0);
            player.Message($"You have {livesLeft} lives left.");
        }

        // Player command to check current death count
        [ChatCommand("deaths")]
        private void CmdDeaths(IPlayer player, string command, string[] args)
        {
            var playerId = player.Id;
            int deaths = deathCounts.ContainsKey(playerId) ? deathCounts[playerId] : 0;
            player.Message($"You have died {deaths} times.");
        }

        // Admin command to view the ban list
        [ConsoleCommand("banlist")]
        private void CmdBanList(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) return;
            if (bannedPlayers.Count == 0)
            {
                Puts("No players are currently banned.");
            }
            else
            {
                Puts("Banned players:");
                foreach (var playerId in bannedPlayers)
                {
                    Puts(playerId);
                }
            }
        }

        // Admin command to wipe the ban list
        [ConsoleCommand("wipebanlist")]
        private void CmdWipeBanList(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) return;
            bannedPlayers.Clear();
            Puts("Ban list has been wiped.");
        }

        // Admin command to unban a specific player
        [ConsoleCommand("unbanplayer")]
        private void CmdUnbanPlayer(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) return;

            if (arg.Args.Length < 1)
            {
                Puts("Usage: unbanplayer <playerId>");
                return;
            }

            var playerId = arg.Args[0];
            if (bannedPlayers.Contains(playerId))
            {
                bannedPlayers.Remove(playerId);
                Puts($"Player {playerId} has been unbanned.");
            }
            else
            {
                Puts($"Player {playerId} is not banned.");
            }
        }

        // Ensure bans persist and players can't reconnect
        private object CanUserLogin(string name, string id)
        {
            if (bannedPlayers.Contains(id))
            {
                return configData.BanMessage;
            }
            return null;
        }

        // Config and data management
        protected override void LoadDefaultConfig() => configData = new ConfigData();

        private void LoadConfigData()
        {
            configData = Config.ReadObject<ConfigData>();
            Config.WriteObject(configData, true);
        }

        private void LoadData()
        {
            deathCounts = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, int>>(Name);
            bannedPlayers = Interface.Oxide.DataFileSystem.ReadObject<List<string>>("DeathTrackerBannedPlayers");
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(Name, deathCounts);
            Interface.Oxide.DataFileSystem.WriteObject("DeathTrackerBannedPlayers", bannedPlayers);
        }

        // Config structure
        private class ConfigData
        {
            public int DefaultLives { get; set; } = 3;
            public double PenaltyAmount { get; set; } = 100.0;
            public string BanMessage { get; set; } = "You have been banned for exceeding death limits.";
        }

        [ConsoleCommand("resetdeaths")]
        private void CmdResetDeaths(ConsoleSystem.Arg arg)
        {
            if (arg.Connection != null && arg.Connection.authLevel < 2) return;
            deathCounts.Clear();
            Puts("Death counts have been reset for all players.");
        }

        [HookMethod("OnEconomicsDataWiped")]
        private void OnEconomicsDataWiped()
        {
            deathCounts.Clear();
            Puts("Death counts reset due to economics data wipe.");
        }
    }
}
