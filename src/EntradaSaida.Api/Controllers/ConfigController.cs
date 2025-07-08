using Microsoft.AspNetCore.Mvc;
using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de configurações do sistema
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;
    private readonly ILogger<ConfigController> _logger;
    
    public ConfigController(IConfigService configService, ILogger<ConfigController> logger)
    {
        _configService = configService;
        _logger = logger;
    }
    
    /// <summary>
    /// Obtém todas as configurações
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllConfigsAsync()
    {
        try
        {
            var configs = await _configService.GetAllConfigsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configurações");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém uma configuração específica
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetConfigAsync(string key)
    {
        try
        {
            var config = await _configService.GetConfigAsync(key);
            
            if (config == null)
                return NotFound($"Configuração '{key}' não encontrada");
            
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração {Key}", key);
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Define uma configuração
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetConfigAsync([FromBody] SetConfigRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Key))
                return BadRequest("Chave da configuração é obrigatória");
            
            // Validar valor
            var isValid = await _configService.ValidateConfigAsync(request.Key, request.Value);
            if (!isValid)
                return BadRequest("Valor inválido para a configuração");
            
            var config = await _configService.SetConfigAsync(request.Key, request.Value, request.Type);
            _logger.LogInformation("Configuração atualizada: {Key} = {Value}", request.Key, request.Value);
            
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir configuração");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Remove uma configuração
    /// </summary>
    [HttpDelete("{key}")]
    public async Task<IActionResult> RemoveConfigAsync(string key)
    {
        try
        {
            var success = await _configService.RemoveConfigAsync(key);
            
            if (success)
            {
                _logger.LogInformation("Configuração removida: {Key}", key);
                return Ok(new { message = "Configuração removida com sucesso" });
            }
            else
            {
                return NotFound($"Configuração '{key}' não encontrada");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover configuração");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Carrega configurações padrão
    /// </summary>
    [HttpPost("defaults")]
    public async Task<IActionResult> LoadDefaultConfigsAsync()
    {
        try
        {
            await _configService.LoadDefaultConfigsAsync();
            _logger.LogInformation("Configurações padrão carregadas");
            
            return Ok(new { message = "Configurações padrão carregadas com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar configurações padrão");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém configurações específicas para a câmera
    /// </summary>
    [HttpGet("camera")]
    public async Task<IActionResult> GetCameraConfigsAsync()
    {
        try
        {
            var cameraConfigs = new
            {
                cameraUrl = await _configService.GetConfigValueAsync<string>(SystemConfig.Keys.CameraUrl),
                videoWidth = await _configService.GetConfigValueAsync<int>(SystemConfig.Keys.VideoWidth),
                videoHeight = await _configService.GetConfigValueAsync<int>(SystemConfig.Keys.VideoHeight),
                frameRate = await _configService.GetConfigValueAsync<int>(SystemConfig.Keys.FrameRate)
            };
            
            return Ok(cameraConfigs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configurações da câmera");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtém configurações específicas para o modelo
    /// </summary>
    [HttpGet("model")]
    public async Task<IActionResult> GetModelConfigsAsync()
    {
        try
        {
            var modelConfigs = new
            {
                modelPath = await _configService.GetConfigValueAsync<string>(SystemConfig.Keys.ModelPath),
                confidenceThreshold = await _configService.GetConfigValueAsync<float>(SystemConfig.Keys.ConfidenceThreshold),
                maxTrackingDistance = await _configService.GetConfigValueAsync<float>(SystemConfig.Keys.MaxTrackingDistance),
                trackingTimeout = await _configService.GetConfigValueAsync<int>(SystemConfig.Keys.TrackingTimeout)
            };
            
            return Ok(modelConfigs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configurações do modelo");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request para definir configuração
/// </summary>
public class SetConfigRequest
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public ConfigType Type { get; set; } = ConfigType.String;
}
