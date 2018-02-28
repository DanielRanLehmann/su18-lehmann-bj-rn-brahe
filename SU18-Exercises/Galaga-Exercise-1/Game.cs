using System;
using DIKUArcade; 

using System.IO;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

using System.Collections.Generic;
using DIKUArcade.EventBus;



namespace Galaga_Exercise_1 {

    public class Game : IGameEventProcessor<object> {

        private Window win;
        private Entity player;
        private GameEventBus<object> eventBus;

        private uint winWidth;
        private uint winHeight;
        

        public Game() {
            // look at the Window.cs file for possible constructors.
            // We recommend using 500 × 500 as window dimensions,
            // which is most easily done using a predefined aspect ratio.

            this.winWidth = 500;
            this.winHeight = 500;
            
            win = new Window("Galaga Game", winWidth, winHeight);

            player = new Entity(
                new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
                new Image(Path.Combine("Assets", "Images", "Player.png")));


            eventBus = new GameEventBus<object>();

            eventBus.InitializeEventBus(new List<GameEventType>() {
                GameEventType.InputEvent,
                GameEventType.WindowEvent,
                GameEventType.PlayerEvent
            });

            win.RegisterEventBus(eventBus);
            eventBus.Subscribe(GameEventType.InputEvent, this);
            eventBus.Subscribe(GameEventType.WindowEvent, this);

        }

        public void GameLoop() {
            while (win.IsRunning()) {
                
                eventBus.ProcessEvents(); // this will call ProcessEvent()
                
                win.PollEvents();
                win.Clear();
                
                player.Shape.Move();
                player.RenderEntity();
                
                
                
                win.SwapBuffers();

            }
        }

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

            // match on e.g. "KEY_UP", "KEY_1", "KEY_A", etc.
            // TODO: use this method to start moving your player object
            // player.Shape.MoveX(0.0001f); // choose a fittingly small number
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