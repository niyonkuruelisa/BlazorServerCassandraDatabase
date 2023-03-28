using Cassandra;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BlazorServerWithCassandraDB.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly Cassandra.ISession session;

        public UserController(IMediator mediator, Cassandra.ISession session)
        {
            this.mediator = mediator;
            this.session = session;
        }

        [AllowAnonymous]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            // Use the _session object to interact with Cassandra
            var query = new SimpleStatement("SELECT * FROM users");
            var result = session.Execute(query);

            // Return the result to the view
            return Ok(result);
        }
    }
}
