using CommandLine;
using Octokit;

namespace GithubDownloadStats
{
    public class CLIArguments
    {
        [Value(0, MetaName = "repo_owner", HelpText = "Name of the repository owner.", Required = true)]
        public string RepoOwner { get; set; }

        [Value(1, MetaName = "repo_name", HelpText = "Name of the repository.", Required = true)]
        public string RepoName { get; set; }

        [Option('t', "tag", HelpText = "Provide a tag to select one release.", Required = false)]
        public string Tag { get; set; }
    }

    public static class Program
    { 
        private static void DisplayReleases(IReadOnlyList<Release> releases)
        {
            var totalDownloadCount = 0;

            foreach (var release in releases)
            {
                Console.WriteLine($"\n---------\n{release.Name}{(release.Prerelease ? " [PRERELEASE]" : "")} [{release.PublishedAt:MM-dd-yyyy}]\n---------\n");
                Console.WriteLine("PUBLISH DATE | RELEASE TAG | FILE NAME | DOWNLOAD COUNT\n");

                foreach (var asset in release.Assets)
                {
                    Console.WriteLine($"{asset.UpdatedAt:MM-dd-yyyy} | {release.TagName} | {asset.Name} | {asset.DownloadCount}");

                    totalDownloadCount += asset.DownloadCount;
                }
            }

            
            Console.WriteLine($"\nTOTAL DOWNLOAD COUNT: {totalDownloadCount}");
        }

        private static void ListAssetsFromAllReleases(CLIArguments arguments, GitHubClient gitHubClient)
        {
            var releases = gitHubClient.Repository.Release.GetAll(arguments.RepoOwner, arguments.RepoName).Result;
            DisplayReleases(releases);
        }

        private static void ListAssetsFromOneRelease(CLIArguments arguments, GitHubClient gitHubClient)
        {
            var releases = gitHubClient.Repository.Release.GetAll(arguments.RepoOwner, arguments.RepoName).Result;
            var releases_filtered = releases.Where(r => r.TagName.Equals(arguments.Tag, StringComparison.OrdinalIgnoreCase)).ToList();

            DisplayReleases(releases_filtered);
        }

        public static void Run(CLIArguments arguments)
        {
            var github = new GitHubClient(new ProductHeaderValue("GithubDownloadStats"));

            if(string.IsNullOrEmpty(arguments.Tag))
            {
                ListAssetsFromAllReleases(arguments, github);
            }
            else
            {
                ListAssetsFromOneRelease(arguments, github);
            }
        }

        public static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.WriteLine("One or more parser error:\n");
            
            foreach (var error in errors)
            {
                Console.WriteLine($"[{error.Tag}]");
            }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CLIArguments>(args)
                          .WithParsed(Run)
                          .WithNotParsed(HandleParseError);
        }
    }
}