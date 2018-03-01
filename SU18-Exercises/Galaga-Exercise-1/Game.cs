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

// using DIKUArcade.Timers.GameTimer;

namespace Galaga_Exercise_1 {

    public class Game : IGameEventProcessor<object> {

        private Window win;
        
        // private Entity player;
        private Player player;
        
        private GameEventBus<object> eventBus;

        private List<Image> enemyStrides;
        private EntityContainer enemies;
        
        // private List<Image> projectileStrides;
        private Image imageShot;
        private EntityContainer playerShots;

        private GameTimer gameTimer;
        
        public Game() {
            // look at the Window.cs file for possible constructors.
            // We recommend using 500 × 500 as window dimensions,
            // which is most easily done using a predefined aspect ratio.
            
            win = new Window("Galaga Game", 500, 500);
            
            /*
            player = new Entity(
                new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
                new Image(Path.Combine("Assets", "Images", "Player.png")));
            */
            
            enemyStrides = ImageStride.CreateStrides(4,
                Path.Combine("Assets", "Images", "BlueMonster.png"));
           
            enemies = new EntityContainer();
           
            // projectileStrides = ImageStride.CreateStrides(1, Path.Combine("Assets", "Images", "BulletRed2.png"));
            imageShot = new Image(Path.Combine("Assets", "Images", "BulletRed2.png"));
            playerShots = new EntityContainer();
           
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
            // create the desired number of enemies here. Remember:
            //   - normalised coordinates
            //   - add them to the entity container
                         
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.10f, 0.1f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.30f, 0.2f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.90f, 0.3f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            enemies.AddDynamicEntity(new DynamicShape(new Vec2F(0.05f, 0.6f), new Vec2F(0.1f, 0.1f)), new ImageStride(80, enemyStrides));
            
        }

        public void GameLoop() {
            while (win.IsRunning()) {
                
                gameTimer.MeasureTime();

                while (gameTimer.ShouldUpdate()) {
                    win.PollEvents();
                    eventBus.ProcessEvents();
                    
                    // game logic
                    // player.Shape.Move();
                    player.entity.Shape.Move();
                }

                if (gameTimer.ShouldRender()) {
                    win.Clear();
                    // render game entities
                    // player.RenderEntity();
                    player.entity.RenderEntity();
                    enemies.RenderEntities();
                    playerShots.RenderEntities(); // sure it's the right place to put it?
                    
                    foreach (Entity playerShot in playerShots) {
                        playerShot.Shape.MoveY(0.010f);
                    }
                    
                    // IterateShots();
                    playerShots.Iterate(IterateShots);
                    
                    win.SwapBuffers();
                }
                
                if (gameTimer.ShouldReset()) {
                    // 1 second has passed - display last captured ups and fps
                    win.Title = "Galaga | UPS: " + gameTimer.CapturedUpdates +
                                ", FPS: " + gameTimer.CapturedFrames;
                }

               
                /*
                eventBus.ProcessEvents(); // this will call ProcessEvent()
                    
                orphane code
                win.PollEvents();
                win.Clear();
                
                player.Shape.Move();
                player.RenderEntity();
                enemies.RenderEntities();
                
                win.SwapBuffers();
                */

            }
        }
           
        // helper method
        public bool EntityOutOfBounds(Entity e, string edgeKey) {
            
            // defensive programming goes here. check for nullability, or cornerKey specification?
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
                 if (!EntityOutOfBounds(player.entity, "EDGE_LEFT")) {
                     player.entity.Shape.MoveX(-0.010f);
                 }
                 break;
                 
              case "KEY_D":
                  if (!EntityOutOfBounds(player.entity, "EDGE_RIGHT")) {
                      player.entity.Shape.MoveX(0.010f);
                  }

                  break;
                  
              case "KEY_W":
                  if (!EntityOutOfBounds(player.entity, "EDGE_TOP")) {
                      player.entity.Shape.MoveY(0.010f);
                  }

                  break;
                  
              case "KEY_S":
                  if (!EntityOutOfBounds(player.entity, "EDGE_BOTTOM")) {
                      player.entity.Shape.MoveY(-0.010f);
                  }

                  break;
                  
              case "KEY_SPACE":

                  DynamicShape shape = new DynamicShape(new Vec2F(
                          player.entity.Shape.Position.X + (player.entity.Shape.Extent.X / 2.0f),
                          player.entity.Shape.Position.Y + player.entity.Shape.Extent.Y),
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
                
            } else {
               
                foreach (Entity enemy in enemies) {
                        
                    CollisionData collisionData = CollisionDetection.Aabb((DynamicShape)shot.Shape, enemy.Shape);
                    if (collisionData.Collision) {
                        
                        shot.DeleteEntity();
                        
                        enemy.DeleteEntity();
                        enemies.Iterate(IterateEnemies);
                        
                    }
                }
            }

            // if none of the cases above, do this.
            if (!outofBounds && !didCollide) {
                shot.Shape.Move();    
            }
            
        }

        // what the fuck is going on here, but hey it works!
        public void IterateEnemies(Entity entity) {
            if (entity.IsDeleted())
            {
                entity.DeleteEntity();    
            }
            
        }

        public void KeyRelease(string key) {
            // match on e.g. "KEY_UP", "KEY_1", "KEY_A", etc.
            player.entity.Shape.MoveX(0.0f);
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