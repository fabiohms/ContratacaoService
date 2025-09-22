# ContratacaoService

Microserviço responsável por processar contratações de propostas aprovadas.

## ?? Início Rápido

### Opção 1: Script PowerShell (Recomendado)
```powershell
# Valida rede Docker e inicia automaticamente
.\scripts\start-with-network-validation.ps1
```

### Opção 2: Docker Compose
```bash
docker network create propostaservice_microservices-network
docker-compose up -d
```

### Opção 3: Desenvolvimento Local
```bash
docker-compose up -d postgres rabbitmq
dotnet run
```

## ?? Acesso

- **API**: http://localhost:5002
- **Swagger**: http://localhost:5002/swagger

## ??? Arquitetura

Este serviço utiliza arquitetura hexagonal (Ports and Adapters):

```
Application/     # Casos de uso e DTOs
Domain/         # Entidades e regras de negócio  
Infrastructure/ # Adaptadores (DB, HTTP, Messaging)
```

## ?? Integração

- **RabbitMQ**: Eventos de propostas aprovadas/rejeitadas
- **HTTP**: Fallback para consulta de status
- **PostgreSQL**: Persistência de contratações

## ??? Tecnologias

- .NET 8
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Docker
- Swagger/OpenAPI