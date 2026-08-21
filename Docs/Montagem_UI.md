# Montagem da UI — Maná

## Por que um montador de Editor, e não arquivos `.unity`/`.prefab`

Cenas e prefabs da Unity são YAML com `fileID`s e GUIDs gerados pelo próprio
Editor. Escrever esses arquivos fora da Unity produz cenas corrompidas com muita
facilidade — referências quebradas, componentes órfãos, prefabs que não abrem.

Por isso a UI é construída por **código de Editor**: quem cria cada GameObject,
cada componente e cada referência é a própria Unity. O resultado é sempre válido
e o processo é repetível.

```
Tools > Maná > Montar cena completa
```

---

## O que o montador faz

Com a cena que você quer usar **aberta**, um clique executa:

1. **Prepara a arte** — reimporta os 15 PNGs de `Art/Placeholder/` como Sprite,
   256px, alpha transparente, 256 pixels por unidade (1 peça = 1 célula), além
   de `Art/UI/fundo_jornada.png` e `Art/UI/moldura_tabuleiro.png` em alta resolução
   para as telas e a moldura do tabuleiro.
2. **Gera dados de exemplo** em `Assets/GameData/` — 5 poderes de tabuleiro,
   3 poderes avulsos, os 8 ScriptableObjects de efeito, 5 fases de campanha e
   5 versículos.
3. **Gera prefabs** em `Assets/Prefabs/` — `Peca`, `Obstaculo`, `BotaoFase`,
   `ItemRanking`, `ItemLoja`, `ItemObjetivo`.
4. **Monta a cena** — câmera, EventSystem, managers, tabuleiro, Canvas e as
   9 telas.
5. **Liga tudo** — todos os `[SerializeField]` privados e todos os `OnClick`.

### Idempotência

Rodar de novo apaga apenas as raízes com o prefixo `[Maná]` e reconstrói. O resto
da cena não é tocado.

Assets de dados **já existentes nunca são sobrescritos**: se você ajustar o
balanceamento de uma fase ou o custo de um poder no Inspector, uma nova montagem
preserva o ajuste.

---

## Hierarquia produzida

```
Main Camera                    (ortográfica, size 8 — cabe 8 células em 9:16)
EventSystem                    (StandaloneInputModule — mesmo Input do BoardManager)
[Maná] Áudio                   AudioManager, HapticsManager, 2 AudioSource
[Maná] Sistemas                FirebaseManager, PrivacyManager, AdsManager,
                               PurchaseManager, LivesManager, BoosterManager,
                               ScreenNavigator, GoogleSignInService
[Maná] Tabuleiro               BoardManager, MatchDetector, BoardPhysics,
                               ScoreAndObjectiveManager, ObstacleManager,
                               GameManager, GameFeedbackController
   ├── Origem                  (-3.5, -3.5) → tabuleiro centrado em (0,0)
   ├── MolduraTabuleiro        (arte ilustrada atrás das peças)
   ├── Pecas
   └── Obstaculos
[Maná] Canvas                  1080x1920, Scale With Screen Size
   ├── Splash                  SplashOnboardingController
   ├── TelaConsentimento       (LGPD — Aceitar / Recusar)
   ├── TelaCarregando
   ├── MapaDeFases             MapaDeFasesController + RecursosView
   ├── TelaJogo                GameHUDController + GameHUDView +
   │                           VerseCardModalController + VerseCardView
   ├── Loja                    LojaController + LojaView
   ├── Perfil                  PerfilController + PerfilView
   ├── Ranking                 RankingController + RankingView
   ├── Configuracoes           ConfiguracoesController + ConfiguracoesView
   └── Login                   LoginController + LoginView
```

Os nomes das telas batem exatamente com os defaults que cada controller espera no
`ScreenNavigator` (`MapaDeFases`, `TelaJogo`, `Loja`, `Perfil`, `Ranking`,
`Configuracoes`, `Login`, `Splash`, `TelaConsentimento`, `TelaCarregando`).

### Por que existem scripts `*View`

Os controllers expõem **eventos C#**, não campos de UI — eles não conhecem
`TextMeshProUGUI` nem `Button`. Isso é bom design, mas significa que alguém
precisa escutar esses eventos e escrever na tela. Esse é o papel dos `View` em
`Scripts/UI/`: nenhuma regra de jogo mora neles, só desenho.

### Ligação dos botões

Os `OnClick` são registrados como **listeners persistentes**
(`UnityEventTools`), não com `AddListener` em runtime — assim eles sobrevivem ao
fechar o Editor e ficam **visíveis no Inspector**, exatamente como se você tivesse
arrastado à mão.

Listas dinâmicas (itens da Loja, botões de fase) são ligadas em runtime pelas
`View`, porque os itens só existem depois que os dados chegam.

---

## Menus auxiliares

| Menu | Uso |
|---|---|
| `Tools > Maná > Montar cena completa` | Tudo, do zero |
| `Tools > Maná > Só preparar arte placeholder` | Depois de trocar PNGs |
| `Tools > Maná > Só gerar prefabs` | Depois de mexer nos scripts de item |
| `Tools > Maná > Só gerar dados de exemplo` | Criar fases/poderes que faltarem |

---

## Pré-requisitos antes de rodar

1. Este conteúdo precisa estar **dentro de `Assets/`** do projeto Unity.
2. **TextMeshPro Essentials** importado
   (`Window > TextMeshPro > Import TMP Essential Resources`). Sem isso os textos
   aparecem sem fonte.
3. Os SDKs de Firebase, AdMob e IAP importados — veja `Docs/Integracoes_SDK.md`.
   Sem eles o projeto **não compila**, e um menu de Editor não aparece enquanto
   houver erro de compilação.
4. **Input System:** o projeto usa o `Input` clássico (`BoardManager` lê toque por
   `UnityEngine.Input`). Em `Project Settings > Player > Active Input Handling`,
   use `Input Manager (Old)` ou `Both`.

---

## Depois de montar

1. Salve a cena (`Ctrl+S`) e adicione-a em `File > Build Settings`.
2. Dê **Play**. O fluxo esperado:
   `Splash → Consentimento → Carregando → Mapa de Fases → Tela de Jogo`.
3. Se algum campo aparecer vazio no Inspector, o Console mostra um aviso
   `[Maná] Campo 'x' não existe em Y` — isso significa que um script foi
   renomeado depois do montador ter sido escrito.

### Áudio

`AudioManager` já toca e persiste volumes, mas a **biblioteca de clipes está
vazia** — não há arquivos de som no projeto. Para ouvir algo:

1. Selecione `[Maná] Áudio` → `AudioManager`.
2. Preencha `Música Padrão` e a lista `Efeitos`, associando cada `EfeitoSonoro`
   (Match, ComboEspecial, EspecialCriado, Vitoria, Derrota...) a um `AudioClip`.

Sem clipes o jogo roda normalmente, apenas em silêncio — `TocarEfeito` sai sem
fazer nada e sem poluir o Console.

### Vibração

`Handheld.Vibrate()` só funciona em aparelho — no Editor e no PC é no-op por
projeto (`HapticsManager` checa `Application.isMobilePlatform`). Teste num Android
real.
