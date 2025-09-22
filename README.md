# ContratacaoService

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-blue.svg)](https://www.docker.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Hexagonal-green.svg)](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software))

Microservi�o respons�vel por processar contrata��es baseadas em propostas aprovadas, implementado com **Arquitetura Hexagonal** (Ports and Adapters) e **.NET 8**.

## ??? Arquitetura

```mermaid
graph TB
    subgraph "External Services"
        PS[PropostaService]
        RMQ[RabbitMQ]
        PG[PostgreSQL]
    end
    
    subgraph "ContratacaoService"
        subgraph "Inbound Adapters"
            REST[REST Controller]
            MSG[Message Consumer]
        end
        
        subgraph "Application Core"
            UC[Use Cases]
            DOM[Domain]
        end
        
        subgraph "Outbound Adapters"
            HTTP[HTTP Client]
            DB[Database]
            CACHE[Cache]
        end
    end
    
    REST --> UC
    MSG --> UC
    UC --> DOM
    UC --> HTTP
    UC --> DB
    UC --> CACHE
    
    HTTP --> PS
    MSG --> RMQ
    DB --> PG
```

## ?? In�cio R�pido

### Op��o 1: Script PowerShell (Recomendado)

Para facilitar o in�cio, use o script PowerShell que valida a rede Docker e inicia o servi�o automaticamente.

```powershell
.\scripts\start-with-network-validation.ps1
```

### Op��o 2: Docker Compose

Caso prefira, � poss�vel iniciar o servi�o manualmente utilizando Docker Compose.

```bash
# Criar rede externa (uma vez apenas)
docker network create propostaservice_microservices-network

# Iniciar servi�o
docker-compose up -d
```

### Op��o 3: Desenvolvimento Local

Para desenvolvimento, voc� pode usar a infraestrutura padr�o do Docker e executar a aplica��o localmente.

```bash
# Usar apenas infraestrutura Docker
docker-compose up -d postgres rabbitmq

# Executar aplica��o localmente
dotnet run
```

## ?? Endpoints

| M�todo | Endpoint | Descri��o |
|--------|----------|-----------|
| `POST` | `/api/contratacoes` | Solicitar nova contrata��o |
| `GET` | `/api/contratacoes/{id}` | Consultar contrata��o por ID |

### Exemplo de Uso

```bash
# Solicitar contrata��o
curl -X POST http://localhost:5002/api/contratacoes \
  -H "Content-Type: application/json" \
  -d '{"propostaId": "50bb7eda-c79a-4c47-8c6e-d0c557467d42"}'

# Consultar contrata��o
curl http://localhost:5002/api/contratacoes/{id}
```

## ?? Integra��o com Microservi�os

### RabbitMQ (Eventos)
- **Exchange**: `proposta_events`
- **Queue**: `contratacao.propostas.events`
- **Events**:
  - `PropostaAprovadaEvent` ? Dispara nova contrata��o
  - `PropostaRejeitadaEvent` ? Atualiza cache de status

### HTTP (Fallback)
- **PropostaService**: `http://localhost:5000` (dev) / `http://proposta-service:8080` (prod)
- **Endpoint**: `GET /api/Propostas/{id}` ? Consulta status quando cache falha

## ?? Tecnologias

### Core
- **.NET 8** - Framework principal
- **C# 12** - Linguagem de programa��o
- **Entity Framework Core** - ORM para PostgreSQL
- **ASP.NET Core** - API REST

### Infraestrutura
- **PostgreSQL** - Banco de dados principal
- **RabbitMQ** - Message broker para eventos
- **Docker** - Containeriza��o
- **Swagger/OpenAPI** - Documenta��o da API

### Padr�es e Pr�ticas
- **Arquitetura Hexagonal** - Ports and Adapters
- **Clean Architecture** - Separa��o de responsabilidades
- **CQRS** - Command Query Responsibility Segregation
- **Event-Driven** - Comunica��o via eventos
- **Resilience** - Retry policies e circuit breakers

## ??? Estrutura do Projeto

```
ContratacaoService/
??? Application/           # ?? Casos de uso e DTOs
?   ??? Commands/         # Comandos de entrada
?   ??? DTOs/            # Data Transfer Objects
?   ??? Events/          # Eventos de dom�nio
?   ??? Ports/           # Interfaces (contratos)
?   ??? UseCases/        # L�gica de aplica��o
??? Domain/               # ?? Regras de neg�cio
?   ??? Entities/        # Entidades de dom�nio
?   ??? Repositories/    # Contratos de persist�ncia
?   ??? ValueObjects/    # Objetos de valor
??? Infrastructure/       # ?? Implementa��es t�cnicas
?   ??? Adapters/        # Adaptadores (Inbound/Outbound)
?   ??? Cache/           # Implementa��es de cache
?   ??? Configuration/   # Configura��es
?   ??? Migrations/      # Migrations do EF Core
??? docs/                # ?? Documenta��o completa
??? scripts/             # ?? Scripts de automa��o
??? docker-compose.yml   # ?? Configura��o Docker
```

## ?? URLs de Acesso

| Servi�o | URL | Descri��o |
|---------|-----|-----------|
| **API** | http://localhost:5002 | Endpoint principal da API |
| **Swagger** | http://localhost:5002/swagger | Documenta��o interativa |
| **Database** | localhost:5432 | PostgreSQL (contratacao) |
| **RabbitMQ** | http://localhost:15672 | Management UI (guest/guest) |

## ?? Configura��o

### Vari�veis de Ambiente (Docker)

As vari�veis de ambiente s�o configuradas no arquivo `docker-compose.yml`. Certifique-se de que est�o corretas para o seu ambiente.

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ConnectionStrings__ContratacaoDb=Host=postgres;Port=5432;Database=contratacao;Username=postgres;Password=postgres
  - PropostaService__BaseUrl=http://proposta-service:8080
  - RabbitMQ__Host=rabbitmq
  - RabbitMQ__Exchange=proposta_events
  - RabbitMQ__Queue=contratacao.propostas.events
```

### Configura��o Local (Development)

Para configurar o projeto para desenvolvimento local, edite o arquivo `appsettings.Development.json` com as informa��es do seu ambiente.

```json
{
  "ConnectionStrings": {
    "ContratacaoDb": "Host=localhost;Port=5432;Database=propostadb;Username=postgres;Password=postgres"
  },
  "PropostaService": {
    "BaseUrl": "http://localhost:5000"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Exchange": "proposta_events",
    "Queue": "contratacao.propostas.events"
  }
}
```

## ?? Documenta��o

Para documenta��o detalhada, consulte a pasta **[docs/](docs/README.md)**:

- [?? Guia Completo](docs/README.md) - �ndice de toda documenta��o
- [?? Scripts](docs/scripts-guide.md) - Como usar os scripts de automa��o
- [?? Docker](docs/docker-integration.md) - Configura��o e execu��o com Docker
- [?? Database](docs/database-setup.md) - Configura��o do banco e troubleshooting

## ??? Comandos �teis

```bash
# Build e execu��o
dotnet build
dotnet run

# Docker
docker-compose up -d
docker-compose logs -f contratacao-service
docker-compose down

# Migrations
dotnet ef migrations add NomeDaMigration
dotnet ef database update

# Testes
dotnet test
```

## ?? Contribuindo

Contribui��es s�o bem-vindas! Siga os passos:

1. Fa�a um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudan�as (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? Licen�a

Este projeto est� sob a licen�a MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## ?? Equipe

- **Desenvolvido por**: Fabio HMS
- **Arquitetura**: Hexagonal (Ports and Adapters)
- **Padr�es**: Clean Architecture, CQRS, Event-Driven

---

? **Se este projeto foi �til, considere dar uma estrela!** ?