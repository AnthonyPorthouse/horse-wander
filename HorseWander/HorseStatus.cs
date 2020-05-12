using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace HorseWander
{
    public class HorseStatus
    {
        private IMonitor _monitor;

        private double _moveChance;
        private int _moveMin;
        private int _moveMax;

        private Horse _horse;
        private int _ticksToMove;
        private Direction _moveDirection = Direction.Up;
        private bool _isWandering;

        public HorseStatus(IMonitor monitor, ModConfig config, Horse horse)
        {
            _monitor = monitor;
            _horse = horse;

            _moveChance = config.GetWanderFrequency();
            _moveMin = config.GetWanderRange().Item1;
            _moveMax = config.GetWanderRange().Item2;
        }

        public void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (_horse.rider == null)
            {
                if (Game1.random.NextDouble() < _moveChance && !_isWandering)
                {
                    _moveDirection = (Direction)Game1.random.Next(0, 4);
                    _ticksToMove = Game1.random.Next(_moveMin, _moveMax);

                    _monitor.Log($"Moving {_horse.getName()} {_moveDirection} for {_ticksToMove}", LogLevel.Debug);
                    _horse.faceDirection((int)_moveDirection);
                    SetHorseDirection(_horse, _moveDirection);
                    _horse.Sprite.StopAnimation();
                    _horse.Sprite.ignoreStopAnimation = true;
                    _horse.Sprite.loop = true;
                    _horse.Sprite.framesPerAnimation = 7;
                    _horse.Sprite.CurrentFrame = 0;
                    _isWandering = true;
                }

                if (_ticksToMove == 0 && _isWandering)
                {
                    StopHorse();
                }

                if (_ticksToMove > 0 && _isWandering)
                {
                    if (_horse.currentLocation.isCollidingPosition(
                        _horse.nextPosition(_horse.getDirection()),
                        Game1.viewport,
                        false,
                        0,
                        false,
                        _horse
                    ))
                    {
                        _monitor.Log("Bonk! Stopping wandering.", LogLevel.Debug);

                        StopHorse();
                        return;
                    }

                    //target.faceDirection((int)moveDirection);
                    _horse.animateInFacingDirection(Game1.currentGameTime);
                    _horse.tryToMoveInDirection(_horse.getDirection(), false, 0, false);
                    if (_moveDirection == Direction.Left)
                    {
                        _horse.flip = true;
                    }
                    //target.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
                    _ticksToMove--;
                }
            }
            else
            {
                _ticksToMove = 0;
                _isWandering = false;
                _horse.Sprite.ignoreStopAnimation = false;
            }
        }

        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_isWandering)
            {
                _horse.animateInFacingDirection(Game1.currentGameTime);
            }
        }

        private void SetHorseDirection(Horse target, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    target.SetMovingOnlyUp();
                    return;
                case Direction.Right:
                    target.SetMovingOnlyRight();
                    return;
                case Direction.Down:
                    target.SetMovingOnlyDown();
                    return;
                case Direction.Left:
                    target.SetMovingOnlyLeft();
                    return;
            }
        }

        private void StopHorse()
        {
            _horse.Sprite.loop = false;
            _horse.Sprite.ignoreStopAnimation = false;
            _horse.Sprite.StopAnimation();
            _horse.Sprite.CurrentFrame = 0;
            _isWandering = false;
        }
    }
}