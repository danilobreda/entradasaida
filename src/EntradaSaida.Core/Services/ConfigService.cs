using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;
using System.Text.Json;

namespace EntradaSaida.Core.Services;

/// <summary>
/// Implementação do serviço de configuração
/// </summary>
public class ConfigService : IConfigService
{
    private readonly Dictionary<string, SystemConfig> _configs = new();
    private int _nextId = 1;

    public async Task<SystemConfig?> GetConfigAsync(string key)
    {
        _configs.TryGetValue(key, out var config);
        return await Task.FromResult(config);
    }

    public async Task<T?> GetConfigValueAsync<T>(string key)
    {
        var config = await GetConfigAsync(key);
        if (config == null) return default;

        try
        {
            return config.Type switch
            {
                ConfigType.String => (T)(object)config.Value,
                ConfigType.Integer => (T)(object)int.Parse(config.Value),
                ConfigType.Float => (T)(object)float.Parse(config.Value),
                ConfigType.Boolean => (T)(object)bool.Parse(config.Value),
                ConfigType.Json => JsonSerializer.Deserialize<T>(config.Value),
                _ => default
            };
        }
        catch
        {
            return default;
        }
    }

    public async Task<SystemConfig> SetConfigAsync(string key, object value, ConfigType type = ConfigType.String)
    {
        var stringValue = type switch
        {
            ConfigType.Json => JsonSerializer.Serialize(value),
            _ => value.ToString() ?? string.Empty
        };

        var config = new SystemConfig
        {
            Id = _nextId++,
            Key = key,
            Value = stringValue,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (_configs.ContainsKey(key))
        {
            var existing = _configs[key];
            config.Id = existing.Id;
            config.CreatedAt = existing.CreatedAt;
        }

        _configs[key] = config;
        return await Task.FromResult(config);
    }

    public async Task<bool> RemoveConfigAsync(string key)
    {
        var removed = _configs.Remove(key);
        return await Task.FromResult(removed);
    }

    public async Task<List<SystemConfig>> GetAllConfigsAsync()
    {
        return await Task.FromResult(_configs.Values.ToList());
    }

    public async Task LoadDefaultConfigsAsync()
    {
        var defaults = new Dictionary<string, (object value, ConfigType type)>
        {
            { SystemConfig.Keys.CameraUrl, ("0", ConfigType.String) },
            { SystemConfig.Keys.ModelPath, ("models/yolov8n.onnx", ConfigType.String) },
            { SystemConfig.Keys.ConfidenceThreshold, (0.5f, ConfigType.Float) },
            { SystemConfig.Keys.MaxTrackingDistance, (50.0f, ConfigType.Float) },
            { SystemConfig.Keys.TrackingTimeout, (30, ConfigType.Integer) },
            { SystemConfig.Keys.VideoWidth, (640, ConfigType.Integer) },
            { SystemConfig.Keys.VideoHeight, (480, ConfigType.Integer) },
            { SystemConfig.Keys.FrameRate, (30, ConfigType.Integer) },
            { SystemConfig.Keys.EnableRecording, (false, ConfigType.Boolean) },
            { SystemConfig.Keys.RecordingPath, ("recordings/", ConfigType.String) }
        };

        foreach (var (key, (value, type)) in defaults)
        {
            if (!_configs.ContainsKey(key))
            {
                await SetConfigAsync(key, value, type);
            }
        }
    }

    public async Task<bool> ValidateConfigAsync(string key, object value)
    {
        try
        {
            return key switch
            {
                SystemConfig.Keys.ConfidenceThreshold => value is float f && f >= 0.0f && f <= 1.0f,
                SystemConfig.Keys.MaxTrackingDistance => value is float d && d > 0,
                SystemConfig.Keys.TrackingTimeout => value is int t && t > 0,
                SystemConfig.Keys.VideoWidth => value is int w && w > 0,
                SystemConfig.Keys.VideoHeight => value is int h && h > 0,
                SystemConfig.Keys.FrameRate => value is int fps && fps > 0 && fps <= 60,
                SystemConfig.Keys.CameraUrl => !string.IsNullOrEmpty(value?.ToString()),
                SystemConfig.Keys.ModelPath => !string.IsNullOrEmpty(value?.ToString()),
                SystemConfig.Keys.RecordingPath => !string.IsNullOrEmpty(value?.ToString()),
                SystemConfig.Keys.EnableRecording => value is bool,
                _ => true
            };
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }
}
