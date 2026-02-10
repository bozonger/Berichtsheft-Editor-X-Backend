using Berichtsheft_Editor_X.Models;

using Berichtsheft_Editor_X_API;
using Berichtsheft_Editor_X_API.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Security.Claims;
using System.Text;

namespace Berichtsheft_Editor_X.Controllers
{
    [Authorize]
    [Route("api/berichtsheft")]
    [ApiController]
    public class BeXController : ControllerBase
    {
        private AppDbContext _DbContext;
        PasswordHasher<string> _Hasher = new();
        private AuthManager _AuthManager;
        public CustomSettings _CustomSettings;
        private string _Env;

        public BeXController(AppDbContext context, AuthManager authmanager, CustomSettings customSettings)
        {
            _DbContext = context;
            _AuthManager = authmanager;
            _CustomSettings = customSettings;
             _Env = _CustomSettings.UploadFolderPath;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<string> Login([FromForm] string username, [FromForm] string password)
        {
            User? user = _DbContext.User.Where(x => x.Username == username).FirstOrDefault();

            if (user == null)
            {
                return Unauthorized("Wrong credentials");
            }

            var hashedPassword = user.Password;
            var verification = _Hasher.VerifyHashedPassword(username, hashedPassword, password);

            if(verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
               var token = _AuthManager.GenerateToken(user);
                return Ok(token);
            }
            else
            {
                return Unauthorized("Wrong credentials");
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("test")]
        public IActionResult Test()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Ok("Good");
            }

            return Unauthorized("Ne");
        }

        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public IActionResult Register([FromForm] string password, [FromForm] string username)
        {
            if (_DbContext.User.Any(x => x.Username == username))
            {
                return BadRequest("User already exists");
            }

            string hashedPassword = _Hasher.HashPassword(username, password);

            User user = new User();

            user.Password = hashedPassword;
            user.Username = username;

            user.Roles = "DefaultClaim";
            
            _DbContext.User.Add(user);
            _DbContext.SaveChanges();

            return Ok("Registered successfully");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("saveFile")]
        [HttpPost]
        public async Task<IActionResult> SaveFile([FromForm] string? wochenBericht)
        {
                if (wochenBericht == null)
            {
                return BadRequest("No data received.");
            }

            var userId = User.FindFirst(ClaimTypes.Name).Value;
            if (userId == null)
            {
                return Unauthorized("No user found");
            }

            var relatedUser = _DbContext.User.FirstOrDefault(x => x.Username == userId);

            Wochenbericht berichtObj = JsonConvert.DeserializeObject<Wochenbericht>(wochenBericht);

            byte[] fileBytes = Encoding.UTF8.GetBytes(wochenBericht);

            string uploadsFolder = Path.Combine(_Env, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = $"user_{userId}_week_{berichtObj.KalenderWoche}_year_{berichtObj.Jahr}.json";
            string filePath = Path.Combine(uploadsFolder, fileName);

            Bericht bericht = new Bericht()
            {
                fileName = fileName,
                UserId = relatedUser.Id
            };

            _DbContext.Bericht.Add(bericht);
            _DbContext.SaveChanges();

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            return Ok("File saved");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("saveFileSeparate")]
        [HttpPost]
        public async Task<IActionResult> SaveFileSeparate([FromForm] WochenberichtForm form)
        {
            if (form == null)
            {
                return BadRequest("No data received.");
            }

            var userId = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userId == null)
            {
                return Unauthorized("No user found");
            }

            var relatedUser = _DbContext.User.FirstOrDefault(x => x.Username == userId);

            // Serialize the form into JSON so we can save it as a file
            byte[] fileBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(form));

            string uploadsFolder = Path.Combine(_Env, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = $"user_{userId}_week_{form.KalenderWoche}_year_{form.Jahr}.json";
            string filePath = Path.Combine(uploadsFolder, fileName);

            Bericht bericht = new Bericht()
            {
                fileName = fileName,
                UserId = relatedUser.Id
            };

            _DbContext.Bericht.Add(bericht);
            _DbContext.SaveChanges();

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            return Ok("File saved");
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("getFile")]
        [HttpPost]
        public async Task<IActionResult> GetFileForUser([FromForm] string calenderWeek, [FromForm] string year)
        {
            var userId = User.FindFirst(ClaimTypes.Name).Value;
            if (userId == null)
            {
                return Unauthorized("No user found");
            }

            var relatedUser = _DbContext.User.FirstOrDefault(x => x.Username == userId);

            Bericht bericht = _DbContext.Bericht.FirstOrDefault(x => x.UserId == relatedUser.Id && x.fileName.Contains("week_" + calenderWeek + "_year_" + year));

            if(bericht == null)
            {
                return BadRequest("No file found with the given name");
            }

            string uploadsFolder = Path.Combine(_Env, "uploads");
            string berichtLocation = Path.Combine(uploadsFolder, bericht.fileName);
            string fileContent = await System.IO.File.ReadAllTextAsync(berichtLocation);

            return Ok(fileContent);
        }
    }
}