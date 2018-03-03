using Lykke.Service.ArbitrageDetector.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    [Route("api/[controller]")]
    public class ArbitrageDetectorController : Controller
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
    }
}
