#!/bin/bash

# Script para executar migrations do ContratacaoService

set -e

echo "?? Executando migrations do ContratacaoService..."

# Verificar se o postgres est� rodando
if ! docker ps | grep -q "contratacao-postgres.*Up"; then
    echo "? PostgreSQL n�o est� rodando. Execute primeiro:"
    echo "   docker-compose up -d postgres"
    exit 1
fi

# Aguardar postgres estar pronto
echo "? Aguardando PostgreSQL ficar pronto..."
until docker exec contratacao-postgres pg_isready -U postgres -d contratacao; do
    sleep 2
done

echo "? PostgreSQL pronto!"

# Executar migrations usando dotnet ef (desenvolvimento)
if command -v dotnet &> /dev/null && [ -f "ContratacaoService.csproj" ]; then
    echo "???  Executando migrations com dotnet ef..."
    dotnet ef database update
    echo "? Migrations executadas com sucesso!"
else
    echo "??  dotnet ef n�o dispon�vel. As migrations ser�o executadas automaticamente quando o servi�o iniciar."
fi