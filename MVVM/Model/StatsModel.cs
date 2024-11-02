using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using System.Windows.Media;

namespace DocStack.MVVM.Model
{
    public class StatsModel
    {
        private const string BaseUrl = "https://api.openalex.org";
        private readonly HttpClient _client;
        public StatsModel()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "DocStack");
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
                MaxColumnWidth = 50,
                ColumnPadding = 5
            },
            new ColumnSeries
            {
                Title = "Citation Count",
                Values = new ChartValues<int>(stats.Select(s => s.CitationCount)),
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#3CAEA3"),
                DataLabels = false,
                MaxColumnWidth = 50,
                ColumnPadding = 5
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

            try
            {
                // Get top fields based on works count
                var fieldsUrl = $"{BaseUrl}/concepts?filter=level:0&sort=works_count:desc&per-page=10";
                var response = await _client.GetStringAsync(fieldsUrl);
                var fieldsData = JsonDocument.Parse(response);
                var results = fieldsData.RootElement.GetProperty("results");

                foreach (var field in results.EnumerateArray())
                {
                    var fieldId = field.GetProperty("id").GetString();
                    var fieldName = field.GetProperty("display_name").GetString();

                    // Get papers count for current year
                    var worksUrl = $"{BaseUrl}/works?filter=concept.id:{fieldId},publication_year:{currentYear}";
                    var worksResponse = await _client.GetStringAsync(worksUrl);
                    var worksData = JsonDocument.Parse(worksResponse);
                    var paperCount = worksData.RootElement.GetProperty("meta").GetProperty("count").GetInt32();

                    // Get citation count
                    var citationsUrl = $"{BaseUrl}/works?filter=concept.id:{fieldId},publication_year:{currentYear}&select=cited_by_count";
                    var citationsResponse = await _client.GetStringAsync(citationsUrl);
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching field stats: {ex.Message}");
                throw;
            }

            return stats;
        }

        public class FieldStats
        {
            public int CitationCount { get; set; }
            public string Field { get; set; }
            public int PaperCount { get; set; }
        }
    }
}