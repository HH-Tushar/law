using Microsoft.AspNetCore.Mvc;


namespace MyAwesomeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        // **Simulated Database** - Replace this with a real database context (like Entity Framework Core) for a production app.
        private static List<Profile> _profiles = new List<Profile>
        {
            new Profile { Id = 1, Name = "Jane Doe", Email = "jane.doe@example.com" }
        };
        private static int _nextId = 2; // Tracks the next ID for new profiles

        // --- 1. CREATE (POST) ---
        // POST: api/Profiles
        [HttpPost]
        public ActionResult<Profile> PostProfile(Profile profile)
        {
            // Assign a new ID and set creation time
            profile.Id = _nextId++;
            profile.CreatedAt = DateTime.UtcNow;

            _profiles.Add(profile);

            // Returns HTTP 201 Created status
            return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, profile);
        }

        // --- 2. READ All (GET) ---
        // GET: api/Profiles
        [HttpGet]
        public ActionResult<IEnumerable<Profile>> GetProfiles()
        {
            return Ok(_profiles); // Returns HTTP 200 OK
        }

        // --- 2. READ Single (GET) ---
        // GET: api/Profiles/5
        [HttpGet("{id}")]
        public ActionResult<Profile> GetProfile(int id)
        {
            var profile = _profiles.FirstOrDefault(p => p.Id == id);

            if (profile == null)
            {
                return NotFound(); // Returns HTTP 404 Not Found
            }

            return Ok(profile); // Returns HTTP 200 OK
        }

        // --- 3. UPDATE (PUT) ---
        // PUT: api/Profiles/5
        [HttpPut("{id}")]
        public IActionResult PutProfile(int id, Profile profile)
        {
            if (id != profile.Id)
            {
                return BadRequest(); // Returns HTTP 400 Bad Request
            }

            var existingProfile = _profiles.FirstOrDefault(p => p.Id == id);
            if (existingProfile == null)
            {
                return NotFound(); // Returns HTTP 404 Not Found
            }

            // Update properties
            existingProfile.Name = profile.Name;
            existingProfile.Email = profile.Email;
            existingProfile.ProfileImageUrl = profile.ProfileImageUrl;
            // Note: We typically don't allow changing Id or CreatedAt during an update

            return NoContent(); // Returns HTTP 204 No Content (Successful update)
        }

        // --- 4. DELETE (DELETE) ---
        // DELETE: api/Profiles/5
        [HttpDelete("{id}")]
        public IActionResult DeleteProfile(int id)
        {
            var profileToRemove = _profiles.FirstOrDefault(p => p.Id == id);

            if (profileToRemove == null)
            {
                return NotFound(); // Returns HTTP 404 Not Found
            }

            _profiles.Remove(profileToRemove);

            return NoContent(); // Returns HTTP 204 No Content (Successful deletion)
        }
    }
}