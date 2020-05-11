using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace HelloWorld
{
    public enum Direction
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public class HorseStatus
    {
        private Horse horse;
        private int ticksToMove = 0;
        private Direction moveDirection = Direction.Up;
        private int nextMove = 1;
        private bool isWandering = false;
        private IMonitor monitor;

        public HorseStatus(IMonitor monitor, Horse horse)
        {
            this.monitor = monitor;
            this.horse = horse;
        }

        public void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (horse.rider == null)
            {
                if (e.Ticks % nextMove == 0 && !isWandering)
                {
                    moveDirection = (Direction)Game1.random.Next(0, 4);
                    ticksToMove = Game1.random.Next(120, 300);

                    var path = new List<Vector2>();

                    var curpos = (horse.getStandingPosition());

                    monitor.Log($"Moving {horse.getName()} {(Direction)moveDirection} for {ticksToMove}", LogLevel.Debug);
                    horse.faceDirection((int)moveDirection);
                    SetHorseDirection(horse, moveDirection);
                    horse.Sprite.StopAnimation();
                    horse.animateInFacingDirection(Game1.currentGameTime);
                    horse.Sprite.ignoreStopAnimation = true;
                    isWandering = true;
                }

                if (ticksToMove == 0 && isWandering)
                {
                    horse.Sprite.ignoreStopAnimation = false;
                    horse.Sprite.StopAnimation();
                    nextMove = Game1.random.Next(300, 3001);
                    isWandering = false;
                }

                if (ticksToMove > 0 && isWandering)
                {
                    horse.faceDirection((int)moveDirection);
                    horse.animateInFacingDirection(Game1.currentGameTime);
                    horse.tryToMoveInDirection((int)moveDirection, false, 0, false);
                    //horse.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
                    ticksToMove--;
                }
            }
            else
            {
                ticksToMove = 0;
                isWandering = false;
                horse.Sprite.ignoreStopAnimation = false;
            }
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (isWandering)
            {
                horse.faceDirection((int)moveDirection);
            }
        }

        private void SetHorseDirection(Horse horse, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    horse.SetMovingOnlyUp();
                    return;
                case Direction.Right:
                    horse.SetMovingOnlyRight();
                    return;
                case Direction.Down:
                    horse.SetMovingOnlyDown();
                    return;
                case Direction.Left:
                    horse.SetMovingOnlyLeft();
                    return;
            }
        }

    }

    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private Dictionary<Guid, HorseStatus> horses = new Dictionary<Guid, HorseStatus>();

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }



        /*********
        ** Private methods
        *********/

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
            {
                return;
            }

            if(e.IsMultipleOf(600))
            {
                foreach (Horse horse in FindHorses())
                {
                    if (!horses.ContainsKey(horse.HorseId))
                    {
                        this.Monitor.Log($"Found Horse: {horse.getName()}", LogLevel.Debug);

                        horses.Add(horse.HorseId, new HorseStatus(Monitor, horse));
                    }                
                }
            }

            foreach (KeyValuePair<Guid, HorseStatus> kvp in horses)
            {
                kvp.Value.OnUpdateTicking(sender, e);
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            foreach (KeyValuePair<Guid, HorseStatus> kvp in horses)
            {
                kvp.Value.OnUpdateTicked(sender, e);
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