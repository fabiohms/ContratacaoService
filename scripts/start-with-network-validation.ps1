#!/usr/bin/env pwsh

# Script PowerShell para validar/criar rede Docker e subir ContratacaoService
# Compat�vel com PowerShell Core (multiplataforma)

param(
    [switch]$Force,
    [switch]$Logs,
    [string]$NetworkName = "propostaservice_microservices-network"
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "??  $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "??  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Red
}

function Test-DockerRunning {
    try {
        $null = docker version 2>&1
        return $true
    }
    catch {
        return $false
    }
}

function Test-NetworkExists {
    param([string]$Name)
    
    try {
        $networks = docker network ls --format "{{.Name}}" 2>$null
        return $networks -contains $Name
    }
    catch {
        return $false
    }
}

function New-DockerNetwork {
    param([string]$Name)
    
    Write-Info "Criando rede Docker '$Name'..."
    try {
        docker network create $Name --driver bridge | Out-Null
        Write-Success "Rede '$Name' criada com sucesso!"
        return $true
    }
    catch {
        Write-Error "Falha ao criar rede '$Name': $($_.Exception.Message)"
        return $false
    }
}

function Show-NetworkInfo {
    param([string]$Name)
    
    Write-Info "Informa��es da rede '$Name':"
    try {
        $networkInfo = docker network inspect $Name --format '{{.Name}}: {{.Driver}} ({{.Scope}})' 2>$null
        Write-Host "   ?? $networkInfo" -ForegroundColor White
        
        Write-Info "Containers conectados:"
        $containers = docker network inspect $Name --format '{{range .Containers}}  - {{.Name}}: {{.IPv4Address}}{{"\n"}}{{end}}' 2>$null
        if ([string]::IsNullOrWhiteSpace($containers)) {
            Write-Host "   ?? Nenhum container conectado" -ForegroundColor Gray
        } else {
            Write-Host $containers -ForegroundColor White
        }
    }
    catch {
        Write-Warning "N�o foi poss�vel obter informa��es detalhadas da rede"
    }
}

function Start-ContratacaoService {
    param([bool]$ShowLogs = $false)
    
    Write-Info "Verificando se docker-compose.yml existe..."
    if (-not (Test-Path "docker-compose.yml")) {
        Write-Error "Arquivo docker-compose.yml n�o encontrado no diret�rio atual"
        return $false
    }

    if ($Force) {
        Write-Info "Parando servi�os existentes (--Force)..."
        docker-compose down 2>$null | Out-Null
    }

    Write-Info "Building ContratacaoService..."
    try {
        docker-compose build contratacao-service
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Falha no build do ContratacaoService"
            return $false
        }
    }
    catch {
        Write-Error "Erro durante o build: $($_.Exception.Message)"
        return $false
    }

    Write-Info "Iniciando ContratacaoService..."
    try {
        docker-compose up -d contratacao-service
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Falha ao iniciar ContratacaoService"
            return $false
        }
    }
    catch {
        Write-Error "Erro ao iniciar servi�o: $($_.Exception.Message)"
        return $false
    }

    Write-Info "Aguardando ContratacaoService inicializar..."
    Start-Sleep -Seconds 10

    # Verificar se est� rodando
    try {
        $status = docker-compose ps --format "table {{.Service}}\t{{.State}}" 2>$null
        if ($status -match "contratacao-service.*Up") {
            Write-Success "ContratacaoService iniciado com sucesso!"
            Write-Info "?? ContratacaoService dispon�vel em: http://localhost:5002"
            Write-Info "?? Swagger UI: http://localhost:5002/swagger"
            Write-Host ""
            Write-Info "Status dos servi�os:"
            docker-compose ps
            
            if ($ShowLogs) {
                Write-Host ""
                Write-Info "Logs recentes do servi�o:"
                docker-compose logs --tail=20 contratacao-service
            }
            
            return $true
        } else {
            Write-Error "ContratacaoService n�o est� executando corretamente"
            Write-Info "Logs do servi�o:"
            docker-compose logs contratacao-service
            return $false
        }
    }
    catch {
        Write-Error "Erro ao verificar status do servi�o: $($_.Exception.Message)"
        return $false
    }
}

# In�cio do script principal
Write-Host "?? ContratacaoService Startup Script" -ForegroundColor Magenta
Write-Host "=====================================" -ForegroundColor Magenta
Write-Host ""

# Verificar se Docker est� rodando
Write-Info "Verificando se Docker est� rodando..."
if (-not (Test-DockerRunning)) {
    Write-Error "Docker n�o est� rodando ou n�o est� acess�vel"
    Write-Info "Por favor, inicie o Docker Desktop ou o daemon do Docker"
    exit 1
}
Write-Success "Docker est� rodando"

# Validar/criar rede
Write-Info "Validando rede microservices..."
if (Test-NetworkExists -Name $NetworkName) {
    Write-Success "Rede '$NetworkName' j� existe"
    Show-NetworkInfo -Name $NetworkName
} else {
    Write-Warning "Rede '$NetworkName' n�o encontrada"
    if (-not (New-DockerNetwork -Name $NetworkName)) {
        exit 1
    }
}

Write-Host ""
Write-Success "Rede '$NetworkName' est� pronta para uso!"

# Iniciar servi�o
Write-Host ""
if (Start-ContratacaoService -ShowLogs $Logs) {
    Write-Host ""
    Write-Success "?? ContratacaoService est� rodando com sucesso!"
    Write-Info "?? Use 'docker-compose logs -f contratacao-service' para acompanhar os logs"
    Write-Info "?? Use 'docker-compose down' para parar o servi�o"
} else {
    Write-Host ""
    Write-Error "?? Falha ao iniciar ContratacaoService"
    exit 1
}