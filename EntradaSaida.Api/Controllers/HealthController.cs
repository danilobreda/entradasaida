using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EntradaSaida.Api.Controllers
{
    /// <summary>
    /// Controller para health checks e status do sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Verifica a saúde geral do sistema
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                var health = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    uptime = DateTime.UtcNow - process.StartTime,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    system = new
                    {
                        os = Environment.OSVersion.ToString(),
                        platform = Environment.OSVersion.Platform.ToString(),
                        architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                        dotnetVersion = Environment.Version.ToString(),
                        workingSet = process.WorkingSet64,
                        gcMemory = GC.GetTotalMemory(false),
                        threadCount = process.Threads.Count
                    },
                    dependencies = new
                    {
                        database = "available", // TODO: Implementar check real
                        filesystem = Directory.Exists("models") ? "available" : "models folder missing",
                        opencv = "available" // TODO: Implementar check real
                    }
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no health check");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }
    }
}
