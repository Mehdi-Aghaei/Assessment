using Octokit;

namespace GitHub.Api.Services;

public class ProcessingService : IProcessingService
{
	private const string Owner = "lodash";

	private const string RepositoryName = "lodash";

	private readonly GitHubClient _client;

	private IOrderedEnumerable<KeyValuePair<char, int>>? _result;

	public ProcessingService()
	{
		// Important: The Credentials is Valid for 30 days and its a bad practice to have key in code only used for Assignment.
		_client = new GitHubClient(new ProductHeaderValue("Finder"))
		{
			Credentials = new Credentials("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")
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

			var repository = await _client.Repository.Get(Owner, RepositoryName);
			var contents = await _client.Repository.Content.GetAllContents(Owner, RepositoryName);
			List<RepositoryContent> jsTsFiles = await GetJsTsFilesAsync(contents);

			Console.WriteLine($"Found {jsTsFiles.Count} JavaScript/TypeScript file(s).");

			var letterFrequency = new Dictionary<char, int>();
			foreach (var file in jsTsFiles)
			{
				var content = await _client.Repository.Content.GetAllContents(Owner, RepositoryName, file.Path);

				var fileContent = content[0].Content;
				foreach (var c in fileContent.AsSpan())
				{
					if (char.IsAsciiLetter(c))
					{
						if (!letterFrequency.TryGetValue(c, out int count))
						{
							letterFrequency[c] = count;
						}

						letterFrequency[c] = ++count;
					}
				}
			}

			_result = letterFrequency.OrderByDescending(kv => kv.Value);

			return _result;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			throw;
		}
	}

	private async Task<List<RepositoryContent>> GetJsTsFilesAsync(IReadOnlyList<RepositoryContent> contents)
	{
		var jsTsFiles = new List<RepositoryContent>();
		foreach (var content in contents)
		{
			if (content.Type == ContentType.File && (content.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || content.Name.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)))
			{
				jsTsFiles.Add(content);
			}
			else if (content.Type == ContentType.Dir)
			{
				var subContents = await _client.Repository.Content.GetAllContents(Owner, RepositoryName, content.Path);
				var subFiles = await GetJsTsFilesAsync(subContents);

				jsTsFiles.AddRange(subFiles);
			}
		}

		return jsTsFiles;
	}
}