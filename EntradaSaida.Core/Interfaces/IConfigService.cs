using EntradaSaida.Core.Models;

namespace EntradaSaida.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de configuração
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Obtém uma configuração por chave
        /// </summary>
        Task<SystemConfig?> GetConfigAsync(string key);
    
        /// <summary>
        /// Obtém o valor de uma configuração
        /// </summary>
        Task<T?> GetConfigValueAsync<T>(string key);
    
        /// <summary>
        /// Define uma configuração
        /// </summary>
        Task<SystemConfig> SetConfigAsync(string key, object value, ConfigType type = ConfigType.String);
    
        /// <summary>
        /// Remove uma configuração
        /// </summary>
        Task<bool> RemoveConfigAsync(string key);
    
        /// <summary>
        /// Obtém todas as configurações
        /// </summary>
        Task<List<SystemConfig>> GetAllConfigsAsync();
    
        /// <summary>
        /// Carrega configurações padrão
        /// </summary>
        Task LoadDefaultConfigsAsync();
    
        /// <summary>
        /// Valida uma configuração
        /// </summary>
        Task<bool> ValidateConfigAsync(string key, object value);
    }
}
