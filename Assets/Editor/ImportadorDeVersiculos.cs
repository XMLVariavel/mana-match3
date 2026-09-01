#if UNITY_EDITOR
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEditor;
using UnityEngine;

namespace BibleMatch3.EditorTools
{
    /// <summary>
    /// Ferramenta de uso único: lê Assets/GameData/versiculos.json e cria um
    /// documento por versículo na coleção "versiculos" do Firestore.
    ///
    /// Antes de rodar: abra as regras do Firestore e troque temporariamente
    ///   match /versiculos/{id} { allow write: if false; }
    /// para
    ///   match /versiculos/{id} { allow write: if request.auth != null; }
    /// Rode a importação (com o Editor em Play Mode, já logado). Depois,
    /// volte a regra pro "if false" — o app nunca precisa escrever aqui de
    /// novo, só ler.
    /// </summary>
    public static class ImportadorDeVersiculos
    {
        [MenuItem("Tools/Maná/Importar Versículos (uma vez)")]
        public static void Importar()
        {
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/versiculos.json");
            if (json == null)
            {
                Debug.LogError("[Maná] Não achei Assets/GameData/versiculos.json — copie o arquivo pra lá primeiro.");
                return;
            }

            ListaVersiculos lista = JsonUtility.FromJson<ListaVersiculos>(json.text);
            if (lista == null || lista.versiculos == null || lista.versiculos.Count == 0)
            {
                Debug.LogError("[Maná] Arquivo JSON vazio ou em formato inesperado.");
                return;
            }

            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            int total = lista.versiculos.Count;
            int concluidos = 0;
            int falhas = 0;

            Debug.Log($"[Maná] Iniciando importação de {total} versículos...");

            for (int i = 0; i < total; i++)
            {
                ItemVersiculo v = lista.versiculos[i];
                var dados = new Dictionary<string, object>
                {
                    { "texto", v.texto },
                    { "referencia", v.referencia }
                };

                int indice = i;
                db.Collection("versiculos").Document($"versiculo_{indice:000}").SetAsync(dados).ContinueWithOnMainThread(task =>
                {
                    concluidos++;
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        falhas++;
                        Debug.LogError($"[Maná] Falha ao importar versiculo_{indice:000}: {task.Exception}");
                    }

                    if (concluidos == total)
                    {
                        Debug.Log(falhas == 0
                            ? $"[Maná] Importação concluída: {total} versículos enviados ao Firestore."
                            : $"[Maná] Importação concluída com {falhas} falha(s) de {total}. Confira os erros acima — provavelmente a regra de escrita ainda não foi liberada.");
                    }
                });
            }
        }

        [System.Serializable]
        private class ItemVersiculo
        {
            public string texto;
            public string referencia;
        }

        [System.Serializable]
        private class ListaVersiculos
        {
            public List<ItemVersiculo> versiculos;
        }
    }
}
#endif
