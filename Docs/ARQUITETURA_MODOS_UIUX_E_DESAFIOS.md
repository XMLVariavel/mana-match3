# Arquitetura dos modos, UI/UX e desafios competitivos

## 1. Como adaptar a UI/UX do Estudo Infinito

O Estudo Infinito não deve parecer uma Campanha sem objetivos. Ele é uma sessão contínua de aprimoramento, portanto a hierarquia visual precisa destacar **combo, XP, nível de dificuldade, recorde e versículos**, não movimentos ou metas de fase.

A parte superior deve usar o card central com o rótulo `COMBO` e o valor atual em formato `x4`, `x8` ou `x12`. O card direito deve trocar `PROGRESSO` por `XP`, com a barra preenchida até o próximo marco de XP ou pontuação. O card esquerdo continua mostrando `PONTOS` e as três estrelas podem representar marcos de sessão, não uma vitória de fase. O pergaminho de objetivos deve ficar oculto, porque o modo não possui objetivos fixos.

Abaixo do cabeçalho, uma faixa curta pode informar o estado de progressão: `NÍVEL DE ESTUDO 3`, `PRÓXIMO VERSÍCULO EM 240 XP` ou `NOVO TIPO DE PEÇA DESBLOQUEADO`. Essa mensagem deve ser temporária e aparecer como feedback após o marco, sem ocupar permanentemente a área do tabuleiro.

| Área | Informação correta no Estudo Infinito | O que deve desaparecer |
|---|---|---|
| Card esquerdo | Pontos acumulados e recorde da sessão | Resultado de fase tradicional |
| Card central | Combo atual e melhor combo | Movimentos restantes |
| Card direito | XP atual, próximo marco e progresso | Meta fixa de campanha |
| Pergaminho | Oculto | Objetivos Pão/Pomba/Azeite |
| Feedback | Novo nível, XP, versículo e obstáculo | Mensagem de missão concluída |
| Barra de poderes | Martelo e Embaralhar disponíveis conforme economia | `+5 Movimentos` |
| Final da sessão | Resumo de score, XP, melhor combo e versículos | Tela de vitória por estrelas |

O ciclo de uma sessão deve ser: iniciar com poucos tipos de peças; realizar combinações; aumentar o combo; receber XP e versículos em marcos; liberar tipos e obstáculos gradualmente; encerrar somente quando o jogador sair ou ocorrer uma condição futura de encerramento. A tela de resumo deve mostrar `Pontuação`, `Melhor combo`, `XP ganho`, `Versículos encontrados` e `Nível alcançado`.

## 2. Regras recomendadas para pontuação do Contra o Relógio

A implementação atual de pontuação do tabuleiro é simples: uma peça normal destruída vale 10 pontos e uma peça especial destruída vale 30 pontos. Para tornar o Contra o Relógio competitivo, essa base deve ser preservada, mas multiplicada por fatores claramente comunicados ao jogador.

Uma regra equilibrada é calcular cada resolução da seguinte forma:

```text
pontosBase = peçasNormaisDestruídas × 10
           + peçasEspeciaisDestruídas × 30

multiplicadorCombo = min(1 + 0,25 × (combo - 1), 3,0)

bônusVelocidade = 1,20 se a jogada ocorrer em até 2 segundos
                 1,50 se a jogada ocorrer em até 1 segundo
                 1,00 caso contrário

pontosDaJogada = arredondar(pontosBase × multiplicadorCombo × bônusVelocidade)
```

O multiplicador de combo deve começar em 1,00, crescer a cada resolução válida e ter limite de 3,00. O bônus de velocidade deve utilizar o tempo entre jogadas válidas, não o tempo de renderização do frame. Assim, o jogador é recompensado por pensar e agir rapidamente, mas não recebe pontuação artificial por uma atualização gráfica.

Também é recomendável conceder pequenos bônus de sequência, sem permitir que eles dominem a pontuação principal. Por exemplo, `+50` a cada cinco combos consecutivos, `+125` ao alcançar dez combos e `+250` ao alcançar quinze combos. Esses bônus devem aparecer como texto flutuante junto à pontuação recebida.

No fim do Contra o Relógio, pode ser aplicado um bônus final moderado pelo tempo restante, por exemplo `+25 pontos a cada 5 segundos restantes`. Como o tempo restante já incentiva velocidade, esse bônus deve ser limitado e não deve ser aplicado também a cada jogada.

