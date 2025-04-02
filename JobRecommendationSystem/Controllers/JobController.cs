using JobRecommendationSystem.Data;
using JobRecommendationSystem.Models;
using JobRecommendationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobRecommendationSystem.Controllers
{
    // JobController: Handles job-related API requests
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/jobs/{id}: Fetches a single job by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJobById(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        // POST api/jobs: Adds a new job to the database
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
        }

    }
}
