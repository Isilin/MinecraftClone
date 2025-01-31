using UnityEngine;

public class TextureAtlas
{
    public class Block
    {
        public Vector2Int top { get; private set; }
        public Vector2Int side { get; private set; }
        public Vector2Int bottom { get; private set; }

        public Block(Vector2Int top, Vector2Int side, Vector2Int bottom)
        {
            this.top = top;
            this.side = side;
            this.bottom = bottom;
        }
    }

    public Block Grass { get; private set; }
    public Block Dirt { get; private set; }

    public TextureAtlas()
    {
        this.Grass = new Block(new Vector2Int(8, 13), new Vector2Int(3, 15), new Vector2Int(2, 15));
        this.Dirt = new Block(new Vector2Int(2, 15), new Vector2Int(2, 15), new Vector2Int(2, 15));
    }
}
