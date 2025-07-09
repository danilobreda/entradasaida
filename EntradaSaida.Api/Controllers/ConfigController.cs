using EntradaSaida.Core.Models;
using EntradaSaida.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntradaSaida.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(AppDbContext dbContext, ILogger<ConfigController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigsAsync()
        {
            var configs = await _dbContext.SystemConfigs.ToListAsync();
            return Ok(configs);
        }

        [HttpPut]
        public async Task<IActionResult> UpsertConfigAsync([FromBody] SystemConfig config)
        {
            var existing = await _dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Id == config.Id);
            if (existing != null)
            {
                _dbContext.SystemConfigs.Update(existing);
            }
            else
            {
                _dbContext.SystemConfigs.Add(config);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(config);
        }
    }
}
