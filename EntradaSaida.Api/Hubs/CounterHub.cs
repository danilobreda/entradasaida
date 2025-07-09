using Microsoft.AspNetCore.SignalR;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Api.Hubs;

/// <summary>
/// Hub SignalR para atualizações em tempo real
/// </summary>
public class CounterHub : Hub
{
    private readonly ILogger<CounterHub> _logger;
    
    public CounterHub(ILogger<CounterHub> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Cliente se conecta ao grupo de atualizações
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} entrou no grupo {GroupName}", Context.ConnectionId, groupName);
    }
    
    /// <summary>
    /// Cliente sai do grupo de atualizações
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} saiu do grupo {GroupName}", Context.ConnectionId, groupName);
    }
    
    /// <summary>
    /// Envia evento de contagem para todos os clientes
    /// </summary>
    public async Task SendCounterEvent(CounterEvent counterEvent)
    {
        await Clients.All.SendAsync("CounterEvent", counterEvent);
    }
    
    /// <summary>
    /// Envia múltiplos eventos de contagem
    /// </summary>
    public async Task SendCounterEvents(List<CounterEvent> events)
    {
        await Clients.All.SendAsync("CounterEvents", events);
    }
    
    /// <summary>
    /// Envia atualização de estatísticas
    /// </summary>
    public async Task SendStatsUpdate(CounterStats stats)
    {
        await Clients.All.SendAsync("StatsUpdate", stats);
    }
    
    /// <summary>
    /// Envia frame processado para visualização
    /// </summary>
    public async Task SendFrameUpdate(byte[] frameData, int detectionCount, int trackedCount)
    {
        var frameUpdate = new
        {
            timestamp = DateTime.UtcNow,
            detectionCount,
            trackedCount,
            frameData = Convert.ToBase64String(frameData)
        };
        
        await Clients.Group("frames").SendAsync("FrameUpdate", frameUpdate);
    }
    
    /// <summary>
    /// Envia status do sistema
    /// </summary>
    public async Task SendSystemStatus(object status)
    {
        await Clients.All.SendAsync("SystemStatus", status);
    }
    
    /// <summary>
    /// Cliente solicita status atual
    /// </summary>
    public async Task RequestCurrentStatus()
    {
        // Aqui você pode implementar a lógica para enviar o status atual
        // Por exemplo, obter estatísticas atuais e enviá-las
        var status = new
        {
            timestamp = DateTime.UtcNow,
            message = "Status solicitado"
        };
        
        await Clients.Caller.SendAsync("CurrentStatus", status);
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        
        // Enviar dados iniciais para o cliente recém-conectado
        var welcomeMessage = new
        {
            message = "Conectado ao sistema EntradaSaida",
            timestamp = DateTime.UtcNow,
            connectionId = Context.ConnectionId
        };
        
        await Clients.Caller.SendAsync("Welcome", welcomeMessage);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "Cliente desconectado com erro: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
