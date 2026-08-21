# Curva de Dificuldade

## Campanha (por LevelData)

A dificuldade da Campanha é 100% definida pelo conteúdo de cada `LevelData`
(movimentos, objetivos, obstáculos) — não há lógica automática de escalonamento
aqui, é responsabilidade de quem cria as fases seguir esta curva:

| Faixa de fases | Objetivo do design | Obstáculos |
|---|---|---|
| 1–10 | Tutorial — ensinar mecânicas básicas de match e os tipos de especial | Nenhum |
| 11–20 | Introduz obstáculos isoladamente, um tipo por vez | Gelo OU Corrente (nunca os dois juntos) |
| 21–30 | Combina o obstáculo já visto com um segundo tipo | Gelo + Corrente, ou Caixa Selada isolada |
| 31+ | Fases "cheias" — múltiplos obstáculos, incluindo Pedra do Deserto isolando a gravidade | Combinações livres dos 4 tipos |

Recomendação prática: `Movimentos` decrescendo lentamente conforme os objetivos
crescem (mais movimentos por objetivo pendente nas fases-tutorial, margem mais
apertada depois de ~fase 20).

## Estudo Infinito (automático, via GameManager)

Diferente da Campanha, aqui a dificuldade escala sozinha com a pontuação —
parâmetros no Inspector do `GameManager`:

- **Variedade de peças**: começa com `tiposIniciaisInfinito` (padrão 3) e soma
  +1 tipo a cada `pontosPorEscalonamento` (padrão 500 pontos), até liberar
  todos os 6.
- **Frequência de obstáculos**: a cada peça destruída, chance de
  `chanceBaseDeObstaculo + incrementoChancePorNivel × nível` de nascer um
  obstáculo aleatório (Gelo/Corrente/Caixa — nunca Pedra, que exigiria
  remover uma peça do tabuleiro em pleno jogo infinito).
- **Card de Versículo**: a cada `pontosPorVersiculo` (padrão 1000), sorteia um
  `VerseData` ainda não mostrado no ciclo atual e concede `xpBonusPorVersiculo`.

Esses quatro números (`tiposIniciaisInfinito`, `pontosPorEscalonamento`,
`chanceBaseDeObstaculo`, `incrementoChancePorNivel`) são o principal alavancador
de "sensação de dificuldade" do modo — ajustar via playtesting, não é algo que
dá pra acertar só no papel.
