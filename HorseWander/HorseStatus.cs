using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace HelloWorld
{
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
}