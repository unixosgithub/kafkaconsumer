using Microsoft.AspNetCore.Mvc;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace kafkaconsumer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class healthcheckController : ControllerBase
    {
        // GET: api/<healthcheckController>
        [HttpGet]
        public HttpStatusCode Get()
        {
            return HttpStatusCode.OK;
        }        
    }
}
