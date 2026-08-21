# Validação da rodada

## Validações concluídas

Os scripts modificados foram verificados estaticamente e as chaves de bloco estão balanceadas nos arquivos alterados. Os assets refinados foram copiados para `Assets/Art/Placeholder/`, enquanto as versões antigas permanecem em `backup_original/`. O fundo de jornada e a moldura do tabuleiro estão em `Assets/Art/UI/`.

Foi confirmada a presença dos símbolos de integração `CellSize`, `ClearAll`, `EmitCurrentState`, `textoModo` e `MolduraTabuleiro`. O fluxo de criação aplica `FitToCell` na geração, no embaralhamento, na criação de especiais e no reabastecimento.

## Validação pendente no Editor

A execução em batch do Unity 6000.5.8f1 carregou o projeto, mas terminou com código 1 antes de executar o método do montador, em razão de entitlement/licenciamento do ambiente batch. A compilação externa também não pôde ser executada porque o computador conectado não possui o .NET SDK.

A validação final deve ser feita com o Unity Editor aberto normalmente e com licença ativa. O procedimento está detalhado em `PLAN.md`: rodar `Tools > Maná > Montar cena completa`, salvar a cena e testar Campanha, Estudo Infinito, cascata, especiais e poderes avulsos.

Os erros de Firebase e de configuração de anúncios observados no log são integrações externas existentes e não foram alterados nesta rodada.
