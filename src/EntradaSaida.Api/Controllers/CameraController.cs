using Microsoft.AspNetCore.Mvc;
using EntradaSaida.ML.Processing;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Api.Controllers;

/// <summary>
/// Controller para controle da câmera e processamento de vídeo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CameraController : ControllerBase
{
    private readonly VideoProcessor _videoProcessor;
    private readonly ILogger<CameraController> _logger;
    
    public CameraController(VideoProcessor videoProcessor, ILogger<CameraController> logger)
    {
        _videoProcessor = videoProcessor;
        _logger = logger;
    }
    
    /// <summary>
    /// Inicia o processamento de vídeo
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartAsync()
    {
        try
        {
            if (_videoProcessor.IsRunning)
                return BadRequest("Processamento já está ativo");
            
            await _videoProcessor.StartAsync();
            _logger.LogInformation("Processamento de vídeo iniciado");
            
            return Ok(new { message = "Processamento iniciado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar processamento");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Para o processamento de vídeo
    /// </summary>
    [HttpPost("stop")]
    public async Task<IActionResult> StopAsync()
    {
        try
        {
            if (!_videoProcessor.IsRunning)
                return BadRequest("Processamento não está ativo");
            
            await _videoProcessor.StopAsync();
            _logger.LogInformation("Processamento de vídeo parado");
            
            return Ok(new { message = "Processamento parado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar processamento");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém o status atual da câmera
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatusAsync()
    {
        try
        {
            var stats = await _videoProcessor.GetProcessingStatsAsync();
            
            return Ok(new
            {
                isRunning = _videoProcessor.IsRunning,
                stats = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Configura a fonte de vídeo
    /// </summary>
    [HttpPost("source")]
    public async Task<IActionResult> SetVideoSourceAsync([FromBody] VideoSourceRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Source))
                return BadRequest("Fonte de vídeo é obrigatória");
            
            var success = await _videoProcessor.SetVideoSourceAsync(request.Source);
            
            if (success)
            {
                _logger.LogInformation("Fonte de vídeo configurada: {Source}", request.Source);
                return Ok(new { message = "Fonte configurada com sucesso" });
            }
            else
            {
                return BadRequest("Falha ao configurar fonte de vídeo");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar fonte");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém o frame atual
    /// </summary>
    [HttpGet("frame")]
    public async Task<IActionResult> GetCurrentFrameAsync()
    {
        try
        {
            var frame = await _videoProcessor.GetCurrentFrameAsync();
            
            if (frame == null)
                return NotFound("Nenhum frame disponível");
            
            return File(frame, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter frame");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Adiciona uma linha de contagem
    /// </summary>
    [HttpPost("lines")]
    public IActionResult AddCountingLine([FromBody] CountingLine line)
    {
        try
        {
            _videoProcessor.AddCountingLine(line);
            _logger.LogInformation("Linha de contagem adicionada: {Name}", line.Name);
            
            return Ok(new { message = "Linha adicionada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar linha");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Remove uma linha de contagem
    /// </summary>
    [HttpDelete("lines/{lineId}")]
    public IActionResult RemoveCountingLine(int lineId)
    {
        try
        {
            var success = _videoProcessor.RemoveCountingLine(lineId);
            
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
            _logger.LogError(ex, "Erro ao remover linha");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Carrega modelo YOLO
    /// </summary>
    [HttpPost("model")]
    public async Task<IActionResult> LoadModelAsync([FromBody] ModelRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ModelPath))
                return BadRequest("Caminho do modelo é obrigatório");
            
            var success = await _videoProcessor.LoadModelAsync(request.ModelPath);
            
            if (success)
            {
                _logger.LogInformation("Modelo carregado: {ModelPath}", request.ModelPath);
                return Ok(new { message = "Modelo carregado com sucesso" });
            }
            else
            {
                return BadRequest("Falha ao carregar modelo");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar modelo");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request para configurar fonte de vídeo
/// </summary>
public class VideoSourceRequest
{
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Request para carregar modelo
/// </summary>
public class ModelRequest
{
    public string ModelPath { get; set; } = string.Empty;
}
