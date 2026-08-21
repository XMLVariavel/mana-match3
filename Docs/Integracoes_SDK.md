# Integração dos SDKs externos — Maná

Três integrações dependem de SDKs que **não podem ser importados fora do Editor da
Unity** (vêm de Package Manager, `.unitypackage` ou registries privados). Este
documento separa, para cada uma, **o que já está pronto no código** do **que só
você pode fazer** (contas, consoles, IDs).

> **Pré-requisito comum:** este repositório contém apenas `Scripts/`, `Editor/`,
> `Art/`, `Tests/` e `Docs/`. Ele precisa estar **dentro da pasta `Assets/`** do
> seu projeto Unity. Sem os SDKs abaixo, `FirebaseManager`, `AdsManager` e
> `PurchaseManager` **não compilam** — eles referenciam os namespaces dos SDKs
> diretamente. Importe os pacotes **antes** de rodar `Tools > Maná > Montar cena
> completa`, senão o montador nem aparece no menu.

---

## 1. Google Sign-In

### Já feito no código

| Item | Onde |
|---|---|
| Serviço que obtém o `idToken` e chama o Firebase | `Scripts/Integracoes/GoogleSignInService.cs` |
| Botão "Entrar com Google" ligado ao serviço | `Scripts/UI/LoginView.cs` + montador |
| Consumo do token | `FirebaseManager.VincularContaGoogle(idToken, callback)` (já existia) |

O código do SDK está sob `#if MANA_GOOGLE_SIGNIN`. **Sem esse define o projeto
compila normalmente** e o botão apenas informa que o recurso está indisponível.
Isso é proposital: a ausência de um plugin externo não pode quebrar a build toda.

### O que você precisa fazer

