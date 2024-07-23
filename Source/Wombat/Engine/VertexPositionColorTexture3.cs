/*
 _       __                __          __ 
| |     / /___  ____ ___  / /_  ____ _/ /_
| | /| / / __ \/ __ `__ \/ __ \/ __ `/ __/
| |/ |/ / /_/ / / / / / / /_/ / /_/ / /_  
|__/|__/\____/_/ /_/ /_/_.___/\__,_/\__/  

A simple 2D engine - use as the base engine for experiments and POCs.
                                          
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wombat.Engine;

public struct VertexPositionColorTexture3 : IVertexType
{
    public Vector3 Position;
    public Color Color;
    public Vector3 Texture;

    static readonly VertexDeclaration VertexDeclaration = new
    (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
    );

    readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public VertexPositionColorTexture3(Vector3 position, Color color, Vector3 texture)
    {
        Position = position;
        Color = color;
        Texture = texture;
    }
}