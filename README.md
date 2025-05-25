# TP\_TDV\_02 — Bratalian

Este trabalho prático consiste na análise e apresentação da implementação do jogo 2D **Bratalian2**, utilizando as tecnologias disponibilizadas no repositório Git.

## Descrição Geral

Recriação de um jogo de captura e batalha de criaturas em 2D, inspirado em RPGs de monstros. O mapa é gerado de forma procedural, com várias zonas delimitadas, caminhos e vegetação (“bushes”) posicionados aleatoriamente. A colisão com uma criatura inimiga (“Bratalian”) inicia um combate por turnos, onde cada criatura dispõe de ataques com poder e precisão distintos.

## Controlo

* **Teclas de setas**: mover o jogador no mapa ou navegar nos menus de batalha
* **Enter**: confirmar ação (iniciar combate, escolher ataque)

## Estrutura de Código e Descrição das Classes

### Ficheiro Game1.cs

Classe principal que gere o ciclo de vida do jogo (modos Exploração e Batalha)

* **Initialize()**: inicializa variáveis e componentes do jogo
* **LoadContent()**: carrega recursos (texturas, fontes, sons)
* **Update(GameTime gameTime)**: atualiza a lógica do jogo a cada frame (input, deslocação, deteção de encontros)
* **Draw(GameTime gameTime)**: desenha o mundo, as criaturas e a interface gráfica

### Ficheiro Bratalian.cs

Representa uma criatura

* **Propriedades**: Nome, Vida, Energia, Lista de ataques
* **Métodos**:

  * Construtor: define estatísticas iniciais
  * **TakeDamage(int valor)**: aplica dano recebido
  * **IsFainted()**: verifica se a criatura ficou inconsciente

### Ficheiro Attack.cs

Define um ataque disponível para as criaturas

* **Propriedades**: Nome, Poder (quantidade de dano), Precisão (percentagem)
* **Métodos**: cálculo de dano e verificação de acerto com base em valores aleatórios

### Ficheiro MapGenerator.cs

Gera o mapa do jogo de forma procedural

* **Generate(MapZone\[] zonas)**: distribui zonas, caminhos e obstáculos de vegetação
* **CreateBushes()**: posiciona elementos de vegetação em locais aleatórios

### Ficheiro MapZone.cs

Representa uma zona do mapa com limites e características

* **Propriedades**: Posição, Dimensões, Tipo de terreno

### Ficheiro Camera2D.cs

Controla a visualização e o scrolling do ecrã

* **Métodos**:

  * **Update(Vector2 targetPosition)**: centra a câmara na posição do jogador
  * **GetViewMatrix()**: devolve a matriz de visualização para ser usada no `SpriteBatch`

### Directoria Content/

* `Content.mgcb`: pipeline MGCB para processamento de assets
* `Textures/`: gráficos PNG (jogador, inimigos, terreno)
* `Fonts/`: ficheiros .spritefont para renderização de texto

## Funcionamento Interno do Jogo

1. No modo **Exploração**, o jogador desloca-se pelo mapa com base na entrada do teclado.
2. Ao colidir com um inimigo, passa-se para o modo **Batalha**.
3. Em cada turno de batalha, utiliza-se as setas para escolher um ataque e Enter para confirmar.
4. O sistema calcula o resultado do ataque (considera poder, precisão e aleatoriedade) e atualiza a vida e energia das criaturas.
5. Se o inimigo for derrotado, regressa-se ao modo Exploração; se o jogador perder, reinicia-se a zona atual.

## Sugestões de Melhorias Futuras

* Implementar sistema de inventário e objetos de batalha
* Evolução e níveis das criaturas
* Menus de pausa, início e fim de jogo
* Guardar/carregar progresso do jogador
* Adicionar efeitos sonoros e música de fundo

## Autores

* João Guilherme Antunes Faria — nº 24903
* Márcio José da Cunha Cardoso — nº 24888

2025
