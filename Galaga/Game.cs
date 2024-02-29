using System.IO;
using DIKUArcade;
using DIKUArcade.GUI;
using DIKUArcade.Math;
using DIKUArcade.Input;
using DIKUArcade.Events;
using DIKUArcade.Entities;
using DIKUArcade.Graphics;
using DIKUArcade.Physics;
using System.Collections.Generic;

namespace Galaga;
public class Game : DIKUGame, IGameEventProcessor {
    private Player player;
    private GameEventBus eventBus;
    private EntityContainer<Enemy> enemies;
    private EntityContainer<PlayerShot> playerShots;
    private IBaseImage playerShotImage;
    private AnimationContainer enemyExplosions;
    private List<Image> explosionStrides;
    private const int EXPLOSION_LENGTH_MS = 500;
    public Game(WindowArgs windowArgs) : base(windowArgs) {
        player = new Player(
            new DynamicShape(new Vec2F(0.45f, 0.1f), new Vec2F(0.1f, 0.1f)),
            new Image(Path.Combine("Assets", "Images", "Player.png")));
        eventBus = new GameEventBus();
        eventBus.InitializeEventBus(new List<GameEventType> { GameEventType.InputEvent });
        window.SetKeyEventHandler(KeyHandler);
        eventBus.Subscribe(GameEventType.InputEvent, this);

        List<Image> images = ImageStride.CreateStrides (
            4, Path.Combine("Assets", "Images", "BlueMonster.png"));
        const int numEnemies = 8; 
        enemies = new EntityContainer<Enemy>(numEnemies); 
        for (int i = 0; i < numEnemies; i++) {
            enemies.AddEntity(new Enemy(
                new DynamicShape(new Vec2F(0.1f + (float)i * 0.1f, 0.9f), new Vec2F(0.1f, 0.1f)),
                new ImageStride(80, images)));
            }
        
        playerShots = new EntityContainer<PlayerShot>();
        playerShotImage = new Image(Path.Combine("Assets", "Images", "BulletRed2.png"));
        
        enemyExplosions = new AnimationContainer(numEnemies);
        explosionStrides = ImageStride.CreateStrides(8,
        Path.Combine("Assets", "Images", "Explosion.png"));
        // TODO: Set key event handler (inherited window field of DIKUGame class)

    }

    public override void Render() {
        player.Render();
        enemies.RenderEntities();
        playerShots.RenderEntities();
        enemyExplosions.RenderAnimations();
    }

    public override void Update() {
        eventBus.ProcessEventsSequentially();
        player.Move();
        IterateShots();
    }

    private void KeyPress(KeyboardKey key) {
        switch (key) {
            case KeyboardKey.Escape:
                window.CloseWindow();
                break;
            case KeyboardKey.Left:
                player.SetMoveLeft(true);
                break;
            case KeyboardKey.Right:
                player.SetMoveRight(true);
                break;
            case KeyboardKey.Space:
                PlayerShot playerShot = new PlayerShot(player.GetPosition(), playerShotImage);
                playerShots.AddEntity(playerShot);
                break;
        }
        // TODO: Close window if escape is pressed
        // TODO: switch on key string and set the player's move direction
    }

    private void KeyRelease(KeyboardKey key) {
        switch (key) {
            case KeyboardKey.Left:
                player.SetMoveLeft(false);
                break;
            case KeyboardKey.Right:
                player.SetMoveRight(false);
                break;
        }
        // TODO: switch on key string and disable the player's move direction
    }

    private void KeyHandler(KeyboardAction action, KeyboardKey key) {
        switch (action) {
            case KeyboardAction.KeyRelease:
                KeyRelease(key);
                break;
            case KeyboardAction.KeyPress:
                KeyPress(key);
                break;
            }

        // TODO: Switch on KeyBoardAction and call proper method
        }

    private void IterateShots() {
        playerShots.Iterate(shot => {
            shot.Move();
            if (shot.Shape.Position.X > 1.0f) {
                shot.DeleteEntity();
            } else {
                enemies.Iterate(enemy => {
                    if (CollisionDetection.Aabb(shot.Shape.AsDynamicShape(), enemy.Shape).Collision) {
                        AddExplosion(enemy.Shape.Position, enemy.Shape.Extent);
                        shot.DeleteEntity();
                        enemy.DeleteEntity();
                    }
                });
            }
        });
    }

    public void AddExplosion(Vec2F position, Vec2F extent) {
        enemyExplosions.AddAnimation(new StationaryShape(position, extent), 
            EXPLOSION_LENGTH_MS, 
            new ImageStride(EXPLOSION_LENGTH_MS/8, explosionStrides));
        // TODO: add explosion to the AnimationContainer
    }

    public void ProcessEvent(GameEvent gameEvent) {
        // Leave this empty for now
    }
}
