/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

namespace RiptideFNATankServer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiptideFNATankCommon;
using RiptideFNATankCommon.Networking;
using RiptideFNATankServer.Gameplay;
using RiptideFNATankServer.Networking;
using Wombat.Engine;

/// <summary>
/// Very simple multiplayer implementation of the server for the game, Tank using the Riptide framework, MoonTools.ECS and Quake3 style client / server multiplayer
/// </summary>
/// <remarks>
/// Based on
/// </remarks>
public class ServerGame : Game
{
    // Multiplayer
    ServerNetworkManager _networkGameManager;

    // ECS
    ServerECSManager _ecsManager;

    //Mapping between networking and ECS
    readonly PlayerEntityMapper _playerEntityMapper = new();

    // Maps
    const int PLAYER_OFFSET_X = 32;

    readonly Vector2[] _playerSpawnPoints = [
        new Vector2(PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2),
        new Vector2(BaseGame.SCREEN_WIDTH - PLAYER_OFFSET_X, BaseGame.SCREEN_HEIGHT / 2)
    ];

    int _playerSpawnPointsIdx = 0;

    //
    SpriteBatch _spriteBatch;

    public ServerGame()
    {
        Window.Title = "Riptide FNA Tank - SERVER";

        Logger.Info("==================================================");
        Logger.Info($"{Window.Title}");
        Logger.Info("==================================================");

        _ = new GraphicsDeviceManager(this);

        TargetElapsedTime = TimeSpan.FromMicroseconds(1000.0f / 20);
    }

    protected override void Initialize()
    {
        base.Initialize();

        _networkGameManager = new(
            port: 17871,
            maxClientCount: 4);

        _ecsManager = new ServerECSManager(_networkGameManager, _playerEntityMapper, _spriteBatch);

        _networkGameManager.ClientConnected += ClientConnectedHandler;
        _networkGameManager.ReceivedClientState += _ecsManager.ClientStateReceivedHandler;

        _networkGameManager.StartServer();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Resources.PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        Resources.PixelTexture.SetData(new[] { Color.White });

        //Resources.DefaultSpriteFont = Content.Load<SpriteFont>("SpriteFonts/debug");

        //BasicEffect = new BasicEffect(GraphicsDevice)
        //{
        //    World = ModelMatrix,
        //    View = ViewMatrix,
        //    Projection = ProjectionMatrix,
        //    TextureEnabled = false,
        //    VertexColorEnabled = true
        //};

        //var fontPath = Path.Combine(Path.GetFullPath("."), "Content", "Fonts", "SquaredDisplay.ttf");
        //Resources.GameFont = TtfFontBaker.Bake(
        //    File.ReadAllBytes(fontPath),
        //    96,
        //    1024,
        //    1024,
        //    new[] {
        //        CharacterRange.BasicLatin,
        //        CharacterRange.Latin1Supplement,
        //        CharacterRange.LatinExtendedA,
        //        CharacterRange.Cyrillic
        //   }).CreateSpriteFont(GraphicsDevice);

        //Resources.SmallFont = TtfFontBaker.Bake(
        //    File.ReadAllBytes(fontPath),
        //    32,
        //    1024,
        //    1024,
        //    new[] {
        //        CharacterRange.BasicLatin,
        //        CharacterRange.Latin1Supplement,
        //        CharacterRange.LatinExtendedA,
        //        CharacterRange.Cyrillic
        //   }).CreateSpriteFont(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _networkGameManager.Update();
        _ecsManager.Update(gameTime);
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);

        _networkGameManager.Stop();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkGray);
        _ecsManager.Draw();
    }

    private void ClientConnectedHandler(ServerNetworkManager.ClientConnectedArgs e)
    {
        var name = e.Message.GetString();

#if DEBUG
        Logger.Info($"Message handler: {nameof(ClientConnectedHandler)} from client: {e.ClientId}");
        Logger.Debug("Read the following...");
        Logger.Debug($"{name}");
#endif

        //TODO: probably need some logic here to do map stuff, get spawn points, etc
        var position = _playerSpawnPoints[_playerSpawnPointsIdx];
        _networkGameManager.SpawnPlayer(e.ClientId, name, position);

        _ecsManager.SpawnPlayer(e.ClientId, name, position);

        PrepareNextPlayer();
    }

    void PrepareNextPlayer()
    {
        // Cycle through the spawn points so that players are located in the correct postions
        _playerSpawnPointsIdx = (_playerSpawnPointsIdx + 1) % _playerSpawnPoints.Length;
    }
}
