using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Preenche campos <c>[SerializeField] private</c> de fora do componente.
    /// É o equivalente em código a arrastar objetos no Inspector: sem isso, um
    /// montador automático não conseguiria ligar nada, porque os controllers
    /// (corretamente) mantêm suas dependências privadas.
    ///
    /// Uso:
    /// <code>
    /// using (var l = new Ligador(hud))
    /// {
    ///     l.Ref("scoreManager", score).Texto("telaMapaDeFases", "MapaDeFases");
    /// }
    /// </code>
    /// O <c>Dispose</c> aplica as mudanças de uma vez só.
    /// </summary>
    internal sealed class Ligador : System.IDisposable
    {
        private readonly SerializedObject serializado;
        private readonly string nomeDoAlvo;

        public Ligador(Object alvo)
        {
            serializado = new SerializedObject(alvo);
            nomeDoAlvo = alvo != null ? $"{alvo.GetType().Name} ({alvo.name})" : "<nulo>";
        }

        private SerializedProperty Achar(string campo)
        {
            SerializedProperty propriedade = serializado.FindProperty(campo);
            if (propriedade == null)
                Debug.LogWarning($"[Maná] Campo '{campo}' não existe em {nomeDoAlvo}. " +
                                 "O script foi renomeado? A ligação foi ignorada.");
            return propriedade;
        }

        public Ligador Ref(string campo, Object valor)
        {
            SerializedProperty p = Achar(campo);
            if (p != null) p.objectReferenceValue = valor;
            return this;
        }

        public Ligador Texto(string campo, string valor)
        {
            SerializedProperty p = Achar(campo);
            if (p != null) p.stringValue = valor;
            return this;
        }

        public Ligador Numero(string campo, int valor)
        {
            SerializedProperty p = Achar(campo);
            if (p != null) p.intValue = valor;
            return this;
        }

        public Ligador Decimal(string campo, float valor)
        {
            SerializedProperty p = Achar(campo);
            if (p != null) p.floatValue = valor;
            return this;
        }

        public Ligador Booleano(string campo, bool valor)
        {
            SerializedProperty p = Achar(campo);
            if (p != null) p.boolValue = valor;
            return this;
        }

        /// <summary>Preenche um array/List de referências (sprites, configs, fases...).</summary>
        public Ligador Lista(string campo, IList<Object> valores)
        {
            SerializedProperty p = Achar(campo);
            if (p == null) return this;

            if (!p.isArray)
            {
                Debug.LogWarning($"[Maná] Campo '{campo}' em {nomeDoAlvo} não é uma lista.");
                return this;
            }

            p.arraySize = valores?.Count ?? 0;
            for (int i = 0; i < p.arraySize; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];

            return this;
        }

        public void Dispose() => serializado.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// Ligações de UnityEvent que sobrevivem ao fechar o Editor.
    ///
    /// Detalhe importante: <c>onClick.AddListener(...)</c> só existe em memória
    /// e some quando a cena recarrega. Para o montador entregar botões já
    /// ligados de verdade — visíveis no Inspector — é preciso registrar
    /// listeners *persistentes*, que é o que <see cref="UnityEventTools"/> faz.
    /// </summary>
    internal static class Eventos
    {
        public static void AoClicar(UnityEngine.UI.Button botao, UnityAction acao)
        {
            if (botao == null || acao == null) return;
            UnityEventTools.AddPersistentListener(botao.onClick, acao);
        }

        /// <summary>
        /// Botão que navega para uma tela. Usa listener persistente com
        /// argumento fixo, então o nome da tela fica visível no Inspector —
        /// é o mesmo que arrastar o ScreenNavigator e digitar o nome à mão.
        /// </summary>
        public static void AoClicarIrPara(UnityEngine.UI.Button botao, ScreenNavigator navegador, string tela)
        {
            if (botao == null || navegador == null) return;
            UnityEventTools.AddStringPersistentListener(
                botao.onClick, new UnityAction<string>(navegador.Mostrar), tela);
        }

        /// <summary>Registra o valor do próprio controle como argumento (modo dinâmico).</summary>
        public static void AoMudarBool(UnityEngine.UI.Toggle toggle, UnityAction<bool> acao)
        {
            if (toggle == null || acao == null) return;
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, acao);
        }

        public static void AoMudarFloat(UnityEngine.UI.Slider slider, UnityAction<float> acao)
        {
            if (slider == null || acao == null) return;
            UnityEventTools.AddPersistentListener(slider.onValueChanged, acao);
        }
    }
}
