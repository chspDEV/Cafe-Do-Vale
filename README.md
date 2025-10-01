# Café do Vale

![Game Banner](https://via.placeholder.com/1280x300.png?text=Café+do+Vale)

## 1. Visão Geral

**Café do Vale** é um jogo de gerenciamento casual para Computador, onde o jogador transforma sua fazenda no coração do Vale do Café em uma cafeteria de sucesso. A jogabilidade foca na autêntica jornada do grão à xícara: produzir, beneficiar e atender clientes com bebidas especiais, enquanto busca alcançar a cobiçada classificação de 5 estrelas.

* **Motor:** Unity 6
* **Plataforma:** PC (Windows/Mac/Linux)
* **Gênero:** Gerenciamento, Simulação, Casual
* **Status Atual:** Em Desenvolvimento

---

## 2. Configuração do Ambiente (Getting Started)

Este guia destina-se a desenvolvedores internos para configurar o ambiente de desenvolvimento.

### 2.1. Pré-requisitos

Antes de começar, garanta que você tenha os seguintes softwares instalados:

1.  **Unity Hub:** Para gerenciar as versões do editor.
2.  **Unity Editor:** O projeto utiliza a versão **6000.0.23f1**. É crucial instalar esta versão exata através do Unity Hub para evitar inconsistências.
3.  **Git:** O Git é necessário para clonar o repositório e para que o Unity Package Manager resolva dependências de pacotes.

### 2.2. Passos para Instalação

1.  **Clonar o Repositório:**
    Abra um terminal ou cliente Git e clone o projeto em seu ambiente local.

    ```bash
    git clone [https://github.com/chspDEV/Cafe-Do-Vale.git](https://github.com/chspDEV/Cafe-Do-Vale.git)
    ```

2.  **Abrir no Unity:**
    * Abra o Unity Hub.
    * Clique em "Open" > "Add project from disk".
    * Navegue até a pasta onde você clonou o projeto e selecione-a.
    * O Unity irá iniciar, importando o projeto e resolvendo todos os pacotes.

O projeto deve abrir sem erros de compilação.

---

## 3. Detalhes Técnicos

### 3.1. Linguagens

A base de código do projeto é predominantemente escrita em C#, seguindo os padrões e a API da Unity.

* **C#:** 98%+
* **Outros:** (ShaderLab, Assembly, etc.) <2%

### 3.2. Dependências e Pacotes Chave

O projeto utiliza diversos pacotes do Unity e assets de terceiros. A lista completa de pacotes está no arquivo `Packages/manifest.json`. Os mais importantes são:

* **Renderização:** Universal Render Pipeline (URP)
* **Sistema de Input:** Input System
* **Câmera:** Cinemachine
* **Assets Externos Notáveis:** TextMesh Pro, Fantasy Skybox FREE, Hierarchy Verpha.

---

## 4. Arquitetura de Software

O projeto utiliza uma combinação de padrões de arquitetura.

### 4.1. Padrão Singleton
Sistemas mais antigos e alguns managers globais utilizam o padrão Singleton para acesso facilitado. A criação de novos singletons deve ser evitada em favor de abordagens mais desacopladas.

### 4.2. Game Events com Scriptable Objects
A arquitetura principal para novos sistemas é orientada a eventos, promovendo baixo acoplamento.

* **Lógica dos Eventos:** O código que define a arquitetura dos eventos está localizado em `Assets/Resources/Scripts/GameEventArchitecture`.
* **Assets de Dados (SOs):** Os Scriptable Objects que funcionam como banco de dados do jogo (itens, diálogos, missões) estão em `Assets/Resources/Database`.
* **Assets de Eventos:** Os Scriptable Objects de eventos específicos estão em `Assets/Resources/Database/_GameEvents`.

### 4.3. Arquitetura Baseada na Pasta `Resources`
O projeto centraliza a vasta maioria de seus assets na pasta `Resources`. Isso significa que os assets são carregados dinamicamente em tempo de execução via código (`Resources.Load<T>()`). Embora facilite o acesso desacoplado aos assets, é crucial estar ciente das implicações de performance e gerenciamento de memória que essa abordagem acarreta.

---

## 5. Estrutura de Pastas (`Assets/`)

A organização de arquivos é fortemente centralizada na pasta `Resources`. A estrutura abaixo detalha os diretórios mais importantes.

```C#
Assets/
├── Plugins/                # Plugins e dependências de terceiros.
│
├── Resources/              # NÚCLEO DO PROJETO: Contém a maioria dos assets carregados dinamicamente.
│   ├── Art/                # Todos os assets visuais.
│   │   ├── Anims, Fonts, Materials, Models, Sprites, VFX, etc.
│   ├── Database/           # Scriptable Objects que funcionam como "banco de dados" do jogo.
│   │   ├── _GameEvents, AlmanaqueSO, DrinksSO, QuestSO, etc.
│   ├── Prefab/             # GameObjects pré-configurados.
│   │   ├── Almanaque, Areas, Minigame, UI, etc.
│   ├── Scripts/            # Código-fonte C# do projeto.
│   │   ├── Characters, GameEventArchitecture, Managers, Systems, UI, etc.
│   └── Sound/              # Assets de áudio.
│       ├── Music, SFX
│
├── Scenes/                 # Cenas do jogo (MainMenu, Gameplay, etc.).
│
├── Settings/               # Arquivos de configuração da Unity (URP, Input System, etc.).
│
└── ... (Outras pastas de assets de terceiros como Fantasy Skybox, TextMesh Pro, etc.)
```


---

## 6. Versionamento e Colaboração (Git Workflow)

Utilizamos um fluxo de trabalho customizado para separar o desenvolvimento de código e a integração de arte.

### 6.1. Branches Principais

* `development`: A branch principal de desenvolvimento. Contém a versão mais atual e estável do projeto.
* `art`: A branch de trabalho para artistas. Todos os novos assets visuais devem ser enviados para esta branch.

### 6.2. Fluxo de Trabalho

* **Desenvolvedores (Programação):**
    1.  Crie uma nova branch a partir de `development` (ex: `feature/nome-da-feature`).
    2.  Implemente a funcionalidade.
    3.  Faça um Pull Request (PR) de volta para a `development`.

* **Artistas (Assets):**
    1.  Trabalhe sempre na branch `art`.
    2.  Quando um conjunto de assets estiver pronto para ser integrado, crie um **Pull Request (PR)** da branch `art` para a `development`.

**IMPORTANTE:** Nunca faça push diretamente para a branch `development`. Todo os assets devem passar por um Pull Request.
