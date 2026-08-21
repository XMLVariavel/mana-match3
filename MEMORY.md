# Memória de implementação

## Descobertas

A captura inicial mostrava sprites de pão ocupando quase quatro células. A causa foi confirmada no importador: o PNG tinha área útil aproximada de 887x971px, `spritePixelsPerUnit = 256` e o tabuleiro usava `cellSize = 1`. A correção foi implementada em runtime para ser robusta mesmo se outros PNGs tiverem dimensões diferentes.

O projeto possui um montador de cena por código no menu `Tools > Maná`, portanto a alteração de UI foi feita no Editor script e não diretamente no YAML da cena. Isso mantém a estratégia original de gerar referências pela própria Unity.

## Limitações encontradas

O executável Unity CLI instalado no PATH não é o Editor; o Editor correto existe em `F:\Nova pasta\6000.5.8f1\Editor\Unity.exe`. A execução em batch alcançou o carregamento do projeto, mas terminou com código 1 antes de executar o método do montador, com o log indicando falha de licenciamento/entitlement do ambiente. A checagem externa via `dotnet build` também não foi possível porque não há .NET SDK instalado.

O projeto ainda precisa ser aberto normalmente no Unity Hub/Editor pelo usuário para executar `Montar cena completa`, salvar a cena e confirmar o Play Mode. O log anterior do projeto contém erros de login anônimo do Firebase e ausência de configuração de anúncios, que são integrações externas e não fazem parte da correção visual.

## Próxima verificação prioritária

Abrir a cena no Editor licenciado, rodar `Tools > Maná > Montar cena completa`, salvar e testar em resolução retrato. Se aparecer algum erro de compilação, corrigir primeiro o script apontado pelo Console; se a cena abrir, validar visualmente escala, moldura, fundo, cabeçalho e os dois modos.

## Segunda rodada — bugs reportados

A captura do Unity revelou três causas concretas. A câmera ainda estava com `orthographicSize = 8`, o que deixava a moldura pequena no Game View paisagem; foi criado `AdaptiveBoardCamera`, com instalação automática em cenas já montadas, e o valor de preview foi ajustado para 4.8.

Os assets `EfeitoMartelo.asset`, `EfeitoEmbaralhar.asset` e `EfeitoMaisMovimentos.asset` tinham `m_Script: {fileID: 0}`. Os YAMLs foram corrigidos com o GUID de `PoderAvulsoSO.cs`, o Martelo foi marcado com `requerAlvo: 1` e o gerador do Editor agora recria assets inválidos e reassocia efeitos sempre que a cena é remontada. `BoosterManager` também possui fallback temporário para uma sessão que ainda não reimportou os assets.

A entrada do tabuleiro usava `Physics2D.Raycast` com direção zero e somente o Input Manager legado. A entrada foi substituída por `Physics2D.OverlapPointAll`, suporte explícito a `UnityEngine.InputSystem`, toque de duas peças vizinhas, arrasto, destaque visual da seleção e requisito automático de `BoxCollider2D`.

A camada de profissionalização adicionada inclui limite e intervalo de obstáculos no Estudo Infinito, feedback de poderes na HUD, custos visíveis, mensagens na loja, sanitização de progresso antes do Firebase/leaderboard e perfil global de desempenho a 60 FPS. O teste em Play Mode continua dependendo do Unity Editor licenciado aberto normalmente.