| Componente | Regra recomendada |
|---|---|
| Peça normal | 10 pontos |
| Peça especial | 30 pontos |
| Combo | Multiplicador de 1,00 até 3,00 |
| Jogada em até 2 s | Multiplicador adicional de 1,20 |
| Jogada em até 1 s | Multiplicador adicional de 1,50 |
| Sequência de 5 combos | +50 pontos |
| Sequência de 10 combos | +125 pontos |
| Tempo restante ao final | +25 por bloco de 5 s, com limite |
| Empate no ranking | Maior combo; depois menor tempo de submissão |

Para manter o ranking justo, o uso de poderes deve ter regras explícitas. O Martelo pode custar pontos ou moedas, e o Embaralhar pode aplicar uma pequena penalidade de pontuação. O `+5` não deve existir nesse modo porque altera a regra temporal. Qualquer pontuação enviada ao Firebase deve ser validada também no servidor ou em Cloud Functions; não se deve confiar somente no cliente Unity para ranking competitivo.

## 3. Modificações de código para corrigir o relógio

A correção precisa ocorrer em quatro camadas. A primeira é a regra do modo. `GameManager` deve possuir uma única duração configurável, inicializada ao entrar no Contra o Relógio. O texto visual não deve ter um `90` independente do gameplay.

A segunda camada é o avanço do tempo. O código atual utiliza `Time.deltaTime`. Isso funciona em condições normais, mas pode produzir comportamento incorreto quando o jogo pausa, altera `Time.timeScale`, entra em transição ou fica temporariamente suspenso. A versão robusta deve controlar explicitamente pausa e usar `Time.unscaledDeltaTime` somente quando o desafio deve continuar contando durante a pausa; se a pausa deve congelar o desafio, deve usar um estado `timerPausado` e não decrementar enquanto ele estiver ativo.

Um esqueleto recomendado é:

```csharp
[SerializeField, Min(1f)] private float duracaoContraRelogio = 90f;
private bool timerAtivo;
private bool timerPausado;

public float TempoRestante { get; private set; }
public float DuracaoModoTemporizado => duracaoContraRelogio;

public void IniciarContraRelogio()
{
    ModoAtual = GameMode.ContraRelogio;
    TempoRestante = duracaoContraRelogio;
    timerAtivo = true;
    timerPausado = false;
    scoreManager.Configurar(int.MaxValue, null, 0, 0, 0);
    boardManager.DefinirTiposAtivos(TodosOsTipos());
    boardManager.ReiniciarTabuleiro();
    OnTempoAlterado?.Invoke(TempoRestante);
}

private void Update()
{
    if (!timerAtivo || timerPausado || scoreManager == null || scoreManager.LevelEnded)
        return;

    TempoRestante = Mathf.Max(0f, TempoRestante - Time.unscaledDeltaTime);
    OnTempoAlterado?.Invoke(TempoRestante);

    if (TempoRestante <= 0f)
    {
        timerAtivo = false;
        scoreManager.EncerrarPorTempo();
    }
}
```

A terceira camada é a regra de movimentos. O `BoardManager` não deve chamar `UseMove()` no Contra o Relógio nem no Estudo Infinito. A decisão precisa ser baseada no modo atual, e não apenas no valor `int.MaxValue`, porque depender do número pode esconder erros de configuração.

A quarta camada é a HUD. Ao entrar na tela, `GameHUDController.EmitirEstadoAtual()` deve emitir imediatamente o tempo atual. `GameHUDView` deve alterar o título do card para `TEMPO`, atualizar o texto em `mm:ss`, preencher a barra com `tempoRestante / duracaoTotal` e ocultar o pergaminho de objetivos e o `+5`. Ao chegar a zero, o evento de derrota deve ser emitido uma única vez.

Os testes mínimos devem verificar: início em 90 segundos; duração configurável; decremento após um intervalo; preenchimento correto da barra; nenhum movimento consumido; derrota exatamente ao chegar a zero; impossibilidade de disparar derrota duas vezes; e restauração correta da HUD ao entrar novamente em outro modo.

## 4. Melhorias competitivas por modo

### Campanha

