DeathTracker (v1.1.0)
DeathTracker is a Rust server plugin designed to track player deaths and manage penalties when death limits are exceeded. The plugin integrates with the Economics plugin to apply monetary penalties when players surpass a configurable number of lives. If players cannot pay the penalty, they are banned and kicked from the server until the next server wipe or they are unbanned by an admin.

Features:
Death Tracking: Monitor the number of deaths for each player.
Configurable Life Limits: Set the default number of lives each player has.
Penalties: When a player exceeds their life limit:
They are charged a configurable fee through the Economics plugin.
If they can't pay, they are banned and kicked from the server.
Ban Management: View, unban, or wipe the list of banned players via console commands.

Player Commands:
/lives: Check how many lives a player has left.
/deaths: Check how many times a player has died.

Admin Commands:
banlist: View the current list of banned players.
wipebanlist: Clear the list of banned players.
unbanplayer <playerId>: Unban a specific player.
resetdeaths: Reset death counts for all players.
Persistent Data: Player death counts and ban lists are saved across server restarts.
Economics Integration: Automatically resets death counts if Economics data is wiped.

Configuration Options:
DefaultLives: Number of lives a player has by default (default: 3).
PenaltyAmount: The amount to be deducted from a player's balance after exceeding their lives (default: 100.0).
BanMessage: The message displayed when a player is banned (default: "You have been banned for exceeding death limits.").
Installation:
Place the plugin in the oxide/plugins folder.
Configure the plugin in the oxide/config/DeathTracker.json file.
Ensure the Economics plugin is installed and properly configured.
