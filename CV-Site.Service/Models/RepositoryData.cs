using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV_Site.Service.Models
{
    public class RepositoryData
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Stars { get; set; }
        public string LastCommit { get; set; }
        public int PullRequests { get; set; }
        public List<string> Languages { get; set; }
    }
}
