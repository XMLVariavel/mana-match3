# Biblioteca de artes BoardV2 — Maná

## Direção visual

A biblioteca segue a referência de arte-conceito `MANÁ`: antiguidade bíblica, bronze envelhecido, dourado queimado, azul-marinho profundo, pergaminho claro, madeira escura, luz quente e ilustrações semi-realistas. A paleta de implementação permanece:

| Função | Cor |
|---|---|
| Fundo noturno | `#06121E` |
| Azul-marinho do grid | `#0E1A2B` a `#16283F` |
| Dourado principal | `#F2B642` |
| Bronze escuro | `#8B5A2B` |
| Pergaminho | `#D9AF6E` |
| Texto creme | `#F6F1E2` |
| Teal de destaque | `#1E8C7D` |

## Assets separados

A pasta `Assets/Art/UI/BoardV2/` contém 15 PNGs organizados por responsabilidade. As seis peças estão em `Pecas/`: `peca_pao`, `peca_peixe`, `peca_uva`, `peca_espiga`, `peca_azeite` e `peca_pomba`. Os assets de moldura e painéis ficam em `Frame/` e `Stats/`. O fundo fica em `Background/`, e o fallback visual do banner fica em `Objectives/`.

Os três ícones de poder separados estão em `PowerUps/`: `power_hammer.png`, `power_shuffle.png` e `power_plus5.png`. Eles são medalhões PNG com transparência e acabamento dourado/bronze, utilizados dentro dos botões do rodapé. O importador `ManaArte.cs` reconhece esses três nomes e os configura como sprites de alta resolução.

## Mapeamento na hierarquia

| Hierarquia Unity | Asset ou componente |
|---|---|
| `TelaJogo/BarraSuperior/Fundo` | `card_panel` como painel ornamental |
| `TelaJogo/BarraSuperior/Marca` | TextoMeshPro `MANÁ` com outline |
| `TelaJogo/BarraSuperior/VersiculoBadge` | Pergaminho com `SALMOS 23:1` |
| `TelaJogo/BarraSuperior/CartaoScore` | Card ornamental + três estrelas |
| `TelaJogo/BarraSuperior/CartaoMovimentos` | Card ornamental azul-marinho |
| `TelaJogo/BarraSuperior/CartaoProgresso` | Card ornamental + barra preenchível + meta |
| `TelaJogo/BarraSuperior/Objetivos` | Pergaminho + `ItemObjetivo` com ícone/contador |
| `Tabuleiro/SlotsDasCelulas` | 64 slots azul-marinho atrás das peças |
| `Tabuleiro/MolduraTabuleiro` | Moldura dourada ornamental |
| `Tabuleiro/FundoBiblico` | `fundo_jornada` ampliado para portrait |
| `TelaJogo/BarraInferior/Poderes` | Três medalhões BoardV2 com badge de usos |

## Correções funcionais associadas

A HUD agora reemite o estado depois de assinar os eventos, evitando que objetivos e estatísticas apareçam vazios na primeira abertura. Quando uma fase antiga não possui objetivos, a Campanha recebe metas de demonstração — pão 20, pomba 14 e azeite 18 — enquanto Estudo Infinito e Contra o Relógio continuam sem objetivos fixos. A barra de progresso é atualizada pelo score real e mostra `atual / 20.000`.

## Validação

A `SampleScene.unity` foi remontada e calibrada diretamente no Editor, sendo salva em 21 de agosto de 2026 às 08:39:45, com 1200380 bytes. A composição persistida usa cabeçalho de 420 unidades, faixa de objetivos ampliada, rodapé de 230 unidades, células de 1,12 e tabuleiro deslocado para reduzir o vazio entre HUD e grid. O Unity Editor abriu fora do SAFE MODE e o log recente não apresentou `error CS`, `Compilation failed` ou `Could not register callback class null`. Os scripts temporários usados durante a montagem foram removidos.

A geração adicional por IA atingiu a quota diária disponível durante esta execução. Por isso, a biblioteca usa os assets bíblicos separados já existentes no projeto, os três novos medalhões de poder gerados de forma determinística como fallback profissional e os spritesheets PNG com manifestos JSON, sem inserir imagens compostas na hierarquia funcional. Quando a quota de geração estiver disponível, os mesmos arquivos podem receber uma substituição artística sem alterar os caminhos de integração.

## Teste visual final

Use o Game View em `1080 x 1440` portrait e pressione Play. Confira a presença de logo, versículo, estrelas, objetivos, slots, peças, fundo bíblico e medalhões. Em seguida, teste troca de peças, martelo, embaralhar, +5 movimentos, Campanha, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra.
