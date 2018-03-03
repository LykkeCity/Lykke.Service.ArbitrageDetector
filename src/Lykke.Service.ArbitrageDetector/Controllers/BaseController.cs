using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : Controller
    {
    }
}
