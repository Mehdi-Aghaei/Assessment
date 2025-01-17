using System.Collections.Concurrent;
using GitHub.Api.Exceptions;
using Octokit;

namespace GitHub.Api.Services;

public class ProcessingService : IProcessingService
{
	private const string Owner = "lodash";

	private const string RepositoryName = "lodash";

	private readonly GitHubClient _client;

	private IOrderedEnumerable<KeyValuePair<char, int>>? _result;

	public ProcessingService(IConfiguration configuration)
	{
		// Important: YOU HAVE TO USE YOU OWN TOKEN, GENERATE IN GITHUB SETTING -> DEVELOPER SETTIN -> GENERATE TOKEN .
		string token = configuration["GithubToken"] is not null ? configuration["GithubToken"]! : "YourToken";

		_client = new GitHubClient(new ProductHeaderValue("Finder"))
		{
			Credentials = new Credentials(token)
		};
	}

	public async Task<IOrderedEnumerable<KeyValuePair<char, int>>> ProcessJsTsFilesAsync()
	{
		try
		{
			// Small Trick for boost performance ;) assuming data will not change so Only first request will take time.
			if (_result is not null)
			{
				return _result;
			}

			var allContents = await _client.Repository.Content.GetAllContents(Owner, RepositoryName);
			var jsTsFiles = await GetJsTsFilesAsync(allContents);
			var mergedContent = await MergeContentAsync(jsTsFiles);

			var letterFrequency = new ConcurrentDictionary<char, int>();
			Parallel.ForEach(mergedContent, character =>
			{
				if (char.IsAsciiLetter(character))
				{
					letterFrequency.AddOrUpdate(character, 1, (_, count) => count + 1);
				}
			});

			_result = letterFrequency.OrderByDescending(kv => kv.Value);

			return _result;
		}
		catch(Octokit.AuthorizationException ex)
		{
			// Example of how to handle exception from outside of the domain by creating proper exceptions (domain errors)
			// we can Have multiple layer exception and have tryCatch in controller -> here is a over kill
			throw new SecurityException(401, ex.Message);
		}
		catch (Exception ex)
		{
			// Wrap rest of Exception to increase security and keep informations in.
			throw new ServiceException(500, ex.Message);
		}
	}

	private async Task<string> MergeContentAsync(RepositoryContent[] jsTsFiles)
	{
		var fileContents = await Task.WhenAll(jsTsFiles.Select(async file =>
		{
			var content = await _client.Repository.Content.GetAllContents(Owner, RepositoryName, file.Path);

			return content[0].Content;
		}));

		return string.Concat(fileContents);
	}

	private async Task<RepositoryContent[]> GetJsTsFilesAsync(IReadOnlyList<RepositoryContent> contents)
	{
		var jsTsFiles = new ConcurrentBag<RepositoryContent>();
		var tasks = new List<Task>();

		await Parallel.ForEachAsync(contents, async (content, _) =>
		{
			if (content.Type == ContentType.File && (content.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || content.Name.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)))
			{
				jsTsFiles.Add(content);
			}
			else if (content.Type == ContentType.Dir)
			{
				var subContents = await _client.Repository.Content.GetAllContents(Owner, RepositoryName, content.Path);
				tasks.Add(GetJsTsFilesAsync(subContents).ContinueWith(t =>
				{
					foreach (var subFile in t.Result)
					{
						jsTsFiles.Add(subFile);
					}
				}, _));
			}
		});

		await Task.WhenAll(tasks);

		return [.. jsTsFiles];
	}
}