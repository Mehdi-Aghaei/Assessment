namespace GitHub.Api.Services;

public interface IProcessingService
{
	Task<IOrderedEnumerable<KeyValuePair<char, int>>> ProcessJsTsFilesAsync();
}
