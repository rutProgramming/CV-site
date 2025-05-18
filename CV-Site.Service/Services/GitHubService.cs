using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CV_Site.Service.Interfaces;
using CV_Site.Service.Models;

namespace CV_Site.Service.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly GitHubOptions _options;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "GitHubPortfolioCache";
        private const string CacheLastUpdatedKey = "GitHubPortfolioLastUpdated";

        public GitHubService(IOptions<GitHubOptions> options, IMemoryCache cache)
        {
            _options = options.Value;
            _cache = cache;
            _client = new GitHubClient(new ProductHeaderValue("DevPortfolio"))
            {
                Credentials = new Credentials(_options.Token)
            };
        }

        public async Task<IEnumerable<RepositoryData>> GetPortfolioAsync()
        {
            if (_cache.TryGetValue(CacheKey, out IEnumerable<RepositoryData> cachedData) &&
                _cache.TryGetValue(CacheLastUpdatedKey, out DateTime lastUpdated))
            {
                var repos = await _client.Repository.GetAllForUser(_options.Username);
                var latestPush = repos.Max(r => r.PushedAt ?? DateTimeOffset.MinValue);

                if (latestPush <= lastUpdated)
                {
                    return cachedData;
                }
            }

            var freshRepos = await _client.Repository.GetAllForUser(_options.Username);
            var result = new List<RepositoryData>();

            foreach (var repo in freshRepos)
            {
                try
                {
                    var languages = await _client.Repository.GetAllLanguages(_options.Username, repo.Name);
                    var pulls = await _client.PullRequest.GetAllForRepository(_options.Username, repo.Name);

                    result.Add(new RepositoryData
                    {
                        Name = repo.Name,
                        Url = repo.HtmlUrl,
                        Stars = repo.StargazersCount,
                        LastCommit = repo.PushedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                        PullRequests = pulls.Count,
                        Languages = languages.Select(l => l.Name).ToList()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"שגיאה בעיבוד ריפו {repo.Name}: {ex.Message}");
                }
            }

            var maxPushDate = freshRepos.Max(r => r.PushedAt ?? DateTimeOffset.MinValue).UtcDateTime;
            _cache.Set(CacheKey, result, TimeSpan.FromMinutes(30));
            _cache.Set(CacheLastUpdatedKey, maxPushDate);

            return result;
        }

        public async Task<IEnumerable<RepositoryData>> SearchRepositoriesAsync(string name, string language, string user)
        {
            var request = new SearchRepositoriesRequest(name ?? string.Empty)
            {
                Language = TryParseLanguage(language),
                User = string.IsNullOrWhiteSpace(user) ? null : user
            };

            var searchResults = await _client.Search.SearchRepo(request);

            var result = new List<RepositoryData>();

            foreach (var repo in searchResults.Items)
            {
                try
                {
                    var languages = await _client.Repository.GetAllLanguages(_options.Username, repo.Name);
                    var pulls = await _client.PullRequest.GetAllForRepository(_options.Username, repo.Name);

                    result.Add(new RepositoryData
                    {
                        Name = repo.Name,
                        Url = repo.HtmlUrl,
                        Stars = repo.StargazersCount,
                        LastCommit = repo.PushedAt?.ToString("yyyy-MM-dd") ?? "N/A",
                        PullRequests = pulls.Count,
                        Languages = languages.Select(l => l.Name).ToList()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"שגיאה בעיבוד ריפו {repo.Name}: {ex.Message}");
                }
            }

            return result;
        }

        private Octokit.Language? TryParseLanguage(string language)
        {
            if (!string.IsNullOrWhiteSpace(language))
            {
                if (Enum.TryParse<Octokit.Language>(language, true, out var langEnum))
                {
                    return langEnum;
                }
            }
            return null;
        }
    }
}
