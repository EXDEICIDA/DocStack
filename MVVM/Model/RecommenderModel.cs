using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace DocStack.MVVM.Model
{
    public class RecommenderModel
    {
        private readonly CallConfigurationModel _apiClient;

        public RecommenderModel()
        {
            _apiClient = new CallConfigurationModel();
        }

        public class RecommendedPaper
        {
            public string Title { get; set; } = string.Empty;
            public string[] Authors { get; set; } = Array.Empty<string>();
            public string Abstract { get; set; } = string.Empty;
            public string Year { get; set; } = string.Empty;
            public string Doi { get; set; } = string.Empty;
            public double Score { get; set; } = 1.0;
        }

        public async Task<List<RecommendedPaper>> GetRecommendationsAsync(string[] keywords, int limit = 10)
        {
            var recommendations = new List<RecommendedPaper>();

            try
            {

                // Make separate API calls for each keyword to get more diverse results
                foreach (var keyword in keywords)
                {

                    var results = await _apiClient.SearchPapersAsync(keyword);

                    if (results != null)
                    {

                        // Take top 2 results from each keyword
                        foreach (var result in results.Take(2))
                        {
                            try
                            {
                                var paper = new RecommendedPaper
                                {
                                    Title = result["title"]?.ToString()?.Trim() ?? "Untitled",
                                    Abstract = result["abstract"]?.ToString()?.Trim() ?? "No abstract available",
                                    Year = result["publishedYear"]?.ToString() ?? "N/A"
                                };

                                // Only add if we don't already have this paper
                                if (!recommendations.Any(r => r.Title == paper.Title))
                                {
                                    recommendations.Add(paper);
                                    Debug.WriteLine($"Added paper: {paper.Title}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error processing paper: {ex.Message}");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetRecommendationsAsync: {ex.Message}");
            }

            // Return the top N recommendations, sorted by score
            return recommendations
                .OrderByDescending(r => r.Score)
                .Take(limit)
                .ToList();
        }
    }
}