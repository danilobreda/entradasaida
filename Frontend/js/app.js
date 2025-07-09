// EntradaSaida Dashboard - JavaScript Principal
class EntradaSaidaDashboard {
    constructor() {
        this.connection = null;
        this.chart = null;
        this.isConnected = false;
        this.lastUpdate = new Date();
        
        // URL base para os endpoints da API
        this.baseUrl = 'http://localhost:52138';
        
        this.init();
    }
    
    async init() {
        console.log('Inicializando Dashboard EntradaSaida...');
        
        // Configurar SignalR
        await this.setupSignalR();
        
        // Configurar event listeners
        this.setupEventListeners();
        
        // Configurar gráfico
        this.setupChart();
        
        // Carregar dados iniciais
        await this.loadInitialData();
        
        // Iniciar atualizações periódicas
        this.startPeriodicUpdates();
        
        console.log('Dashboard inicializado com sucesso!');
    }
    
    async setupSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(`${this.baseUrl}/counterhub`)
                .withAutomaticReconnect()
                .build();
            
            // Event handlers
            this.connection.on("CounterEvent", (event) => {
                this.handleCounterEvent(event);
            });
            
            this.connection.on("CounterEvents", (events) => {
                this.handleCounterEvents(events);
            });
            
            this.connection.on("StatsUpdate", (stats) => {
                this.updateStats(stats);
            });
            
            this.connection.on("FrameUpdate", (frameData) => {
                this.updateVideoFeed(frameData);
            });
            
            this.connection.on("SystemStatus", (status) => {
                this.updateSystemStatus(status);
            });
            
            this.connection.on("Welcome", (message) => {
                console.log('Mensagem de boas-vindas:', message);
            });
            
            // Connection events
            this.connection.onclose(() => {
                this.setConnectionStatus(false);
            });
            
            this.connection.onreconnecting(() => {
                console.log('Reconectando...');
                this.setConnectionStatus(false);
            });
            
            this.connection.onreconnected(() => {
                console.log('Reconectado!');
                this.setConnectionStatus(true);
                this.loadInitialData();
            });
            
            // Conectar
            await this.connection.start();
            console.log('SignalR conectado!');
            this.setConnectionStatus(true);
            
