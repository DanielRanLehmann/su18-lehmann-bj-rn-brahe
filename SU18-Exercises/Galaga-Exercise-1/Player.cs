using System;
using DIKUArcade; 

using System.IO;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Math;

using DIKUArcade.EventBus;

namespace Galaga_Exercise_1 {
    public class Player: IGameEventProcessor<object> {

        public Entity entity;
        
        public Player() {
            entity = new Entity(
                new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
                new Image(Path.Combine("Assets", "Images", "Player.png"))); 
            
        }
        public void ProcessEvent(GameEventType eventType, GameEvent<object> gameEvent) {
            if (eventType == GameEventType.PlayerEvent) {
                
                // do something fancy here.
            } 
        }
        
        
    }
}