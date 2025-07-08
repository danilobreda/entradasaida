using EntradaSaida.Core.Models;

namespace EntradaSaida.Core.Interfaces;

/// <summary>
/// Interface para serviços de contagem de pessoas
/// </summary>
public interface ICounterService
{
    /// <summary>
    /// Processa detecções e atualiza contadores
    /// </summary>
    Task<List<CounterEvent>> ProcessDetectionsAsync(List<Detection> detections);
    
    /// <summary>
    /// Adiciona uma linha de contagem
    /// </summary>
    Task<CountingLine> AddCountingLineAsync(CountingLine line);
    
    /// <summary>
    /// Remove uma linha de contagem
    /// </summary>
    Task<bool> RemoveCountingLineAsync(int lineId);
    
    /// <summary>
    /// Obtém todas as linhas de contagem ativas
    /// </summary>
    Task<List<CountingLine>> GetActiveCountingLinesAsync();
    
    /// <summary>
    /// Obtém estatísticas de contagem para uma data
    /// </summary>
    Task<CounterStats> GetStatsAsync(DateTime date);
    
    /// <summary>
    /// Obtém estatísticas de contagem para um período
    /// </summary>
    Task<List<CounterStats>> GetStatsRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Obtém o número atual de pessoas no estabelecimento
    /// </summary>
    Task<int> GetCurrentOccupancyAsync();
    
    /// <summary>
    /// Obtém todos os eventos de contagem do dia
    /// </summary>
    Task<List<CounterEvent>> GetTodayEventsAsync();
    
    /// <summary>
    /// Reseta os contadores
    /// </summary>
    Task ResetCountersAsync();
}
