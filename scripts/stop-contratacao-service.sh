#!/bin/bash

# Script para parar o ContratacaoService

set -e

echo "?? Parando ContratacaoService..."

docker-compose down

echo "? ContratacaoService parado!"

# Mostrar status
echo "?? Status dos containers:"
docker ps -a --filter "name=contratacao"