# Revisão geral de navegação, perfil e desafios

## Navegação

A antiga tela intitulada `JORNADA • NÍVEIS` foi renomeada para `MODOS DE JOGO`, porque reúne cinco experiências diferentes: Campanha, Estudo Infinito, Desafio Diário, Contra o Relógio e Guardião da Palavra.

A navegação agora possui três destinos distintos. `Início` é o hub inicial, com explicação curta, acesso aos modos, acesso aos desafios e entrada direta em Opções. `Jornada` abre a tela `MODOS DE JOGO`, onde ficam Campanha e Estudo Infinito junto da visão geral dos cinco modos. `Desafios` abre uma tela própria com Desafio Diário, Contra o Relógio e Guardião da Palavra. Loja, Perfil e Opções permanecem acessíveis pela navegação inferior.

## Perfil

O nome agora é salvo localmente em `PlayerPrefs` e também enviado ao `FirebaseManager` quando o progresso está disponível. O controller reemite o perfil imediatamente após salvar para atualizar o campo, a mensagem e o placar sem exigir reabertura da tela.

O avatar selecionado também é salvo localmente e sincronizado no progresso. Os cinco retratos foram copiados para `Assets/Resources/Avatars`, corrigindo o carregamento em runtime usado pelo `AvatarPickerView`.

## Opções

A tela `Opções` já existia, mas não estava acessível pela navegação principal. Foi adicionado um botão `Opções` à navegação inferior e também um botão destacado na tela `Início`. A tela preserva controles de música, efeitos, vibração, vínculo de conta, anúncios, exportação e exclusão de dados.

## Desafios

A rota `Desafios` deixou de apontar para a mesma tela da Jornada. Ela agora abre uma tela dedicada com três cards clicáveis. Cada card chama diretamente o método correspondente do `MapaDeFasesController`, corrigindo o fluxo de entrada para o Desafio Diário, Contra o Relógio e Guardião da Palavra.

## Arte e responsividade

O fundo celestial fornecido foi incorporado como `Assets/Art/UI/fundo_celestial.png`, registrado no `ManaArte` e usado pelo `PainelIlustrado` como fundo padrão das telas de navegação, com `fundo_jornada` como fallback. A imagem é esticada pelo Canvas responsivo, sem alterar o tabuleiro ou o cabeçalho específico da tela de jogo.

## Validação

A cena foi remontada em 21 de agosto de 2026 às 15:17:54, com 1.483.979 bytes. O log batch registrou `Cena remontada automaticamente`, `Montagem batch concluída e SampleScene salva` e `Exiting batchmode successfully now`, sem `error CS`, `Compilation failed` ou exceção do montador.

A validação estrutural confirmou as telas `Inicio`, `MapaDeFases`, `Desafios`, `Perfil` e `Configuracoes`, além de `BotaoOpcoes`, `BotaoSalvarNome`, `InstrucoesPoderes` e o asset `fundo_celestial.png`.

## Roteiro de teste manual

Após abrir o jogo, confirme que o splash leva para `Início`. Toque em `MODOS DE JOGO` e confirme a abertura da tela com os cinco modos. Toque em `Desafios` e abra cada um dos três cards. Na navegação inferior, abra `Opções`, altere música, efeitos e vibração e confirme que os controles continuam acessíveis. No Perfil, troque o avatar, digite um nome com pelo menos dois caracteres, salve e saia/retorne à tela para verificar a persistência. Por fim, confira o fundo celestial em Início, Modos de jogo, Desafios, Perfil e Opções em portrait 1080x1440 e em uma tela mais estreita.
