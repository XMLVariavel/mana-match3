using System;
using System.Collections.Generic;

namespace BibleMatch3
{
    [Serializable]
    public sealed class AvatarOption
    {
        public string Id;
        public string Nome;
        public string CaminhoSprite;
        public bool LiberadoInicialmente;

        public AvatarOption(string id, string nome, string caminhoSprite, bool liberadoInicialmente)
        {
            Id = id;
            Nome = nome;
            CaminhoSprite = caminhoSprite;
            LiberadoInicialmente = liberadoInicialmente;
        }
    }

    public static class AvatarCatalog
    {
        public const string Padrao = "davi";

        private static readonly List<AvatarOption> opcoes = new List<AvatarOption>
        {
            new AvatarOption("davi", "Davi", "Art/Avatars/avatar_davi", true),
            new AvatarOption("ester", "Ester", "Art/Avatars/avatar_ester", true),
            new AvatarOption("daniel", "Daniel", "Art/Avatars/avatar_daniel", true),
            new AvatarOption("rute", "Rute", "Art/Avatars/avatar_rute", true),
            new AvatarOption("moises", "Moisés", "Art/Avatars/avatar_moises", true)
        };

        public static IReadOnlyList<AvatarOption> Opcoes => opcoes;

        public static bool Existe(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (AvatarOption opcao in opcoes)
                if (string.Equals(opcao.Id, id, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static AvatarOption Obter(string id)
        {
            foreach (AvatarOption opcao in opcoes)
                if (string.Equals(opcao.Id, id, StringComparison.OrdinalIgnoreCase)) return opcao;
            return opcoes[0];
        }
    }
}
