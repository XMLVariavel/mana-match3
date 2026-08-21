# Estrutura do projeto Maná

## Runtime do jogo

A lógica de tabuleiro permanece em `Assets/Scripts/`. `BoardManager` controla a grade, entrada e troca; `MatchDetector` identifica combinações; `BoardPhysics` resolve destruição, queda, reabastecimento e cascatas; `ObstacleManager` mantém a grade paralela de obstáculos; `ScoreAndObjectiveManager` controla score, movimentos e objetivos; `GameManager` inicia Campanha ou Estudo Infinito; `GameHUDController` conecta o resultado do jogo aos eventos da interface.

O `Tile` é responsável pelos próprios dados e pela apresentação do sprite. A normalização de escala foi colocada nele porque todos os caminhos de criação e reuso passam por `Setup`, e `FitToCell` pode ser aplicado tanto a peças comuns quanto a especiais. O collider permanece logicamente equivalente a uma célula, mesmo quando o desenho ocupa somente 82% dela.

## UI

Os controllers não recebem referências diretas a textos e botões. As views em `Assets/Scripts/UI/` continuam escutando eventos C# e desenhando o estado. A HUD agora expõe uma etiqueta de modo e reemite o estado inicial do score/objectivos ao ser ativada.

`ScreenNavigator` continua controlando a troca entre as nove telas. A distinção entre os modos é feita pelo `MapaDeFasesController` na entrada e pelo `GameHUDView` durante a partida.

## Editor

`Assets/Editor/MontadorDeUI.cs` é o ponto seguro para reconstruir a cena, `ManaPrefabs.cs` gera prefabs e `ManaArte.cs` prepara os PNGs. A nova arte de fundo e a moldura do tabuleiro são integradas pelo montador, evitando referências YAML escritas manualmente.

## Arte

Os sprites de peças ficam em `Assets/Art/Placeholder/` para preservar os nomes e GUIDs esperados pelo montador. As versões antigas estão em `backup_original/`. A arte de interface fica em `Assets/Art/UI/`.
