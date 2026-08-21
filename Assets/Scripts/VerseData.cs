using UnityEngine;

namespace BibleMatch3
{
    /// <summary>
    /// Um versículo exibido no "Card de Versículo" do Estudo Infinito.
    /// </summary>
    [CreateAssetMenu(fileName = "NovoVersiculo", menuName = "BibleMatch3/Versículo")]
    public class VerseData : ScriptableObject
    {
        [TextArea] public string Texto;
        public string Referencia; // ex: "João 3:16"
        [TextArea] public string Reflexao;
    }
}
