
# 🌿 Sage Programming Language: Technical Documentation

**Sage** é uma linguagem de programação compilada (transpilada) que utiliza o C como alvo de baixo nível. Sua arquitetura segue o modelo clássico de compiladores em "pipeline", priorizando a modularidade através do **Design Pattern Visitor**.

## 1. Arquitetura do Compilador

O processo de compilação da Sage é dividido em 6 etapas sequenciais. Cada etapa transforma o código em uma representação mais abstrata ou mais próxima do hardware.

### A. Frontend (Análise)

1. **Lexer (Lexical Analysis):** Quebra a string bruta do código-fonte em unidades significativas chamadas `Tokens`.
2. **Parser (Syntactic Analysis):** Organiza os tokens em uma estrutura de árvore hierárquica chamada **AST (Abstract Syntax Tree)**.

### B. Middle-end (Validação)

3. **Semantic Analyzer:** A "inteligência" do compilador. Valida se as variáveis foram declaradas, se os tipos são compatíveis (Type Checking) e gerencia escopos via `SymbolTable`.

### C. Backend (Síntese)

4. **Code Generator:** Percorre a AST e emite código C equivalente.
5. **Native Compiler (GCC):** O compilador nativo transforma o código C em um binário (.exe) funcional.
6. **Process Executor:** Gerencia a execução do programa final no Sandbox.

## 2. Estrutura do Projeto

* **`/Ast`**: Define a "espinha dorsal" da linguagem. Cada classe representa uma construção (ex: `BinaryExpressionNode`).
* **`/Core`**: Contém a lógica pesada: `Lexer`, `Parser`, `SemanticAnalyzer` e `CodeGenerator`.
* **`/Enums`**: Centraliza os tipos de tokens (`TokenType`).
* **`/Interfaces`**: Define o contrato `IAstVisitor`, permitindo que novas funcionalidades (como um interpretador ou otimizador) sejam adicionadas sem mudar as classes da AST.
* **`/Utilities`**: Ferramentas de diagnóstico como `AstPrinter` e `CompilerLogger`.

## 3. Guia do Desenvolvedor: Adicionando Novos Recursos

Para adicionar um novo recurso (ex: um comando `if`), siga este fluxo padrão:

### Passo 1: O Token

Adicione o tipo do token no arquivo `TokenType.cs`.

```txt
Keyword_If,
Keyword_Else,
```

Em seguida, mapeie no `Lexer.cs` dentro do dicionário `Keywords`.

### Passo 2: O Nó da AST

Crie uma nova classe em `/Ast` (ex: `IfStatementNode.cs`) herdando de `AstNode`. Ela deve armazenar a condição e os blocos de código.

### Passo 3: O Contrato (IAstVisitor)

Adicione o método `Visit` para o seu novo nó na interface `IAstVisitor<T>`. **Nota:** Isso causará erros de compilação em todos os visitantes, o que é bom! Te obriga a implementar a lógica em todo lugar.

### Passo 4: O Parser

No `Parser.cs`, crie um método `ParseIfStatement`. Ele deve consumir os tokens `if`, `(`, a expressão, `)` e o bloco `{}`.

### Passo 5: Semântica e Geração

* No `SemanticAnalyzer.cs`, valide se a condição do `if` resulta em um booleano.
* No `CodeGenerator.cs`, emita o `if` correspondente em C.

## 4. O Sistema de Tipos e Casting

A Sage utiliza um sistema de **Tipagem Estática** com suporte a **Promoção Implícita** e **Casting Explícito**.

* **Promoção:** O compilador permite automaticamente que um `i32` seja tratado como `f64` para evitar erros em operações matemáticas comuns.
* **Casting (`as`):** Permite a conversão forçada. No nível do C, isso é traduzido para um cast de tipo direto `(tipo)valor`.

### SymbolTable (Tabela de Símbolos)

Diferente de uma tabela simples, a `SymbolTable` da Sage usa uma `Stack<Dictionary<string, string>>`.

* Cada dicionário representa um **Escopo** (Global, Função, Bloco).
* Isso permite que você tenha variáveis com o mesmo nome em funções diferentes sem conflitos.

## 5. Convenções de Código (Clean Code & SOLID)

1. **Single Responsibility:** O `Lexer` só conhece texto. O `Parser` só conhece tokens. Não misture as lógicas.
2. **Visitor Pattern:** As classes da AST são "burras" (apenas dados). Toda a lógica de comportamento está nos visitantes (`CodeGenerator`, `SemanticAnalyzer`).
3. **Imutabilidade:** Sempre que possível, use propriedades `get-only` nos nós da AST para garantir que a árvore não seja alterada acidentalmente após o parsing.

## 6. Futuro do Desenvolvimento (Roadmap)

1. **Booleanos e Comparações:** Implementar os tokens `==`, `!=`, `<`, `>` e o tipo `b8`.
2. **Controle de Fluxo:** Implementar `if/else` e loops `while`.
3. **Function Table:** Criar uma tabela global de funções para validar o número de argumentos e tipos de retorno em chamadas entre módulos.
4. **Arrays e Ponteiros:** Suporte básico para coleções de dados.
