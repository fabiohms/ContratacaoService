# ContratacaoService

Microservi�o respons�vel por processar contrata��es de propostas aprovadas.

## ?? In�cio R�pido

### Op��o 1: Script PowerShell (Recomendado)
```powershell
# Valida rede Docker e inicia automaticamente
.\scripts\start-with-network-validation.ps1
```

### Op��o 2: Docker Compose
```bash
docker network create propostaservice_microservices-network
docker-compose up -d
```

### Op��o 3: Desenvolvimento Local
```bash
docker-compose up -d postgres rabbitmq
dotnet run
```

## ?? Acesso

- **API**: http://localhost:5002
- **Swagger**: http://localhost:5002/swagger

## ??? Arquitetura

Este servi�o utiliza arquitetura hexagonal (Ports and Adapters):

```
Application/     # Casos de uso e DTOs
Domain/         # Entidades e regras de neg�cio  
Infrastructure/ # Adaptadores (DB, HTTP, Messaging)
```

## ?? Integra��o

- **RabbitMQ**: Eventos de propostas aprovadas/rejeitadas
- **HTTP**: Fallback para consulta de status
- **PostgreSQL**: Persist�ncia de contrata��es

## ??? Tecnologias

- .NET 8
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Docker
- Swagger/OpenAPI