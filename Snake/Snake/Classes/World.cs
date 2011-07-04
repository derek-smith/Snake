using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Snake.Classes {
    class World {
        const int BLOCK_SIZE = 16;

        int gameWidth, gameHeight;
        Vector2 drawVector = Vector2.Zero;

        const int UP = 1;
        const int DOWN = 2;
        const int LEFT = 3;
        const int RIGHT = 4;
        Queue<int> directions = new Queue<int>();
        int direction = World.RIGHT;

        Queue<Vector2> snake;
        Vector2 prevHead;
        List<Vector2> food;

        bool isGrowing;
        int growCount;

        int moveTimer = 0;
        // Milliseconds
        int moveTimeout = 200;

        Texture2D snakeTexture;
        Texture2D foodTexture;
        SpriteFont font;
        Color backgroundColor = new Color(102, 153, 102);

        bool keyUpEnabled = true;
        bool keyDownEnabled = true;
        bool keyLeftEnabled = true;
        bool keyRightEnabled = true;

        Random randomGenerator = new Random(DateTime.Now.Millisecond);

        public World(ContentManager content, int width, int height) {
            this.snakeTexture = content.Load<Texture2D>(@"Graphics\snake-piece");
            this.foodTexture = content.Load<Texture2D>(@"Graphics\snake-food");

            this.gameWidth = width;
            this.gameHeight = height;

            this.snake = new Queue<Vector2>();
            this.snake.Enqueue(new Vector2(0, 0));
            this.prevHead = new Vector2(this.snake.Peek().X, this.snake.Peek().Y);
            this.food = new List<Vector2>();

            this.AddFood();
            this.Grow(3);
        }

        public void Update(KeyboardState keyState, int elapsed) {
            // Check for reset (probably could be improved - probably wouldn't matter)
            if (keyState.IsKeyUp(Keys.Up)) keyUpEnabled = true;
            if (keyState.IsKeyUp(Keys.Down)) keyDownEnabled = true;
            if (keyState.IsKeyUp(Keys.Left)) keyLeftEnabled = true;
            if (keyState.IsKeyUp(Keys.Right)) keyRightEnabled = true;

            // Should the snake's direction change?
            switch (direction) {
                case World.UP:
                case World.DOWN:
                    if (keyState.IsKeyDown(Keys.Right) && keyRightEnabled) {
                        keyRightEnabled = false;
                        directions.Enqueue(World.RIGHT);
                    }
                    else if (keyState.IsKeyDown(Keys.Left) && keyLeftEnabled) {
                        keyLeftEnabled = false;
                        directions.Enqueue(World.LEFT);
                    }
                    break;

                case World.LEFT:
                case World.RIGHT:
                    if (keyState.IsKeyDown(Keys.Up) && keyUpEnabled) {
                        keyUpEnabled = false;
                        directions.Enqueue(World.UP);
                    }
                    else if (keyState.IsKeyDown(Keys.Down) && keyDownEnabled) {
                        keyDownEnabled = false;
                        directions.Enqueue(World.DOWN);
                    }
                    break;
            }

            // Update elapsed time (since last update)
            this.moveTimer += elapsed;
            // Is it time to move?
            if (this.moveTimer >= this.moveTimeout) {
                // Reset
                this.moveTimer = 0;
                Vector2 snakeHead = new Vector2(this.prevHead.X, this.prevHead.Y);

                if (this.directions.Count > 0) this.direction = this.directions.Dequeue();

                switch (this.direction) {
                    case World.UP:
                        snakeHead.Y -= 1;
                        if (snakeHead.Y == -1) snakeHead.Y = gameHeight - 1;
                        break;
                    case World.DOWN:
                        snakeHead.Y += 1;
                        if (snakeHead.Y == gameHeight) snakeHead.Y = 0;
                        break;
                    case World.LEFT:
                        snakeHead.X -= 1;
                        if (snakeHead.X == -1) snakeHead.X = gameWidth - 1;
                        break;
                    case World.RIGHT:
                        snakeHead.X += 1;
                        if (snakeHead.X == gameWidth) snakeHead.X = 0;
                        break;
                }
                // Save updated copy of snakeHead for next update
                this.prevHead.X = snakeHead.X;
                this.prevHead.Y = snakeHead.Y;

                this.snake.Enqueue(snakeHead);

                if (this.isGrowing) {
                    this.growCount--;

                    if (this.growCount == 0)
                        this.isGrowing = false;
                }
                else {
                    this.snake.Dequeue();
                }

                // Check for collision with food
                Vector2 block = Vector2.Zero;
                bool removeFood = false;
                foreach (Vector2 foodBlock in food) {
                    if (foodBlock.X == snakeHead.X && foodBlock.Y == snakeHead.Y) {
                        block = foodBlock;
                        removeFood = true;
                        break;
                    }
                }
                if (removeFood) {
                    food.Remove(block);
                    AddFood();
                    Grow(3);
                    this.moveTimeout -= 25;
                }
                
            }
        }

        public void Draw(GraphicsDevice graphics, SpriteBatch batch) {
            graphics.Clear(backgroundColor);
            batch.Begin();

            foreach (Vector2 snakeBlock in this.snake) {
                batch.Draw(this.snakeTexture, snakeBlock * BLOCK_SIZE, Color.White);
            }

            foreach (Vector2 foodBlock in this.food) {
                batch.Draw(this.foodTexture, foodBlock * BLOCK_SIZE, Color.White);
            }

            batch.End();
        }

        private void Grow(int n) {
            this.isGrowing = true;
            this.growCount = n;
        }

        private void AddFood() {
            while (true) {
                int randomX = randomGenerator.Next(gameWidth);
                int randomY = randomGenerator.Next(gameHeight);

                foreach (Vector2 snakeBlock in this.snake) {
                    if ((int)snakeBlock.X == randomX && (int)snakeBlock.Y == randomY) continue;
                }

                food.Add(new Vector2(randomX, randomY));
                return;
            }
        }
    }
}
