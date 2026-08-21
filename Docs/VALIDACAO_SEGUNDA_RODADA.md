# Validação da segunda rodada

## Correções verificadas por inspeção e compilação incremental

A cena reportada apresentava enquadramento pequeno porque a câmera estava fixa em `orthographicSize = 8`; agora existe `AdaptiveBoardCamera`, que calcula o tamanho pela largura disponível e instala-se automaticamente na câmera principal durante o carregamento da cena. O montador usa 4.8 como preview confortável no Game View paisagem.

Os poderes avulsos tinham assets com `m_Script: {fileID: 0}`. Os três YAMLs foram corrigidos com o GUID de `PoderAvulsoSO.cs`; o Martelo agora exige alvo. O gerador `ManaAssets` também detecta e recria assets inválidos. Enquanto a reimportação não ocorre, `BoosterManager` recupera os efeitos conhecidos pelo nome para evitar uma sessão inutilizável.

O clique no pão e nas outras peças agora usa `UnityEngine.InputSystem`, com fallback legado, detecção por `Physics2D.OverlapPointAll`, suporte a toque de duas peças vizinhas, arrasto e destaque visual da peça selecionada. O prefab exige `SpriteRenderer` e `BoxCollider2D`.

A camada de produto recebeu limites de obstáculos no Estudo Infinito, mensagens visíveis para poderes, custos no rodapé, feedback de loja, sanitização de progresso antes do Firebase/leaderboard e modo offline controlado quando o login Firebase falhar.

## Teste manual obrigatório no Unity

Com o Unity 6000.5.8f1 licenciado aberto, aguarde o término da compilação e confirme que o Console não tem `error CS`. Execute `Tools > Maná > Montar cena completa`, salve `Assets/Scenes/SampleScene.unity` e pressione Play.

Na partida, use Game View em `9:16` para conferir o alvo visual portrait e, depois, `16:9` para conferir o enquadramento paisagem. A moldura deve ocupar a maior parte da área útil, sem deixar o tabuleiro minúsculo. Clique uma peça pão e depois em uma vizinha: a primeira deve ficar destacada e a troca deve ocorrer se formar combinação; um arrasto horizontal/vertical também deve funcionar.

Teste o Martelo tocando primeiro no botão e depois em qualquer peça. Teste Embaralhar e +5 Movimentos, observando que não aparece mais o aviso `PowerUpConfig inválido ou não é um poder avulso`; se não houver moedas, a HUD deve explicar o custo. Entre em Campanha e Estudo Infinito e confirme que as etiquetas e regras são diferentes. Abra Loja, Perfil, Placar e Opções para confirmar que a navegação e as mensagens continuam funcionando.

## Limitações conhecidas

A execução automatizada em batch não conseguiu concluir o método do Editor por restrição de entitlement/licenciamento do ambiente. O log possui falha de autenticação Firebase, agora tratada como modo offline no runtime; isso não impede jogar localmente, mas impede ranking e sincronização até o Firebase estar configurado e acessível.
