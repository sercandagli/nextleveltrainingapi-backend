using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.ViewModels;

namespace NextLevelTrainingApi.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class ApiKeyController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

      
        public ApiKeyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        [Route("GetApiKey")]
        public ActionResult GetApiKey()
        {
            var apiKeyDocument = _unitOfWork.ApiKeyRepository.AsQueryable().FirstOrDefault();
            if(apiKeyDocument == null)
                return BadRequest(new ErrorViewModel() { errors = new Error() { error = new string[] { "Api key not found." } } });

            return new OkObjectResult(apiKeyDocument);

        }
    }
}
