using Microsoft.AspNetCore.Mvc;
using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Api.Controllers;

/// <summary>
/// Controller para estatísticas e eventos de contagem
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CounterController : ControllerBase
{
    private readonly ICounterService _counterService;
    private readonly ILogger<CounterController> _logger;
    
    public CounterController(ICounterService counterService, ILogger<CounterController> logger)
    {
        _counterService = counterService;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtém estatísticas de hoje
    /// </summary>
    [HttpGet("stats/today")]
    public async Task<IActionResult> GetTodayStatsAsync()
    {
        try
        {
            var stats = await _counterService.GetStatsAsync(DateTime.Today);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de hoje");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém estatísticas de uma data específica
    /// </summary>
    [HttpGet("stats/{date}")]
    public async Task<IActionResult> GetStatsAsync(DateTime date)
    {
        try
        {
            var stats = await _counterService.GetStatsAsync(date.Date);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas da data {Date}", date);
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém estatísticas de um período
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsRangeAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest("Data inicial deve ser menor que data final");
            
            var stats = await _counterService.GetStatsRangeAsync(startDate.Date, endDate.Date);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas do período {Start} - {End}", startDate, endDate);
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém ocupação atual
    /// </summary>
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetCurrentOccupancyAsync()
    {
        try
        {
            var occupancy = await _counterService.GetCurrentOccupancyAsync();
            return Ok(new { currentOccupancy = occupancy });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ocupação atual");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém eventos de hoje
    /// </summary>
    [HttpGet("events/today")]
    public async Task<IActionResult> GetTodayEventsAsync()
    {
        try
        {
            var events = await _counterService.GetTodayEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter eventos de hoje");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém todas as linhas de contagem ativas
    /// </summary>
    [HttpGet("lines")]
    public async Task<IActionResult> GetCountingLinesAsync()
    {
        try
        {
            var lines = await _counterService.GetActiveCountingLinesAsync();
            return Ok(lines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter linhas de contagem");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Adiciona uma nova linha de contagem
    /// </summary>
    [HttpPost("lines")]
    public async Task<IActionResult> AddCountingLineAsync([FromBody] CountingLine line)
    {
        try
        {
            if (string.IsNullOrEmpty(line.Name))
                return BadRequest("Nome da linha é obrigatório");
            
            var result = await _counterService.AddCountingLineAsync(line);
            _logger.LogInformation("Nova linha de contagem criada: {Name}", line.Name);
            
            return CreatedAtAction(nameof(GetCountingLinesAsync), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar linha de contagem");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Remove uma linha de contagem
    /// </summary>
    [HttpDelete("lines/{lineId}")]
    public async Task<IActionResult> RemoveCountingLineAsync(int lineId)
    {
        try
        {
            var success = await _counterService.RemoveCountingLineAsync(lineId);
            
            if (success)
            {
                _logger.LogInformation("Linha de contagem removida: {LineId}", lineId);
                return Ok(new { message = "Linha removida com sucesso" });
            }
            else
            {
                return NotFound("Linha não encontrada");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover linha de contagem");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Reseta todos os contadores
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetCountersAsync()
    {
        try
        {
            await _counterService.ResetCountersAsync();
            _logger.LogWarning("Contadores resetados");
            
            return Ok(new { message = "Contadores resetados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resetar contadores");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém resumo das estatísticas
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        try
        {
            var todayStats = await _counterService.GetStatsAsync(DateTime.Today);
            var currentOccupancy = await _counterService.GetCurrentOccupancyAsync();
            var todayEvents = await _counterService.GetTodayEventsAsync();
            
            var summary = new
            {
                today = todayStats,
                currentOccupancy,
                recentEvents = todayEvents.TakeLast(10).OrderByDescending(e => e.Timestamp)
            };
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter resumo");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
