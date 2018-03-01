using System;
using DIKUArcade; 

using System.IO;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

using System.Collections.Generic;
using DIKUArcade.EventBus;
using DIKUArcade.Physics;
using DIKUArcade.Timers;

namespace Galaga_Exercise_1 {

    public class Game : IGameEventProcessor<object> {

        private Window win;
        
        private Player player;
        
        private GameEventBus<object> eventBus;

        private List<Image> enemyStrides;
        private EntityContainer enemies;
        
        private Image imageShot;
        private EntityContainer playerShots;
        
        private List<Image> explosionStrides;
        private AnimationContainer explosions;
        
        private int explosionLength = 500;
        
        private GameTimer gameTimer;
        
        public Game() {
            
            win = new Window("Galaga Game", 500, 500);
            
            enemyStrides = ImageStride.CreateStrides(4,
                Path.Combine("Assets", "Images", "BlueMonster.png"));
           
            enemies = new EntityContainer();
          
            imageShot = new Image(Path.Combine("Assets", "Images", "BulletRed2.png"));
            playerShots = new EntityContainer();
            
            explosionStrides = ImageStride.CreateStrides(8,
                Path.Combine("Assets", "Images", "Explosion.png"));
            
            explosions = new AnimationContainer(explosionLength);
            
           
            eventBus = new GameEventBus<object>();

            eventBus.InitializeEventBus(new List<GameEventType>() {
                GameEventType.InputEvent,
                GameEventType.WindowEvent,
                GameEventType.PlayerEvent
            });

            
            player = new Player();
            AddEnemies();
            
            win.RegisterEventBus(eventBus);
            eventBus.Subscribe(GameEventType.InputEvent, this);
            eventBus.Subscribe(GameEventType.WindowEvent, this);
            eventBus.Subscribe(GameEventType.PlayerEvent, player);

            gameTimer = new GameTimer(60, 60);

        }
        
        private void AddEnemies() {
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.10f, 0.1f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.30f, 0.2f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.90f, 0.3f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.05f, 0.6f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            
        }
        
        public void AddExplosion(float posX, float posY,
            float extentX, float extentY) {
            explosions.AddAnimation(
                new StationaryShape(posX, posY, extentX, extentY), explosionLength,
                new ImageStride(explosionLength / 8, explosionStrides));
        }

        public void GameLoop() {
            while (win.IsRunning()) {
                
                gameTimer.MeasureTime();

                while (gameTimer.ShouldUpdate()) {
                    win.PollEvents();
                    eventBus.ProcessEvents();
                    
                    player.Entity.Shape.Move();
                }

                if (gameTimer.ShouldRender()) {
                    win.Clear();
                    
                    player.Entity.RenderEntity();
                    enemies.RenderEntities();
                    playerShots.RenderEntities(); 
                    
                    explosions.RenderAnimations();
                    
                    foreach (Entity playerShot in playerShots) {
                        playerShot.Shape.MoveY(0.010f);
                    }
                   
                    playerShots.Iterate(IterateShots);
                    
                    win.SwapBuffers();
                }
                
                if (gameTimer.ShouldReset()) {
                    win.Title = "Galaga | UPS: " + gameTimer.CapturedUpdates +
                                ", FPS: " + gameTimer.CapturedFrames;
                }
            }
        }
           
        // helper method
        public bool EntityOutOfBounds(Entity e, string edgeKey) {
            if (edgeKey.Length == 0) {
                throw new ArgumentOutOfRangeException($"Undefined or unknown edgeKey {edgeKey}");
            }

            bool _outOfBounds;
                
            switch (edgeKey) {
                
                case "EDGE_LEFT":
                    _outOfBounds = (e.Shape.Position.X <= 0.0f + e.Shape.Extent.X);
                    break;
                    
                case "EDGE_RIGHT":
                    _outOfBounds = (e.Shape.Position.X >= 1.0f - e.Shape.Extent.X);
                    break;
                    
                case "EDGE_TOP":
                    _outOfBounds = (e.Shape.Position.Y >= 1.0f - e.Shape.Extent.Y);
                    break;
                    
                case "EDGE_BOTTOM":
                    _outOfBounds = (e.Shape.Position.Y <= 0.0f + e.Shape.Extent.Y);
                    break;
                    
                default:
                    _outOfBounds = false;
                    break;
            }
    
            return _outOfBounds;
        }

