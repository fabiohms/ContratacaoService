#!/bin/bash

# Script para criar e executar ContratacaoService

set -e

echo "?? Iniciando ContratacaoService..."

# Verificar se a rede existe
if ! docker network ls | grep -q "microservices-network"; then
    echo "?? Criando rede microservices-network..."
    docker network create microservices-network
fi

# Build e start do ContratacaoService
echo "?? Building ContratacaoService..."
docker-compose build contratacao-service

echo "??  Iniciando ContratacaoService..."
docker-compose up -d contratacao-service

# Aguardar o serviço ficar saudável
echo "? Aguardando ContratacaoService inicializar..."
sleep 10

# Verificar se está rodando
if docker-compose ps | grep -q "contratacao-service.*Up"; then
    echo "? ContratacaoService iniciado com sucesso!"
    echo "?? ContratacaoService disponível em: http://localhost:5002"
    echo "?? Swagger UI: http://localhost:5002/swagger"
    echo ""
    echo "?? Status dos serviços:"
    docker-compose ps
else
    echo "? Erro ao iniciar ContratacaoService"
    echo "?? Logs do serviço:"
    docker-compose logs contratacao-service
    exit 1
fi