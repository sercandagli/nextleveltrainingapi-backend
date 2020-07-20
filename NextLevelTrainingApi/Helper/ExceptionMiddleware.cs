using Microsoft.AspNetCore.Http;
using NextLevelTrainingApi.DAL.Entities;
using NextLevelTrainingApi.DAL.Interfaces;
using NextLevelTrainingApi.DAL.Repository;
using NextLevelTrainingApi.Models;
using NextLevelTrainingApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.Helper
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private INextLevelDBSettings _settings;
        public ExceptionMiddleware(RequestDelegate next, INextLevelDBSettings settings)
        {
            _next = next;
            _settings = settings;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                IUnitOfWork _unitOfWork = new UnitOfWork(_settings);
                ErrorLog error = new ErrorLog();
                error.Id = Guid.NewGuid();
                error.Exception = ex.Message;
                error.StackTrace = ex.StackTrace;
                _unitOfWork.ErrorLogRepository.InsertOne(error);

                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string json = JsonSerializer.Serialize(new ErrorViewModel()
            {
                errors = new Error() { error = new string[] { exception.Message } },
                status = (int)HttpStatusCode.InternalServerError
            }); ;
            return context.Response.WriteAsync(json, Encoding.UTF8);
        }
    }
}
