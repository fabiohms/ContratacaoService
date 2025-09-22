#!/bin/bash

# Script para validar/criar rede microservices

set -e

NETWORK_NAME="propostaservice_microservices-network"

echo "?? Validando rede microservices..."

# Verificar se a rede existe
if docker network ls --format "table {{.Name}}" | grep -q "^${NETWORK_NAME}$"; then
    echo "? Rede ${NETWORK_NAME} j� existe"
    
    # Mostrar informa��es da rede
    echo "?? Informa��es da rede:"
    docker network inspect ${NETWORK_NAME} --format '{{.Name}}: {{.Driver}} ({{.Scope}})'
    echo "?? Containers conectados:"
    docker network inspect ${NETWORK_NAME} --format '{{range .Containers}}  - {{.Name}}: {{.IPv4Address}}{{"\n"}}{{end}}'
else
    echo "??  Rede ${NETWORK_NAME} n�o encontrada"
    echo "?? Criando rede ${NETWORK_NAME}..."
    
    docker network create ${NETWORK_NAME} --driver bridge
    
    echo "? Rede ${NETWORK_NAME} criada com sucesso!"
    echo "?? Driver: bridge"
fi

echo ""
echo "?? Rede ${NETWORK_NAME} est� pronta para uso!"
echo "?? Agora voc� pode subir os servi�os com: docker-compose up -d"