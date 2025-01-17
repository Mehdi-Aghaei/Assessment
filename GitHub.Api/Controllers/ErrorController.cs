using GitHub.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GitHub.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ErrorController(ILogger<ErrorController> logger) : ControllerBase
	{
		private readonly ILogger<ErrorController> _logger = logger;

		public IActionResult HandleError()
		{
			var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

			if (exception == null)
			{
				return Problem();
			}

			_logger.LogError(exception, "An error occurred.");

			return exception switch
			{
				SecurityException securityException => Problem(
					statusCode: securityException.StatusCode,
					title: securityException.GetType().Name,
					detail: securityException.ErrorMessage),

				ServiceException serviceException => Problem(
					statusCode: serviceException.StatusCode,
					title: serviceException.GetType().Name,
					detail: serviceException.ErrorMessage),

				_ => Problem(
					title: "Unexpected Error",
					detail: exception.Message)
			};
		}
	}
}