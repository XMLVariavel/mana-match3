using System.Collections.Generic;
using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Contexto passado a um efeito especial no momento da ativação — contém
    /// tudo que o efeito pode precisar para calcular o que deve ser destruído.
    /// </summary>
    public struct EfeitoContexto
    {
        public Tile[,] Grid;
        public int Width;
        public int Height;
        public int OriginX;
        public int OriginY;
        public TileType CorAlvo;
        public int Nivel;
        public ScoreAndObjectiveManager ObjectiveManager; // usado pela Estrela Guia
    }

    /// <summary>
    /// Contrato do padrão Strategy para os efeitos das peças especiais de tabuleiro.
    /// Cada implementação decide, a partir do contexto, quais peças entram em
    /// result.TilesToDestroy.
    /// </summary>
    public interface IEfeitoEspecial
    {
        void Aplicar(EfeitoContexto contexto, MatchResult result);
    }

    /// <summary>
    /// Base como ScriptableObject: cada efeito vira um asset arrastável no
    /// Inspector (referenciado pelo PowerUpConfig), mantendo o Strategy
    /// idiomático ao fluxo de dados da Unity em vez de um switch fixo no código.
    /// </summary>
    public abstract class EfeitoEspecialSO : ScriptableObject, IEfeitoEspecial
    {
        public abstract void Aplicar(EfeitoContexto contexto, MatchResult result);

        protected void AdicionarSeExistir(Tile[,] grid, int x, int y, int width, int height, MatchResult result)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            Tile t = grid[x, y];
            if (t != null) result.TilesToDestroy.Add(t);
        }
    }

    [CreateAssetMenu(fileName = "EfeitoEspadaLinha", menuName = "BibleMatch3/Efeitos/Espada da Palavra (Linha)")]
    public class EfeitoEspadaLinhaSO : EfeitoEspecialSO
    {
        public override void Aplicar(EfeitoContexto c, MatchResult result)
        {
            for (int x = 0; x < c.Width; x++)
                AdicionarSeExistir(c.Grid, x, c.OriginY, c.Width, c.Height, result);
        }
    }

    [CreateAssetMenu(fileName = "EfeitoEspadaColuna", menuName = "BibleMatch3/Efeitos/Espada da Palavra (Coluna)")]
    public class EfeitoEspadaColunaSO : EfeitoEspecialSO
    {
        public override void Aplicar(EfeitoContexto c, MatchResult result)
        {
            for (int y = 0; y < c.Height; y++)
                AdicionarSeExistir(c.Grid, c.OriginX, y, c.Width, c.Height, result);
        }
    }

    /// <summary>
    /// Tocha Acesa: nível 1 = área 3x3, nível 2 e 3 = área 4x4.
    /// No nível 3, qualquer peça especial atingida pela explosão também é
    /// ativada — o "dano em cascata" citado no briefing.
    /// </summary>
    [CreateAssetMenu(fileName = "EfeitoTochaAcesa", menuName = "BibleMatch3/Efeitos/Tocha Acesa")]
    public class EfeitoTochaAcesaSO : EfeitoEspecialSO
    {
        public override void Aplicar(EfeitoContexto c, MatchResult result)
        {
            int alcanceExtra = c.Nivel >= 2 ? 2 : 1; // nível 1: -1..+1 (3x3) | nível 2-3: -1..+2 (4x4)
            var atingidas = new List<Tile>();

            for (int x = c.OriginX - 1; x <= c.OriginX + alcanceExtra; x++)
            {
                for (int y = c.OriginY - 1; y <= c.OriginY + alcanceExtra; y++)
                {
                    if (x < 0 || x >= c.Width || y < 0 || y >= c.Height) continue;
                    Tile t = c.Grid[x, y];
                    if (t == null) continue;
                    result.TilesToDestroy.Add(t);
                    atingidas.Add(t);
                }
            }

            if (c.Nivel >= 3)
            {
                foreach (Tile t in atingidas)
                {
                    if (t.Special == SpecialType.Nenhum) continue;
                    // Sinaliza para o MatchDetector expandir o efeito desta peça também
                    // (ele conhece o PowerUpConfig de cada SpecialType e evita loops).
                    result.SpecialsAtivadasEmCascata.Add(t);
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "EfeitoArcaAlianca", menuName = "BibleMatch3/Efeitos/Arca da Aliança")]
    public class EfeitoArcaAliancaSO : EfeitoEspecialSO
    {
        public override void Aplicar(EfeitoContexto c, MatchResult result)
        {
            for (int x = 0; x < c.Width; x++)
                for (int y = 0; y < c.Height; y++)
                {
                    Tile t = c.Grid[x, y];
                    if (t != null && t.Type == c.CorAlvo) result.TilesToDestroy.Add(t);
                }
        }
    }

    /// <summary>
    /// Estrela Guia: busca no tabuleiro o tipo de peça pendente no objetivo
    /// atual da fase e remove algumas ocorrências dele.
    /// </summary>
    [CreateAssetMenu(fileName = "EfeitoEstrelaGuia", menuName = "BibleMatch3/Efeitos/Estrela Guia")]
    public class EfeitoEstrelaGuiaSO : EfeitoEspecialSO
    {
        [SerializeField] private int quantidadeAlvo = 5;

        public override void Aplicar(EfeitoContexto c, MatchResult result)
        {
            TileType? objetivo = c.ObjectiveManager != null ? c.ObjectiveManager.GetPriorityObjectiveType() : null;
            TileType alvo = objetivo ?? c.CorAlvo;

            int encontradas = 0;
            for (int x = 0; x < c.Width && encontradas < quantidadeAlvo; x++)
            {
                for (int y = 0; y < c.Height && encontradas < quantidadeAlvo; y++)
                {
                    Tile t = c.Grid[x, y];
                    if (t != null && t.Type == alvo)
                    {
                        result.TilesToDestroy.Add(t);
                        encontradas++;
                    }
                }
            }
        }
    }
}
