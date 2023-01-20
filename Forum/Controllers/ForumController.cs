using Microsoft.AspNetCore.Mvc;
using Forum.Models;
using Forum.Models.DataModel;

namespace Forum.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ForumController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<ForumController> _logger;

        public ForumController(ILogger<ForumController> logger)
        {
            _logger = logger;
        }



        [HttpGet("GetUser")]
        public IEnumerable<User> GetUser()
        {
            IEnumerable<User> result = new List<User>();
            using (ForumDBModel s = new ForumDBModel())
            {

                return s.GetUser().ToArray();

            }            
        }
    }
}