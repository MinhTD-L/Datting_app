using DataTransferObject;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using Presentation.FormPost;

namespace Presentation.Reports
{
    public class StatsQuestPdfReport : IDocument
    {
        private readonly FullStatsReportDTO _data;

        public StatsQuestPdfReport(FullStatsReportDTO data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(30);
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(text => text.Span("Báo cáo Thống kê LoveConnect").Style(TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium)));
                    column.Item().Text(text => text.Span($"Ngày xuất: {_data.ReportDate}").Style(TextStyle.Default.FontSize(10)));
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);

                // Phần 1: Tổng quan
                column.Item().Element(ComposeOverview);

                // Phần 2: Tags phổ biến
                column.Item().Element(ComposePopularTags);

                // Phần 3: Dữ liệu theo thời gian
                column.Item().Element(ComposeTimeSeries);
            });
        }

        void ComposeOverview(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text(text => text.Span("1. Tổng quan").Style(TextStyle.Default.FontSize(16).Bold()));
                column.Item().PaddingTop(5).Grid(grid =>
                {
                    grid.VerticalSpacing(5);
                    grid.HorizontalSpacing(5);
                    grid.Columns(4); // 2 cột key, 2 cột value

                    // Định nghĩa style
                    var keyStyle = TextStyle.Default.SemiBold();
                    var valueStyle = TextStyle.Default;

                    foreach (var item in _data.OverviewStats)
                    {
                        grid.Item(2).AlignRight().Text(text => text.Span(item.Key + ":").Style(keyStyle));
                        grid.Item(2).Text(text => text.Span(item.Value?.ToString() ?? "").Style(valueStyle));
                    }
                });
            });
        }

        void ComposePopularTags(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text(text => text.Span("2. Tags phổ biến").Style(TextStyle.Default.FontSize(16).Bold()));
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tag");
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tổng bài đăng");
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tổng lượt thích");
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("TB Thích/Bài");
                    });

                    foreach (var tag in _data.PopularTags)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tag.Tag);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tag.TotalPosts.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tag.TotalLikes.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(tag.AvgLikes.ToString("N2"));
                    }
                });
            });
        }

        void ComposeTimeSeries(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text(text => text.Span("3. Thống kê theo thời gian").Style(TextStyle.Default.FontSize(16).Bold()));
                column.Spacing(15);

                foreach (var metric in _data.TimeSeries)
                {
                    column.Item().ShowEntire().Column(col =>
                    {
                        col.Item().Text(text => text.Span(metric.Key).Style(TextStyle.Default.FontSize(12).SemiBold()));
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Ngày");
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Số lượng");
                            });

                            var orderedData = metric.Value
                                .Where(p => System.DateTime.TryParse(p.Date, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out _))
                                .OrderBy(p => System.DateTime.Parse(p.Date));

                            foreach (var point in orderedData)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(System.DateTime.Parse(point.Date).ToString("dd/MM/yyyy"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(point.Count.ToString());
                            }
                        });
                    });
                }
            });
        }
    }
}