1. **Importar o plugin.** Baixe o `.unitypackage` mais recente de
   [googlesignin-unity](https://github.com/googlesamples/google-signin-unity/releases)
   e importe via `Assets > Import Package > Custom Package`.
2. **Google Cloud Console** (só você):
   - Criar/selecionar o projeto vinculado ao mesmo projeto Firebase.
   - Criar credencial OAuth 2.0 do tipo **Android** com o *package name* e o
     **SHA-1** da sua keystore (debug e release são SHA-1 diferentes).
   - Criar credencial OAuth 2.0 do tipo **Web application** e copiar o
     **Web Client ID**.
3. **Firebase Console** → Authentication → Sign-in method → habilitar **Google**.
4. **Na Unity:**
   - `Project Settings > Player > Android > Other Settings > Scripting Define
     Symbols` → adicionar `MANA_GOOGLE_SIGNIN`.
   - Selecionar `[Maná] Sistemas` na cena → componente `GoogleSignInService` →
     colar o **Web Client ID** (é o Web, não o Android).
5. Colocar o `google-services.json` em `Assets/` (o plugin do Firebase lê dele).

> **Atenção:** o campo é o *Web Client ID*. Usar o Android Client ID é o erro mais
> comum e falha silenciosamente com "token inválido".

---

## 2. AdMob (Google Mobile Ads)

### Já feito no código

| Item | Onde |
|---|---|
| Inicialização, carga e exibição de recompensado/intersticial | `Scripts/AdsManager.cs` (já existia) |
| Respeito à compra "Remover Anúncios" | `PurchaseManager` ↔ `AdsManager` (já existia) |
| **IDs de unidade agora são as unidades de TESTE oficiais do Google** | `Scripts/AdsManager.cs` |

Os placeholders `SEU_AD_UNIT_ID_*` foram trocados pelas unidades de teste
documentadas pelo Google:

- Recompensado: `ca-app-pub-3940256099942544/5224354917`
- Intersticial: `ca-app-pub-3940256099942544/1033173712`

Assim você consegue **testar o fluxo de anúncios hoje**, sem risco de banimento
por cliques inválidos. Elas **não geram receita** e precisam ser trocadas antes
de publicar.

### O que você precisa fazer

1. **Importar o SDK:** baixe o `.unitypackage` de
   [googleads-mobile-unity/releases](https://github.com/googleads/googleads-mobile-unity/releases)
   e importe. Depois rode `Assets > External Dependency Manager > Android
   Resolver > Force Resolve`.
2. **Criar a conta AdMob** e registrar o app (só você).
3. Criar as **unidades de anúncio reais** (uma recompensada, uma intersticial).
4. `Assets > Google Mobile Ads > Settings` → colar o **App ID** do AdMob.
5. Selecionar `[Maná] Sistemas` → componente `AdsManager` → substituir os dois IDs
   de teste pelos reais. **Me passe os IDs se quiser que eu troque no código.**

---

## 3. Unity IAP (Remover Anúncios)

### Já feito no código

| Item | Onde |
|---|---|
| `IStoreListener`, inicialização, compra e restauração | `Scripts/PurchaseManager.cs` (já existia) |
| Produto esperado: `remover_anuncios`, **não-consumível** | `PurchaseManager` |
| Persistência do "sem anúncios" no perfil | `PlayerProgress.SemAnuncios` (já existia) |
| Botões de compra na Loja e em Configurações | montador do Editor |

### O que você precisa fazer

1. **Instalar o pacote:** `Window > Package Manager > Unity Registry >
   In-App Purchasing > Install`. Ou adicionar em `Packages/manifest.json`:
   ```json
   "com.unity.purchasing": "4.12.2"
   ```
2. `Project Settings > Services > In-App Purchasing` → **Enable** (exige o projeto
   ligado a uma Unity Organization).
3. **Google Play Console** (só você):
   - Publicar ao menos uma build em teste interno (obrigatório para IAP funcionar).
   - Criar o produto gerenciado com ID **exatamente** `remover_anuncios`, tipo
     **não-consumível**, e **ativá-lo**.
   - Adicionar sua conta como testador de licença.
4. Se após instalar o pacote aparecer erro de referência de assembly, adicione
   `"Unity.Purchasing"` ao array `references` de `Scripts/BibleMatch3.asmdef` — o
   Unity IAP expõe um asmdef próprio em algumas versões.

---

## Ordem recomendada

1. Colocar este conteúdo dentro de `Assets/` no projeto Unity.
2. Importar **Firebase** (Auth + Firestore), **Google Mobile Ads** e **Unity IAP**.
3. Confirmar que o projeto compila sem erros.
4. Rodar `Tools > Maná > Montar cena completa`.
5. Salvar a cena e adicioná-la em `Build Settings`.
6. Só então importar o Google Sign-In e ligar o define `MANA_GOOGLE_SIGNIN`.

---

## Resumo do que continua pendente do seu lado

| # | Pendência | Onde se resolve |
|---|---|---|
| 1 | Importar Firebase (Auth + Firestore) + `google-services.json` | Firebase Console + Unity |
| 2 | Importar Google Mobile Ads SDK | GitHub + Unity |
| 3 | Importar plugin googlesignin-unity | GitHub + Unity |
| 4 | Instalar `com.unity.purchasing` | Package Manager |
| 5 | Criar conta AdMob, app e 2 unidades de anúncio reais | AdMob Console |
| 6 | Substituir os 2 IDs de teste pelos reais + App ID | Unity Inspector / Settings |
| 7 | Criar credenciais OAuth Android (SHA-1) e Web no Google Cloud | Google Cloud Console |
| 8 | Preencher o Web Client ID no `GoogleSignInService` | Unity Inspector |
| 9 | Habilitar provedor Google no Firebase Authentication | Firebase Console |
| 10 | Cadastrar `remover_anuncios` (não-consumível) e ativar | Google Play Console |
| 11 | Publicar build em teste interno (pré-requisito do IAP) | Google Play Console |
| 12 | Adicionar o define `MANA_GOOGLE_SIGNIN` | Player Settings |
