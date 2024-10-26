using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace DocStack.MVVM.Model
{
    public class KeywordManager
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<string, List<string>> _categoryKeywords = new Dictionary<string, List<string>>
        {
            ["AI & ML"] = new List<string>
            {
                "machine learning", "artificial intelligence", "deep learning", "neural networks",
                "natural language processing", "computer vision", "reinforcement learning",
                "transfer learning", "generative ai", "federated learning", "transformer models",
                "autonomous systems", "machine learning applications", "AI ethics"
            },
            ["Data Science"] = new List<string>
            {
                "data science", "big data", "data mining", "data visualization",
                "statistical analysis", "predictive modeling", "data engineering",
                "time series analysis", "anomaly detection", "data preprocessing",
                "feature engineering", "dimensional reduction", "clustering algorithms"
            },
            ["Computer Science"] = new List<string>
            {
                "algorithms", "distributed systems", "cloud computing", "cybersecurity",
                "software engineering", "parallel computing", "computer architecture",
                "microservices", "devops", "system design", "performance optimization",
                "containerization", "edge computing", "serverless architecture"
            },
            ["Physics"] = new List<string>
            {
                "quantum computing", "quantum mechanics", "particle physics",
                "theoretical physics", "astrophysics", "condensed matter", "plasma physics",
                "quantum entanglement", "string theory", "dark matter", "cosmology",
                "quantum field theory", "quantum cryptography"
            },
            ["Mathematics"] = new List<string>
            {
                "topology", "complex analysis", "number theory", "abstract algebra",
                "differential geometry", "mathematical optimization", "graph theory",
                "category theory", "algebraic geometry", "functional analysis",
                "discrete mathematics", "numerical methods", "chaos theory"
            },
            ["Interdisciplinary"] = new List<string>
            {
                "bioinformatics", "computational biology", "quantum chemistry",
                "cognitive science", "robotics", "blockchain technology", "internet of things",
                "computational neuroscience", "systems biology", "synthetic biology",
                "biomedical engineering", "computational physics", "mathematical biology"
            }
        };

        public string[] GetRandomKeywordsFromCategories(int keywordsPerCategory = 3)
        {
            var selectedKeywords = new List<string>();
            foreach (var category in _categoryKeywords.Values)
            {
                selectedKeywords.AddRange(
                    category.OrderBy(x => _random.Next())
                           .Take(keywordsPerCategory)
                );
            }
            return selectedKeywords.OrderBy(x => _random.Next()).ToArray();
        }

        public string[] GetRandomKeywords(int count = 15)
        {
            return _categoryKeywords.Values
                .SelectMany(x => x)
                .OrderBy(x => _random.Next())
                .Take(count)
                .ToArray();
        }
    }


    public class RecommenderModel
    {
        private readonly CallConfigurationModel _apiClient;
        private readonly Random _random = new Random();
        private const int MaxResultsPerKeyword = 5;
        private const int DefaultTotalLimit = 30;

        public RecommenderModel()
        {
            _apiClient = new CallConfigurationModel();
        }

        public async Task<List<RecommendedPaper>> GetRecommendationsAsync(string[] keywords, int limit = DefaultTotalLimit)
        {
            var recommendations = new List<RecommendedPaper>();
            var processedDOIs = new HashSet<string>(); // Track unique papers

            try
            {
                // Shuffle keywords for randomization
                keywords = keywords.OrderBy(x => _random.Next()).ToArray();

                foreach (var keyword in keywords)
                {
                    if (recommendations.Count >= limit) break;

                    var results = await _apiClient.SearchPapersAsync(keyword);
                    if (results == null || !results.Any()) continue;

                    // Randomly select papers from results
                    var selectedResults = results
                        .OrderBy(x => _random.Next())
                        .Take(MaxResultsPerKeyword);

                    foreach (var result in selectedResults)
                    {
                        try
                        {
                            string doi = result["doi"]?.ToString() ?? string.Empty;

                            // Skip if we've already processed this DOI
                            if (!string.IsNullOrEmpty(doi) && processedDOIs.Contains(doi))
                                continue;

                            var paper = new RecommendedPaper
                            {
                                Title = result["title"]?.ToString()?.Trim() ?? "Untitled",
                                Abstract = result["abstract"]?.ToString()?.Trim() ?? "No abstract available",
                                Year = (result["yearPublished"] != null && int.TryParse(result["yearPublished"].ToString(), out int year)) ? year : 0,
                                FullTextLink = result["downloadUrl"]?.ToString() ?? string.Empty,
                                Doi = doi,
                                Keywords = new[] { keyword }  // Store the keyword that found this paper
                            };

                            // Only add if it's a unique paper
                            if (!string.IsNullOrEmpty(doi))
                                processedDOIs.Add(doi);

                            recommendations.Add(paper);
                            Debug.WriteLine($"Added paper: {paper.Title} (Keyword: {keyword})");

                            if (recommendations.Count >= limit)
                                break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing paper: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetRecommendationsAsync: {ex.Message}");
            }

            // Final shuffle and return limited results
            return recommendations
                .OrderBy(x => _random.Next())
                .Take(limit)
                .ToList();
        }

        public class RecommendedPaper
        {
            public string Abstract { get; set; } = string.Empty;
            public string Authors { get; set; } = string.Empty;
            public string Doi { get; set; } = string.Empty;
            public string FullTextLink { get; set; }
            public string Title { get; set; } = string.Empty;
            public int Year { get; set; }
            public string[] Keywords { get; set; } = Array.Empty<string>();
        }
    }
}