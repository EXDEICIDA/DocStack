using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using System.Windows.Media;
using System.Threading;

namespace DocStack.MVVM.Model
{
    public class StatsModel
    {
        private const string BaseUrl = "https://api.openalex.org";
        private readonly HttpClient _client;
        private readonly TimeSpan _delayBetweenRequests = TimeSpan.FromMilliseconds(250);
        private readonly SemaphoreSlim _semaphore;
        // 250ms delay between requests
        private DateTime _lastRequestTime = DateTime.MinValue;
        public StatsModel()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "DocStack");
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public SeriesCollection CreateCitationChart(List<FieldStats> stats)
        {
            return new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Citation Count",
                    Values = new ChartValues<int>(stats.Select(s => s.CitationCount)),
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.MediumSeaGreen
                }
            };
        }

        public SeriesCollection CreateCombinedChart(List<FieldStats> stats)
        {
            return new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Paper Count",
            Values = new ChartValues<int>(stats.Select(s => s.PaperCount)),
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#4361EE"),
            DataLabels = false,
            MaxColumnWidth = 40,  // Reduced from 50
            ColumnPadding = 2     // Reduced from 5
        },
        new ColumnSeries
        {
            Title = "Citation Count",
            Values = new ChartValues<int>(stats.Select(s => s.CitationCount)),
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#3CAEA3"),
            DataLabels = false,
            MaxColumnWidth = 40,  // Reduced from 50
            ColumnPadding = 2     // Reduced from 5
        }
    };
        }

        public SeriesCollection CreatePaperCountChart(List<FieldStats> stats)
        {
            return new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Paper Count",
                    Values = new ChartValues<int>(stats.Select(s => s.PaperCount)),
                    DataLabels = true,
                    Fill = System.Windows.Media.Brushes.DodgerBlue
                }
            };
        }

        public async Task<List<FieldStats>> GetFieldStatsAsync()
        {
            var currentYear = DateTime.Now.Year;
            var stats = new List<FieldStats>();
            var fieldsProcessed = 0;

            try
            {
                // Get top fields based on works count
                var fieldsUrl = $"{BaseUrl}/concepts?filter=level:0&sort=works_count:desc&per-page=10";
                var response = await MakeRequestWithRetryAsync(fieldsUrl);
                var fieldsData = JsonDocument.Parse(response);
                var results = fieldsData.RootElement.GetProperty("results");

                foreach (var field in results.EnumerateArray())
                {
                    try
                    {
                        var fieldId = field.GetProperty("id").GetString();
                        var fieldName = field.GetProperty("display_name").GetString();

                        // Get papers count
                        var worksUrl = $"{BaseUrl}/works?filter=concept.id:{fieldId},publication_year:{currentYear}";
                        var worksResponse = await MakeRequestWithRetryAsync(worksUrl);
                        var worksData = JsonDocument.Parse(worksResponse);
                        var paperCount = worksData.RootElement.GetProperty("meta").GetProperty("count").GetInt32();

                        // Get citation count
                        var citationsUrl = $"{BaseUrl}/works?filter=concept.id:{fieldId},publication_year:{currentYear}&select=cited_by_count";
                        var citationsResponse = await MakeRequestWithRetryAsync(citationsUrl);
                        var citationsData = JsonDocument.Parse(citationsResponse);
                        var citationCount = citationsData.RootElement
                            .GetProperty("results")
                            .EnumerateArray()
                            .Sum(work => work.GetProperty("cited_by_count").GetInt32());

                        stats.Add(new FieldStats
                        {
                            Field = fieldName,
                            PaperCount = paperCount,
                            CitationCount = citationCount
                        });

                        fieldsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing field: {ex.Message}");
                        // Continue with next field instead of breaking
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching field stats: {ex.Message}");
                if (fieldsProcessed > 0)
                {
                    // Return partial data if we have any
                    return stats;
                }
                throw;
            }

            return stats;
        }

        private async Task<string> MakeRequestWithRetryAsync(string url, int maxRetries = 3)
        {
            await _semaphore.WaitAsync();
            try
            {
                var timeToWait = _delayBetweenRequests - (DateTime.Now - _lastRequestTime);
                if (timeToWait > TimeSpan.Zero)
                {
                    await Task.Delay(timeToWait);
                }

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        _lastRequestTime = DateTime.Now;
                        return await _client.GetStringAsync(url);
                    }
                    catch (HttpRequestException ex) when (i < maxRetries - 1)
                    {
                        await Task.Delay((i + 1) * 1000); // Exponential backoff
                        continue;
                    }
                }
                throw new Exception($"Failed to fetch data after {maxRetries} attempts");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public class FieldStats
        {
            public int CitationCount { get; set; }
            public string Field { get; set; }
            public int PaperCount { get; set; }
        }
    }
}