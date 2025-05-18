using CV_Site.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV_Site.Service.Interfaces
{
    public interface IGitHubService
    {
        Task<IEnumerable<RepositoryData>> GetPortfolioAsync();
        Task<IEnumerable<RepositoryData>> SearchRepositoriesAsync(string name, string language, string user);

    }
}
