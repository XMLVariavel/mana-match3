using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace BibleMatch3
{
    [Serializable]
    public class VersiculoDoDia
    {
        public string Texto;
        public string Referencia;
    }

    /// <summary>
    /// Busca a lista de versículos do Firestore (coleção "versiculos"), guarda
    /// em cache local por 24h, e escolhe deterministicamente um versículo por
    /// dia — mesmo dia (UTC) = mesmo versículo pra todo mundo, sem sorteio e
    /// sem precisar consultar o Firestore toda vez que o jogo abre.
    /// </summary>
    public class VersiculoDoDiaService : MonoBehaviour
    {
        private const string ChaveCacheLista = "BibleMatch3_VersiculosCache";
        private const string ChaveDataCache = "BibleMatch3_VersiculosCacheData";

        public event Action<VersiculoDoDia> OnVersiculoPronto;
        public event Action<string> OnErro;

        private void Start()
        {
            CarregarEExibir();
        }

        public void CarregarEExibir()
        {
            string hojeChave = DateTime.UtcNow.ToString("yyyyMMdd");

            // Usa o cache local se já foi baixado hoje — evita gastar cota de
            // leitura do Firestore toda vez que o jogo abre.
            if (PlayerPrefs.GetString(ChaveDataCache, "") == hojeChave && PlayerPrefs.HasKey(ChaveCacheLista))
            {
                List<VersiculoDoDia> listaCache = DesserializarCache(PlayerPrefs.GetString(ChaveCacheLista));
                if (listaCache != null && listaCache.Count > 0)
                {
                    ExibirVersiculoDeHoje(listaCache);
                    return;
                }
            }

            BuscarDoFirestore(hojeChave);
        }

        private void BuscarDoFirestore(string hojeChave)
        {
            FirebaseFirestore.DefaultInstance.Collection("versiculos").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    OnErro?.Invoke("Não foi possível buscar o versículo de hoje.");
                    Debug.LogWarning($"Falha ao buscar versículos: {task.Exception}");
                    return;
                }

                var lista = new List<VersiculoDoDia>();
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    string texto = doc.ContainsField("texto") ? doc.GetValue<string>("texto") : null;
                    string referencia = doc.ContainsField("referencia") ? doc.GetValue<string>("referencia") : null;
                    if (!string.IsNullOrEmpty(texto))
                        lista.Add(new VersiculoDoDia { Texto = texto, Referencia = referencia });
                }

                if (lista.Count == 0)
                {
                    OnErro?.Invoke("Nenhum versículo cadastrado ainda.");
                    return;
                }

                PlayerPrefs.SetString(ChaveCacheLista, SerializarCache(lista));
                PlayerPrefs.SetString(ChaveDataCache, hojeChave);
                PlayerPrefs.Save();

                ExibirVersiculoDeHoje(lista);
            });
        }

        private void ExibirVersiculoDeHoje(List<VersiculoDoDia> lista)
        {
            int diaDoAno = DateTime.UtcNow.DayOfYear;
            int indice = diaDoAno % lista.Count;
            OnVersiculoPronto?.Invoke(lista[indice]);
        }

        private string SerializarCache(List<VersiculoDoDia> lista) =>
            JsonUtility.ToJson(new VersiculosWrapper { Itens = lista });

        private List<VersiculoDoDia> DesserializarCache(string json)
        {
            try
            {
                return JsonUtility.FromJson<VersiculosWrapper>(json)?.Itens;
            }
            catch
            {
                return null;
            }
        }

        [Serializable]
        private class VersiculosWrapper
        {
            public List<VersiculoDoDia> Itens;
        }
    }
}
