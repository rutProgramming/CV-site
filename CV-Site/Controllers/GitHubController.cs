using CV_Site.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CV_Site.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CV_Site.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _service;

        public GitHubController(IGitHubService service)
        {
            _service = service;
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> GetPortfolio()
        {
            var data = await _service.GetPortfolioAsync();
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name, [FromQuery] string language, [FromQuery] string user)
        {
            var data = await _service.SearchRepositoriesAsync(name, language, user);
            return Ok(data);
        }
    }

}
