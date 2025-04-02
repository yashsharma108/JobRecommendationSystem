using JobRecommendationSystem.Data;
using JobRecommendationSystem.Models;
using JobRecommendationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobRecommendationSystem.Controllers
{
    [Route("api/recommendations")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RecommendationService _recommendationService;
        private readonly OpenAIService _openAIService;

        public RecommendationController(AppDbContext context, RecommendationService recommendationService, OpenAIService openAIService)
        {
            _context = context;
            _recommendationService = recommendationService;
            _openAIService = openAIService;
        }

        [HttpPost("database")] // Get job recommendations from database
        public async Task<ActionResult<List<Job>>> GetDatabaseRecommendations([FromBody] UserProfile userProfile)
        {
            if (userProfile == null)
            {
                return BadRequest("User profile is required.");
            }

            var jobs = await _context.Jobs.ToListAsync();
            if (jobs == null || jobs.Count == 0)
            {
                return NotFound("No jobs found in the database.");
            }

            var recommendedJobs = await _recommendationService.GetRecommendedJobs(userProfile, jobs);
            return Ok(recommendedJobs);
        }

        [HttpPost("ai")] // Get job recommendations from OpenAI
        public async Task<IActionResult> GetAIRecommendations([FromBody] UserProfile userProfile)
        {
            if (userProfile == null)
            {
                return BadRequest("User profile is required.");
            }

            string response = await _openAIService.GetResponse();
            return Ok(response);
        }
    }
}
