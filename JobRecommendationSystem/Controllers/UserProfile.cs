using JobRecommendationSystem.Data;
using JobRecommendationSystem.Models;
using JobRecommendationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobRecommendationSystem.Controllers
{
    
    [Route("api/userprofiles")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/userprofiles/{id}: Fetches a single user profile by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileById(int id)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id);

            if (userProfile == null)
            {
                return NotFound();
            }

            return userProfile;
        }

        // POST api/userprofiles: Adds a new user profile to the database
        [HttpPost]
        public async Task<ActionResult<UserProfile>> PostUserProfile(UserProfile userProfile)
        {
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserProfileById), new { id = userProfile.Id }, userProfile);
        }

    }
}
