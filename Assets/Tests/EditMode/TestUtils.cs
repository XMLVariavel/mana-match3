using System.Reflection;
using UnityEngine;

namespace BibleMatch3.Tests
{
    /// <summary>
    /// Utilitários compartilhados pelos testes EditMode: criação de peças
    /// "fake" para montar tabuleiros de teste e acesso via reflexão a campos
    /// privados serializados (normalmente conectados via Inspector, não em runtime).
    /// </summary>
    public static class TestUtils
    {
        public static Tile CriarTile(TileType type, int x, int y, SpecialType special = SpecialType.Nenhum)
        {
            var go = new GameObject($"TestTile_{x}_{y}");
            var tile = go.AddComponent<Tile>();
            tile.Type = type;
            tile.X = x;
            tile.Y = y;
            tile.Special = special;
            return tile;
        }

        public static Tile[,] CriarGrid(TileType[,] tipos)
        {
            int width = tipos.GetLength(0);
            int height = tipos.GetLength(1);
            var grid = new Tile[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    grid[x, y] = CriarTile(tipos[x, y], x, y);

            return grid;
        }

        public static void DestruirGrid(Tile[,] grid)
        {
            if (grid == null) return;
            foreach (Tile tile in grid)
                if (tile != null) Object.DestroyImmediate(tile.gameObject);
        }

        public static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new System.ArgumentException($"Campo privado '{fieldName}' não encontrado em {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        public static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new System.ArgumentException($"Campo privado '{fieldName}' não encontrado em {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }
    }
}
