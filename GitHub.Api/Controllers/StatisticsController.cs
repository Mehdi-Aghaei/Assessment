using GitHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GitHub.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class StatisticsController(IProcessingService processingService) : ControllerBase
	{
		private readonly IProcessingService _processingService = processingService;

		[HttpGet]
		public async Task<IEnumerable<LetterStatistic>> GetLettersStatistics()
		{
			var processedPairs = await _processingService.ProcessJsTsFilesAsync();
			return processedPairs.Select(a => new LetterStatistic(a.Key, a.Value)).ToList();
		}
	}

	public record LetterStatistic(char Letter, int Count);
}