#!/bin/bash

# Script de inicialização do Sistema EntradaSaida

echo "🚀 Iniciando Sistema EntradaSaida..."

# Verificar se .NET 9 está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 9 SDK não encontrado!"
    echo "Por favor, instale o .NET 9 SDK: https://dotnet.microsoft.com/download"
    exit 1
fi

# Verificar versão do .NET
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET Version: $DOTNET_VERSION"

# Criar diretórios necessários
echo "📁 Criando diretórios..."
mkdir -p models
mkdir -p recordings
mkdir -p data

# Verificar se o modelo YOLO existe
if [ ! -f "models/yolov8n.onnx" ]; then
    echo "⚠️  Modelo YOLOv8 não encontrado em models/yolov8n.onnx"
    echo "📥 Você pode baixar o modelo de: https://github.com/ultralytics/ultralytics"
    echo "💡 O sistema funcionará, mas sem detecção até o modelo ser adicionado"
fi

# Restaurar dependências
echo "📦 Restaurando dependências..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Erro ao restaurar dependências!"
    exit 1
fi

# Build do projeto
echo "🔨 Compilando projeto..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Erro na compilação!"
    exit 1
fi

# Executar migrações do banco
echo "🗄️  Configurando banco de dados..."
cd src/EntradaSaida.Api
dotnet ef database update --no-build

# Iniciar aplicação
echo "🌟 Iniciando aplicação..."
echo "🌐 Dashboard estará disponível em: http://localhost:5000"
echo "📚 API Documentation em: http://localhost:5000/swagger"
echo ""
echo "⚡ Para parar o sistema, pressione Ctrl+C"
echo ""

dotnet run --no-build --configuration Release
