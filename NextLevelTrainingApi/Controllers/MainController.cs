using Microsoft.AspNetCore.Mvc;

namespace NextLevelTrainingApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MainController: ControllerBase
    {
        public MainController()
        {
        }

        [HttpGet]
        [Route("AppLink")]
        public ContentResult AppLink()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = @"
                    <script type='text/javascript'>
                        window.location.href = 'nextlevel://app'
                    </script>"
            };
        }
    }
}