A Campanha deve competir por eficiência, não apenas por conclusão. O ranking pode comparar estrelas, pontuação, movimentos restantes e quantidade de obstáculos removidos. Cada fase deve ter medalhas de desempenho, objetivos secundários e desafios opcionais, como concluir sem usar poderes ou terminar com pelo menos dez movimentos restantes.

### Estudo Infinito

O Estudo Infinito deve ter recordes separados por pontuação, melhor combo, maior nível alcançado e maior quantidade de versículos coletados. Para evitar sessões monótonas, a cada marco o jogo pode introduzir uma mutação controlada: mais obstáculos, peças especiais obrigatórias, tabuleiro parcialmente bloqueado ou uma janela curta de bônus.

### Desafio Diário

O Diário deve usar a mesma semente e os mesmos modificadores para todos os jogadores. O ranking deve ser diário e também manter uma sequência semanal. Recomenda-se mostrar a posição do jogador, a melhor pontuação de amigos, a pontuação necessária para entrar no top 10 e o número de tentativas permitidas. Para ser competitivo, o desafio não deve gerar um tabuleiro diferente a cada tentativa do mesmo dia.

### Contra o Relógio

O Relógio deve destacar velocidade, combo e precisão. Trocas inválidas podem consumir uma pequena fração de tempo, por exemplo 0,5 segundo, mas isso deve ser comunicado antes da partida. Outra opção mais acessível é não punir a troca inválida e simplesmente fazer o jogador perder tempo real. O ranking deve usar a pontuação final, melhor combo e, em caso de empate, o horário de submissão validado.

### Guardião da Palavra

O Guardião pode evoluir para objetivos em camadas. Primeiro, o jogador coleta Pão e Peixe; depois precisa proteger uma sequência, remover obstáculos específicos ou completar um objetivo antes de outro. O ranking pode considerar movimentos restantes, objetivos extras e poderes não utilizados. A interface deve mostrar sempre qual objetivo está em risco e qual peça tem prioridade.

## 5. Desafios variados ou fixos

A melhor solução não é escolher entre tudo fixo ou tudo aleatório. O jogo deve usar três níveis de controle.

A **Campanha** deve ser majoritariamente fixa e criada manualmente, porque precisa contar uma jornada bíblica com dificuldade e narrativa planejadas. O **Desafio Diário** deve ser fixo durante o dia, mas variar de um dia para outro por meio de uma semente determinística. O **Contra o Relógio** pode manter a duração fixa e variar o modificador, o layout inicial ou a condição de bônus em ciclos semanais. O **Estudo Infinito** deve ser procedural e variar progressivamente durante a sessão. O **Guardião da Palavra** pode usar uma estrutura fixa de objetivos com variações semanais.

| Camada | Exemplo | Determinismo |
|---|---|---|
| Campanha | Fase com obstáculos e metas autorais | Fixo |
| Diário | Semente `ano-mês-dia` e missão do dia | Igual para todos no dia |
| Relógio | 90 s mais modificador semanal | Regra fixa, contexto variável |
| Infinito | Dificuldade e tipos de peça por marcos | Procedural controlado |
| Guardião | Objetivos bíblicos e variação de prioridade | Estrutura fixa, variantes rotativas |

O fluxo recomendado para qualquer desafio é: o mapa apresenta um cartão de briefing; o jogador vê regra, duração, movimentos, objetivos, bônus e restrições; o jogo gera ou carrega a configuração; a partida registra score, combo, tempo, poderes e objetivos; ao finalizar, uma tela de resultado mostra a pontuação detalhada; por fim, o resultado é enviado ao ranking com o identificador do modo e da temporada.

Uma futura implementação pode separar as regras em `ModeRules` ou `ChallengeDefinition` como `ScriptableObject`. O `GameManager` carregaria a definição, o `GameHUDView` leria suas propriedades e o ranking receberia `modeId`, `challengeId`, `seed`, `score`, `bestCombo` e `duration`. Isso evita espalhar números fixos em vários scripts e permite criar novos desafios sem duplicar a cena.

## Conclusão

O Estudo Infinito deve parecer uma sessão de evolução e descoberta; o Contra o Relógio deve parecer uma prova de velocidade; o Diário deve ser uma competição igual para todos; a Campanha deve ser uma jornada autoral; e o Guardião deve ser um desafio de planejamento por objetivos. A interface precisa comunicar essas diferenças antes mesmo da primeira jogada, porque uma HUD genérica faz modos diferentes parecerem o mesmo jogo.