        public void KeyPress(string key) {
            switch (key) {
            case "KEY_ESCAPE":
                eventBus.RegisterEvent(
                    GameEventFactory<object>.CreateGameEventForAllProcessors(
                        GameEventType.WindowEvent, this, "CLOSE_WINDOW", "", ""));
                break;
                
             case "KEY_A":
                 if (!EntityOutOfBounds(player.Entity, "EDGE_LEFT")) {
                     player.Entity.Shape.MoveX(-0.010f);
                 }
                 break;
                 
              case "KEY_D":
                  if (!EntityOutOfBounds(player.Entity, "EDGE_RIGHT")) {
                      player.Entity.Shape.MoveX(0.010f);
                  }

                  break;
                  
              case "KEY_W":
                  if (!EntityOutOfBounds(player.Entity, "EDGE_TOP")) {
                      player.Entity.Shape.MoveY(0.010f);
                  }

                  break;
                  
              case "KEY_S":
                  if (!EntityOutOfBounds(player.Entity, "EDGE_BOTTOM")) {
                      player.Entity.Shape.MoveY(-0.010f);
                  }

                  break;
                  
              case "KEY_SPACE":

                  DynamicShape shape = new DynamicShape(new Vec2F(
                          player.Entity.Shape.Position.X + (player.Entity.Shape.Extent.X / 2.0f),
                          player.Entity.Shape.Position.Y + player.Entity.Shape.Extent.Y),
                      new Vec2F(0.008f, 0.027f), new Vec2F(0.0f, 0.01f));
                  
                  playerShots.AddDynamicEntity(
                      shape,
                      imageShot);
                  
                  
                  break;
            }
        }
        
        public void IterateShots(Entity entity) {
            Entity shot = entity;
            
            bool outofBounds = false;
            bool didCollide = false;
            
            if (EntityOutOfBounds(shot, "EDGE_TOP")) {
                shot.DeleteEntity();
                outofBounds = true;

            } else {
               
                foreach (Entity enemy in enemies) {
                        
                    CollisionData collisionData = CollisionDetection.Aabb((DynamicShape)shot.Shape, enemy.Shape);
                    if (collisionData.Collision) {
                        
                        AddExplosion(enemy.Shape.Position.X, enemy.Shape.Position.Y,
                            enemy.Shape.Extent.X, enemy.Shape.Extent.Y);
                        
                        shot.DeleteEntity();
                        
                        enemy.DeleteEntity();
                        enemies.Iterate(IterateEnemies);

                        
                        didCollide = true;
                        break;
                    }
                }
            }

            if (!outofBounds && !didCollide) {
                shot.Shape.Move();    
            }
            
        }

        public void IterateEnemies(Entity entity) {
            if (entity.IsDeleted())
            {
                entity.DeleteEntity();    
            }
            
        }

        public void KeyRelease(string key) {
            player.Entity.Shape.MoveX(0.0f);
        }

        public void ProcessEvent(GameEventType eventType, GameEvent<object> gameEvent) {
            if (eventType == GameEventType.WindowEvent) {
                switch (gameEvent.Message) {
                case "CLOSE_WINDOW":
                    win.CloseWindow();
                    break;
                default:
                    break;
                }
            } else {
                switch (gameEvent.Parameter1) {
                case "KEY_PRESS":
                    KeyPress(gameEvent.Message);
                    break;

                case "KEY_RELEASE":
                    KeyRelease(gameEvent.Message);
                    break;
                }
            }
        }
    }
}