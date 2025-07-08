# Sistema EntradaSaida - Detecção e Contagem de Pessoas

## 📋 Visão Geral
Sistema completo de visão computacional para monitoramento automático de entrada e saída de pessoas em estabelecimentos comerciais usando câmeras de segurança.

## 🎯 Funcionalidades
- ✅ Detectar pessoas em tempo real usando YOLOv8 ONNX
- ✅ Rastrear movimento e contar entrada/saída via linha virtual
- ✅ API REST para integração
- ✅ Dashboard web para monitoramento em tempo real
- ✅ Compatibilidade total com Linux
- ✅ Atualizações em tempo real via SignalR
- ✅ Estatísticas detalhadas e gráficos

## 🛠 Stack Tecnológica
- **.NET 9** (ASP.NET Core Web API)
- **EmguCV 4.8+** para processamento de imagem
- **ONNX Runtime 1.16+** para inferência ML
- **YOLOv8** modelo de detecção otimizado
- **Entity Framework Core** com SQLite
- **SignalR** para updates em tempo real
- **Chart.js** para visualizações

## 📁 Estrutura do Projeto
```
src/
├── EntradaSaida.Api/                    # 🌐 Web API Principal
│   ├── Controllers/                     # REST endpoints
│   ├── Hubs/                           # SignalR real-time
│   ├── wwwroot/                        # Dashboard web
│   └── Program.cs                      # Entry point
│
├── EntradaSaida.Core/                   # 🧠 Lógica de Negócio
│   ├── Models/                         # Entidades e DTOs
│   ├── Interfaces/                     # Contratos
│   └── Services/                       # Serviços de domínio
│
├── EntradaSaida.Infrastructure/         # 🔧 Implementações
│   ├── Data/                           # Entity Framework
│   ├── Repositories/                   # Padrão Repository
│   └── Services/                       # Serviços externos
│
└── EntradaSaida.ML/                     # 🤖 Visão Computacional
    ├── Detection/                      # Detecção YOLO
    ├── Tracking/                       # Rastreamento
    └── Processing/                     # Processamento
```

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0 SDK
- Modelo YOLOv8 ONNX (baixar e colocar em `models/yolov8n.onnx`)
- Câmera USB ou arquivo de vídeo

### Passos
1. **Clone o repositório**
   ```bash
   git clone <url-do-repo>
   cd entradasaida
   ```

2. **Restaurar dependências**
   ```bash
   dotnet restore
   ```

3. **Executar o projeto**
   ```bash
   cd src/EntradaSaida.Api
   dotnet run
   ```

4. **Acessar o dashboard**
   - Abrir: http://localhost:5000
   - Dashboard em tempo real com estatísticas

## 🎮 Como Usar

### Dashboard Web
1. Acesse o dashboard no navegador
2. Configure a fonte de vídeo (câmera ou arquivo)
3. Clique em "Iniciar" para começar o monitoramento
4. Configure linhas de contagem conforme necessário
5. Monitore estatísticas em tempo real

### API REST
- `GET /api/counter/stats/today` - Estatísticas de hoje
- `POST /api/camera/start` - Iniciar processamento
- `POST /api/camera/stop` - Parar processamento
- `GET /api/counter/occupancy` - Ocupação atual
- `POST /api/counter/reset` - Resetar contadores

### Configurações
- Acessar via API: `/api/config`
- Configurar threshold de confiança
- Ajustar parâmetros de rastreamento
- Definir fonte de vídeo

## 📊 Funcionalidades do Dashboard

### Tempo Real
- Feed de vídeo com detecções visualizadas
- Estatísticas atualizadas automaticamente
- Notificações de eventos de entrada/saída
- Gráficos de tráfego por hora

### Estatísticas
- Contagem de entradas e saídas
- Ocupação atual do estabelecimento
- Histórico por horas/dias
- Eventos recentes detalhados

### Controles
- Iniciar/parar monitoramento
- Resetar contadores
- Configurar linhas de contagem
- Ajustar parâmetros do modelo

## 🔧 Configuração

### Modelo YOLOv8
1. Baixar modelo: https://github.com/ultralytics/ultralytics
2. Colocar em: `models/yolov8n.onnx`
3. Configurar caminho via API ou appsettings.json

### Linhas de Contagem
- Configurar via dashboard ou API
- Definir coordenadas de início e fim
- Especificar direção (entrada/saída)
- Ativar/desativar conforme necessário

### Câmera
- USB: usar índice (0, 1, 2...)
- Arquivo: caminho completo
- RTSP: URL do stream
- Configurar resolução e FPS

## 🐳 Docker (Futuro)
```dockerfile
# Dockerfile incluído para containerização
docker build -t entradasaida .
docker run -p 5000:5000 entradasaida
```

## 🤝 Contribuindo
1. Fork o projeto
2. Criar branch de feature
3. Commit suas mudanças
4. Push para a branch
5. Abrir Pull Request

## 📝 Licença
Este projeto está sob licença MIT.

## 🆘 Suporte
- Issues: GitHub Issues
- Documentação: Wiki do projeto
- Email: suporte@exemplo.com

---

**Sistema EntradaSaida** - Monitoramento inteligente de pessoas 👥🔍