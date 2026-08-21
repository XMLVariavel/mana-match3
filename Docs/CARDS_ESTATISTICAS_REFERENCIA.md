# Cards de estatísticas — referência aplicada

Os três cards superiores da tela de jogo foram reorganizados conforme a imagem fornecida pelo usuário.

## Pontos

O card `CartaoScore` usa painel azul-marinho com moldura dourada, mostra o rótulo `PONTOS`, o número do score em destaque e três estrelas posicionadas imediatamente abaixo do número. As estrelas são douradas e ficam dentro de uma faixa inferior própria.

## Movimentos

O card `CartaoMovimentos` mostra o rótulo `MOVIMENTOS` e apenas o número de jogadas disponível. O texto de XP não aparece neste card e nenhum indicador adicional foi adicionado.

## Progresso

O card `CartaoProgresso` usa o mesmo painel azul-marinho dourado, mostra o rótulo `PROGRESSO`, uma barra horizontal preenchível, uma estrela dourada ao lado e o texto de meta no formato `atual / 20.000`. O texto `XP 0` foi ocultado para evitar duplicação visual e deixar a leitura igual à referência.

## Implementação

Foi criado o asset `Assets/Art/UI/BoardV2/Stats/stat_card_panel.png` a partir do painel fornecido, com fundo branco removido e transparência preparada para uso como painel 9-slice. O importador `ManaArte.cs` reconhece o painel e o helper `ManaUI.PainelEstatistica()` é usado nos três cards. A montagem está em `MontadorDeUI.cs`.

## Validação

A `SampleScene.unity` foi remontada e salva às 09:48:04 de 21 de agosto de 2026, com 1204224 bytes. A cena contém `CartaoScore`, `CartaoMovimentos`, `CartaoProgresso`, `EstrelaProgresso` e `MetaProgresso`. O log recente do Unity não apresentou `SAFE MODE`, `error CS`, `Compilation failed` ou `Could not register callback class null`.