            // Entrar no grupo de frames
            await this.connection.invoke("JoinGroup", "frames");
            
        } catch (err) {
            console.error('Erro ao conectar SignalR:', err);
            this.setConnectionStatus(false);
        }
    }
    
    setupEventListeners() {
        // Botões de controle
        document.getElementById('startBtn').addEventListener('click', () => {
            this.startCamera();
        });
        
        document.getElementById('stopBtn').addEventListener('click', () => {
            this.stopCamera();
        });
        
        document.getElementById('resetBtn').addEventListener('click', () => {
            this.resetCounters();
        });
        
        // Clique no feed de vídeo para atualizar
        document.getElementById('videoFeed').addEventListener('click', () => {
            this.refreshVideoFeed();
        });
    }
    
    setupChart() {
        const ctx = document.getElementById('hourlyChart').getContext('2d');
        
        this.chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: Array.from({length: 24}, (_, i) => `${i}:00`),
                datasets: [
                    {
                        label: 'Entradas',
                        data: new Array(24).fill(0),
                        borderColor: '#48bb78',
                        backgroundColor: 'rgba(72, 187, 120, 0.1)',
                        tension: 0.4,
                        fill: true
                    },
                    {
                        label: 'Saídas',
                        data: new Array(24).fill(0),
                        borderColor: '#f56565',
                        backgroundColor: 'rgba(245, 101, 101, 0.1)',
                        tension: 0.4,
                        fill: true
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: 'Tráfego por Hora - Hoje'
                    }
                }
            }
        });
    }
    
    async loadInitialData() {
        try {
            // Carregar estatísticas de hoje
            const response = await fetch(`${this.baseUrl}/api/counter/summary`);
            const data = await response.json();
            
            if (data.today) {
                this.updateStats(data.today);
                this.updateChart(data.today);
            }
            
            if (data.recentEvents) {
                this.updateRecentEvents(Array.from(data.recentEvents));
            }
            
            // Atualizar ocupação atual
            this.updateElement('occupancyCount', data.currentOccupancy || 0);
            
        } catch (error) {
            console.error('Erro ao carregar dados iniciais:', error);
        }
    }
    
    async startCamera() {
        try {
            const response = await fetch(`${this.baseUrl}/api/camera/start`, {
                method: 'POST'
            });
            
            const result = await response.json();
            
            if (response.ok) {
                this.showNotification('Câmera iniciada com sucesso!', 'success');
                setTimeout(() => this.refreshVideoFeed(), 1000);
            } else {
                this.showNotification(result.error || 'Erro ao iniciar câmera', 'error');
            }
        } catch (error) {
            console.error('Erro ao iniciar câmera:', error);
            this.showNotification('Erro ao iniciar câmera', 'error');
        }
    }
    
    async stopCamera() {
        try {
            const response = await fetch(`${this.baseUrl}/api/camera/stop`, {
                method: 'POST'
            });
            
            const result = await response.json();
            
            if (response.ok) {
                this.showNotification('Câmera parada com sucesso!', 'success');
                this.clearVideoFeed();
            } else {
                this.showNotification(result.error || 'Erro ao parar câmera', 'error');
            }
        } catch (error) {
            console.error('Erro ao parar câmera:', error);
            this.showNotification('Erro ao parar câmera', 'error');
        }
    }
    
    async resetCounters() {
        if (!confirm('Tem certeza que deseja resetar todos os contadores?')) {
            return;
        }
        
        try {
            const response = await fetch(`${this.baseUrl}/api/counter/reset`, {
                method: 'POST'
            });
            
            const result = await response.json();
            
            if (response.ok) {
                this.showNotification('Contadores resetados!', 'success');
                await this.loadInitialData();
            } else {
                this.showNotification(result.error || 'Erro ao resetar contadores', 'error');
            }
        } catch (error) {
            console.error('Erro ao resetar contadores:', error);
            this.showNotification('Erro ao resetar contadores', 'error');
        }
    }
    
    async refreshVideoFeed() {
        try {
            const response = await fetch(`${this.baseUrl}/api/camera/frame`);
            
            if (response.ok) {
                const blob = await response.blob();
                const url = URL.createObjectURL(blob);
                
                const videoFeed = document.getElementById('videoFeed');
                videoFeed.src = url;
                
                // Limpar URL anterior
                setTimeout(() => URL.revokeObjectURL(url), 500);
                
                // Esconder overlay
                document.getElementById('videoStatus').style.display = 'none';
            } else {
                this.clearVideoFeed();
            }
        } catch (error) {
            console.error('Erro ao atualizar feed:', error);
            this.clearVideoFeed();
        }
    }
    
    clearVideoFeed() {
        const videoFeed = document.getElementById('videoFeed');
        videoFeed.src = "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjQwIiBoZWlnaHQ9IjQ4MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KICA8cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZjBmMGYwIi8+CiAgPHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIxOCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPk5lbmh1bSBmZWVkIGRpc3BvbsOtdmVsPC90ZXh0Pgo8L3N2Zz4K";
        document.getElementById('videoStatus').style.display = 'flex';
    }
    
    handleCounterEvent(event) {
        console.log('Evento de contagem recebido:', event);
        this.addRecentEvent(event);
        this.updateLastUpdate();
    }
    
    handleCounterEvents(events) {
        console.log('Múltiplos eventos recebidos:', events);
        events.forEach(event => this.addRecentEvent(event));
        this.updateLastUpdate();
    }
    
    updateStats(stats) {
        this.updateElement('entriesCount', stats.totalEntries || 0);
        this.updateElement('exitsCount', stats.totalExits || 0);
        this.updateElement('balanceCount', stats.balance || 0);
        
        // Atualizar cor do saldo
        const balanceElement = document.getElementById('balanceCount');
        const balance = stats.balance || 0;
        balanceElement.style.color = balance >= 0 ? '#48bb78' : '#f56565';
        
        this.updateLastUpdate();
    }
    
    updateChart(stats) {
        if (!this.chart || !stats.hourlyBreakdown) return;
        
        const entries = new Array(24).fill(0);
        const exits = new Array(24).fill(0);
        
        stats.hourlyBreakdown.forEach(hourData => {
            if (hourData.hour >= 0 && hourData.hour < 24) {
                entries[hourData.hour] = hourData.entries || 0;
                exits[hourData.hour] = hourData.exits || 0;
            }
        });
        
        this.chart.data.datasets[0].data = entries;
        this.chart.data.datasets[1].data = exits;
        this.chart.update();
    }
    
    updateVideoFeed(frameData) {
        if (frameData.frameData) {
            const videoFeed = document.getElementById('videoFeed');
            videoFeed.src = `data:image/jpeg;base64,${frameData.frameData}`;
            document.getElementById('videoStatus').style.display = 'none';
        }
        
        this.updateElement('detectionsValue', frameData.detectionCount || 0);
        this.updateElement('trackedValue', frameData.trackedCount || 0);
        this.updateLastUpdate();
    }
    
    updateSystemStatus(status) {
        console.log('Status do sistema:', status);
        this.updateLastUpdate();
    }
    
    addRecentEvent(event) {
        const eventsList = document.getElementById('recentEvents');
        const noEvents = eventsList.querySelector('.no-events');
        
        if (noEvents) {
            noEvents.remove();
        }
        
        const eventElement = document.createElement('div');
        eventElement.className = `event-item ${event.type === 0 ? 'entry' : 'exit'}`;
        
        const iconClass = event.type === 0 ? 'fa-arrow-right' : 'fa-arrow-left';
        const eventType = event.type === 0 ? 'Entrada' : 'Saída';
        const iconColorClass = event.type === 0 ? 'entry' : 'exit';
        
        eventElement.innerHTML = `
            <div class="event-icon ${iconColorClass}">
                <i class="fas ${iconClass}"></i>
            </div>
            <div class="event-content">
                <h4>${eventType} - Pessoa ${event.personId}</h4>
                <p>${new Date(event.timestamp).toLocaleString()}</p>
            </div>
        `;
        
        eventsList.insertBefore(eventElement, eventsList.firstChild);
        
        // Manter apenas os últimos 10 eventos
        const events = eventsList.querySelectorAll('.event-item');
        if (events.length > 10) {
            events[events.length - 1].remove();
        }
    }
    
    updateRecentEvents(events) {
        const eventsList = document.getElementById('recentEvents');
        eventsList.innerHTML = '';
        
        if (events.length === 0) {
            eventsList.innerHTML = `
                <div class="no-events">
                    <i class="fas fa-info-circle"></i>
                    <p>Nenhum evento registrado</p>
                </div>
            `;
        } else {
            events.forEach(event => this.addRecentEvent(event));
        }
    }
    
    setConnectionStatus(connected) {
        this.isConnected = connected;
        const statusElement = document.getElementById('connectionStatus');
        
        if (connected) {
            statusElement.className = 'status-badge connected';
            statusElement.innerHTML = '<i class="fas fa-circle"></i> Conectado';
        } else {
            statusElement.className = 'status-badge disconnected';
            statusElement.innerHTML = '<i class="fas fa-circle"></i> Desconectado';
        }
    }
    
    updateElement(id, value) {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = value;
        }
    }
    
    updateLastUpdate() {
        this.lastUpdate = new Date();
        document.getElementById('lastUpdate').textContent = this.lastUpdate.toLocaleTimeString();
    }
    
    showNotification(message, type = 'info') {
        // Criar notificação toast
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            border-radius: 10px;
            color: white;
            font-weight: 600;
            z-index: 1000;
            animation: slideInRight 0.3s ease-out;
        `;
        
        switch (type) {
            case 'success':
                notification.style.background = '#48bb78';
                break;
            case 'error':
                notification.style.background = '#f56565';
                break;
            default:
                notification.style.background = '#667eea';
        }
        
        notification.textContent = message;
        document.body.appendChild(notification);
        
        // Remover após 3 segundos
        setTimeout(() => {
            notification.remove();
        }, 3000);
    }
    
    startPeriodicUpdates() {
        // Atualizar dados a cada 30 segundos
        setInterval(async () => {
            if (this.isConnected) {
                await this.loadInitialData();
            }
        }, 30000);
        
        // Atualizar feed de vídeo a cada 5 segundos
        setInterval(async () => {
            if (this.isConnected) {
                await this.refreshVideoFeed();
            }
        }, 500);
    }
}

// CSS adicional para animações
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);

// Inicializar dashboard quando a página carregar
document.addEventListener('DOMContentLoaded', () => {
    window.dashboard = new EntradaSaidaDashboard();
});
