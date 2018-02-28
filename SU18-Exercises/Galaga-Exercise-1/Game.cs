using System;
using DIKUArcade; 

using System.IO;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

using System.Collections.Generic;
using DIKUArcade.EventBus;
using DIKUArcade.Timers;

// using DIKUArcade.Timers.GameTimer;

namespace Galaga_Exercise_1 {

    public class Game : IGameEventProcessor<object> {

        private Window win;
        private Entity player;
        private GameEventBus<object> eventBus;

        private List<Image> enemyStrides;
        private EntityContainer enemies;

        private GameTimer gameTimer;
        
        public Game() {
            // look at the Window.cs file for possible constructors.
            // We recommend using 500 × 500 as window dimensions,
            // which is most easily done using a predefined aspect ratio.
            
            win = new Window("Galaga Game", 500, 500);

            player = new Entity(
                new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
                new Image(Path.Combine("Assets", "Images", "Player.png")));

            
            enemyStrides = ImageStride.CreateStrides(4,
                Path.Combine("Assets", "Images", "BlueMonster.png"));
            enemies = new EntityContainer();
            
            AddEnemies();
            
            eventBus = new GameEventBus<object>();

            eventBus.InitializeEventBus(new List<GameEventType>() {
                GameEventType.InputEvent,
                GameEventType.WindowEvent,
                GameEventType.PlayerEvent
            });

            win.RegisterEventBus(eventBus);
            eventBus.Subscribe(GameEventType.InputEvent, this);
            eventBus.Subscribe(GameEventType.WindowEvent, this);

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
                    player.Shape.Move();
                    
                }

                if (gameTimer.ShouldRender()) {
                    win.Clear();
                    // render game entities
                    player.RenderEntity();
                    enemies.RenderEntities();
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
                 if (!EntityOutOfBounds(player, "EDGE_LEFT")) {
                     player.Shape.MoveX(-0.010f);
                 }
                 break;
                 
              case "KEY_D":
                  if (!EntityOutOfBounds(player, "EDGE_RIGHT")) {
                      player.Shape.MoveX(0.010f);
                  }

                  break;
                  
              case "KEY_W":
                  if (!EntityOutOfBounds(player, "EDGE_TOP")) {
                      player.Shape.MoveY(0.010f);
                  }

                  break;
                  
              case "KEY_S":
                  if (!EntityOutOfBounds(player, "EDGE_BOTTOM")) {
                      player.Shape.MoveY(-0.010f);
                  }

                  break;

            }
        }

        public void KeyRelease(string key) {
            // match on e.g. "KEY_UP", "KEY_1", "KEY_A", etc.
            player.Shape.MoveX(0.0f);
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