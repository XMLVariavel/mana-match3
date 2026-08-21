# Plano de melhoria do Maná

## Objetivo

Transformar a base funcional do match-3 em uma experiência bíblica visualmente consistente, legível em mobile e com distinção clara entre Campanha e Estudo Infinito.

## Implementado nesta rodada

A camada visual das peças foi normalizada por célula. O `Tile` calcula uma escala máxima de 82% da célula usando os limites reais do sprite e compensa o `BoxCollider2D` para que a área de toque continue cobrindo 1x1 unidade. A aplicação ocorre na criação inicial, no embaralhamento, no reabastecimento e na criação de especiais.

A troca de modo agora reconstrói o tabuleiro e limpa obstáculos anteriores. O Estudo Infinito passa a iniciar efetivamente com a quantidade inicial de tipos configurada; a Campanha volta a iniciar sem resíduos da sessão anterior.

A HUD passa a sincronizar score, movimentos e objetivos quando a tela é ativada depois da configuração da fase. Também identifica visualmente o modo atual com `CAMPANHA • OBJETIVOS DA FASE` ou `ESTUDO INFINITO • PONTOS + XP`.

A direção visual foi atualizada para azul-noturno, pergaminho, madeira e dourado. O montador passou a usar `fundo_jornada.png` nas telas com fundo e `moldura_tabuleiro.png` atrás das peças. Os seis sprites principais foram substituídos por arte bíblica refinada e os placeholders antigos foram preservados em `Assets/Art/Placeholder/backup_original/`.

## Verificação no Editor

1. Abrir `Assets/Scenes/SampleScene.unity` no Unity 6000.5.8f1.
2. Executar `Tools > Maná > Montar cena completa` com a cena aberta.
3. Salvar a cena com `Ctrl+S`.
4. Confirmar que o console não apresenta erros de compilação.
5. Entrar em uma fase de Campanha e conferir que as peças ocupam menos que a célula, os objetivos aparecem e a moldura fica atrás do tabuleiro.
6. Voltar ao mapa, entrar em Estudo Infinito e conferir a etiqueta do modo, o símbolo `∞`, o XP e a ausência de objetivos fixos.
7. Testar um match, uma cascata, um especial, o Martelo, o Embaralhar, `+5 Mov.` e o retorno ao mapa.

## Critérios de aceite visual

O tabuleiro deve permanecer legível em retrato, sem sobreposição entre vizinhos. Cada peça deve ser reconhecida por cor e silhueta. Os botões de ação devem ter respiro e contraste, o cabeçalho deve apresentar hierarquia e as telas de navegação devem abandonar o fundo marrom chapado em favor da arte de jornada.
