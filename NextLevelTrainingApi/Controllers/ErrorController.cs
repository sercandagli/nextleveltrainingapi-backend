using System;
using NextLevelTrainingApi.ViewModels;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NextLevelTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ErrorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpPost]
        [Route("CreateErrorLog")]
        public IActionResult Error(CreateErrorViewModel errorVm)
        {
            ErrorLog error = new ErrorLog();
            error.Id = Guid.NewGuid();
            error.Exception = errorVm.Exception;
            error.StackTrace = errorVm.StackTrace;
            error.CreatedDate = DateTime.Now;
            _unitOfWork.ErrorLogRepository.InsertOne(error);
            return Ok();
        }
    }
}
