using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace HorseWander
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private Dictionary<Guid, HorseStatus> horses = new Dictionary<Guid, HorseStatus>();
        private ModConfig config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }



        /*********
        ** Private methods
        *********/

        /// <summary>
        /// When the loaded game is quit, we should remove all registered horses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Monitor.Log("Clearing horse list", LogLevel.Debug);
            horses.Clear();
        }

        /// <summary>
        /// When the day first starts, we should register any horses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Do not register any horses if we're not the main player
            // this should stop horses from being affected by multiple players
            // with this mod installed.
            if (!Context.IsMainPlayer)
            {
                this.Monitor.Log("Not main player. Disabling", LogLevel.Debug);
                return;
            }

            GetHorses();
        }

        /// <summary>
        /// On each update, if the world is loaded, we should run our logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
            {
                return;
            }

            foreach (KeyValuePair<Guid, HorseStatus> kvp in horses)
            {
                kvp.Value.OnUpdateTicking(sender, e);
            }
        }

        /// <summary>
        /// Ensure our state is still good, post update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            foreach (KeyValuePair<Guid, HorseStatus> kvp in horses)
            {
                kvp.Value.OnUpdateTicked(sender, e);
            }
        }


        /// <summary>
        /// Populate our list of horses in the current game
        /// </summary>
        private void GetHorses()
        {
            foreach (Horse horse in FindHorses())
            {
                if (!horses.ContainsKey(horse.HorseId))
                {
                    this.Monitor.Log($"Found Horse: {horse.getName()}", LogLevel.Debug);

                    horses.Add(horse.HorseId, new HorseStatus(Monitor, config, horse));
                }
            }
        }

        private IEnumerable<Horse> FindHorses()
        {
            foreach (GameLocation location in GetLocations())
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                {
                    yield return horse;
                }
            }
        }

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations)
            {
                yield return location;
            }
        }
    }
}