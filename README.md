# Design Patterns em C#

Catálogo vivo dos 23 padrões do GoF (*Gang of Four*) implementados em C# moderno, com cenários realistas em vez de "Animal / Cachorro / Gato".

Cada padrão segue a mesma receita:

1. **Código ruim** (`Bad/`): a solução ingênua, para sentir a dor que o padrão resolve.
2. **Código bom** (`Good/`): a mesma funcionalidade aplicando o padrão.
3. **Testes xUnit**: provam o comportamento e garantem que a refatoração não mudou o resultado.
4. **README do padrão**: intenção, estrutura, quando usar, quando **não** usar e como o C# já traz isso embutido.

## Como rodar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download) (se usar outra versão, ajuste `TargetFramework` em `Directory.Build.props`).

```bash
dotnet restore
dotnet build
dotnet test
```

## Estrutura

```
.
├── docs/
│   └── PATTERN_TEMPLATE.md        # modelo de README para cada novo padrão
├── src/DesignPatterns/
│   ├── Creational/
│   ├── Structural/
│   │   └── Decorator/
│   └── Behavioral/
│       └── Strategy/
│           ├── Bad/               # versão sem o padrão
│           ├── Good/              # versão com o padrão
│           └── README.md
└── tests/DesignPatterns.Tests/    # espelha a estrutura de src/
```

## Índice de padrões

Legenda: ✅ pronto · ⬜ a fazer

### Criacionais

| Padrão | Cenário sugerido | Status |
|---|---|---|
| Factory Method | Criar notificações (e-mail, SMS, push) | ⬜ |
| Abstract Factory | Famílias de provedores (storage + fila) por nuvem | ⬜ |
| Builder | Montar um pedido com muitas opções | ⬜ |
| Prototype | Clonar modelos de documento | ⬜ |
| Singleton | Configuração global (e por que preferir DI) | ⬜ |

### Estruturais

| Padrão | Cenário sugerido | Status |
|---|---|---|
| Adapter | Integrar gateway de pagamento de terceiros | ⬜ |
| Bridge | Tipos de notificação × canais de envio | ⬜ |
| Composite | Combos e kits dentro de um carrinho | ⬜ |
| [Decorator](src/DesignPatterns/Structural/Decorator/README.md) | Logging e cache em cima de um repositório | ✅ |
| Facade | Checkout simplificado sobre vários serviços | ⬜ |
| Flyweight | Compartilhar estilos/ícones em grande volume | ⬜ |
| Proxy | Carregamento preguiçoso e controle de acesso | ⬜ |

### Comportamentais

| Padrão | Cenário sugerido | Status |
|---|---|---|
| Chain of Responsibility | Pipeline de validação de pedido | ⬜ |
| Command | Ações enfileiradas e desfazíveis | ⬜ |
| Interpreter | Mini-linguagem de regras de desconto | ⬜ |
| Iterator | Paginação com `yield return` | ⬜ |
| Mediator | Handlers de requisição (estilo MediatR) | ⬜ |
| Memento | Desfazer em um editor | ⬜ |
| Observer | Eventos de pedido (estoque, e-mail, financeiro) | ⬜ |
| State | Ciclo de vida do pedido (criado → pago → enviado) | ⬜ |
| [Strategy](src/DesignPatterns/Behavioral/Strategy/README.md) | Cálculo de frete | ✅ |
| Template Method | Importadores de arquivo (CSV, JSON) com passos fixos | ⬜ |
| Visitor | Relatórios sobre uma árvore de itens | ⬜ |

## Ordem de estudo sugerida

1. **Base do dia a dia:** Strategy, Factory Method, Decorator, Observer
2. **Segundo nível:** Builder, Adapter, Facade, Command, State
3. **Avançados:** Chain of Responsibility, Mediator, Proxy, Template Method
4. **Raros (por completude):** Visitor, Flyweight, Memento, Interpreter

## Como adicionar um novo padrão

1. Copie `docs/PATTERN_TEMPLATE.md` para `src/DesignPatterns/<Categoria>/<Padrao>/README.md` e preencha.
2. Crie `Bad/` e `Good/` ao lado do README, cada um com seu namespace (`DesignPatterns.<Categoria>.<Padrao>.Bad` / `.Good`).
3. Crie os testes em `tests/DesignPatterns.Tests/<Categoria>/<Padrao>/`, incluindo um teste que compara o comportamento de `Bad` e `Good`.
4. Atualize o índice acima (status ✅ e link para o README).
5. Garanta que `dotnet test` passa antes de abrir o PR.

## Referências

- [Refactoring.Guru: Padrões de projeto](https://refactoring.guru/pt-br/design-patterns)
- *Design Patterns: Elements of Reusable Object-Oriented Software* (GoF)
- *Head First Design Patterns*
- *Dive Into Design Patterns* (Alexander Shvets)
