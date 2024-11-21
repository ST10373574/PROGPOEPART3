using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Prog2bPOEPart2.Models;
using System.Collections.Generic;

namespace Prog2bPOEPart2.Services
{
    public class PdfReportService
    {
        public byte[] GeneratePdfReport(List<Claim> claims)
        {
            using var memoryStream = new MemoryStream();
            var document = new iTextSharp.text.Document();
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Title Section
            var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
            var title = new Paragraph("Claims Report", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // Adding generated date
            var dateParagraph = new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", FontFactory.GetFont("Arial", 12));
            dateParagraph.Alignment = Element.ALIGN_RIGHT;
            document.Add(dateParagraph);

            // Adding space between sections
            document.Add(new Paragraph("\n"));

            // Table for Claims Data
            var table = new PdfPTable(6); // 6 columns now, adding one for the Claim Name
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 2f, 3f }); // Adjusting column width

            // Adding Table Header
            table.AddCell("Claim ID");
            table.AddCell("Claim Date");
            table.AddCell("Hours Worked");
            table.AddCell("Hourly Rate");
            table.AddCell("Total Amount");
            table.AddCell("Claim Name"); // New column for Claim Name

            // Adding Data Rows
            foreach (var claim in claims)
            {
                table.AddCell(claim.ClaimId.ToString());
                table.AddCell(claim.DateSubmitted.ToString("yyyy-MM-dd"));
                table.AddCell(claim.HoursWorked.ToString("N2")); // Formatting hours worked to two decimal places
                table.AddCell($"R{claim.HourlyRate:N2}"); // Formatting the hourly rate with currency format
                table.AddCell($"R{claim.TotalEarning:N2}"); // Formatting the total amount with currency format
                table.AddCell(claim.Name); // Adding the Claim Name (change if the field is different)
            }

            document.Add(table);

            document.Close();
            return memoryStream.ToArray();
        }
        private void AddReportTitle(iTextSharp.text.Document document) // Fully qualified name
        {
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Claim Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(title);
        }

        private void AddMetadata(iTextSharp.text.Document document) // Fully qualified name
        {
            var metadataFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", metadataFont));
            document.Add(new Paragraph(" ", metadataFont)); // Add spacing
        }

        private PdfPTable CreateClaimsTable(List<ClaimViewModel> claims)
        {
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 15, 25, 20, 20, 20 }); // Column widths

            // Add table headers
            AddCellToTable(table, "Claim ID", true);
            AddCellToTable(table, "Claim Date", true);
            AddCellToTable(table, "Hours Worked", true);
            AddCellToTable(table, "Hourly Rate", true);
            AddCellToTable(table, "Total Amount", true);

            // Add data rows
            foreach (var claim in claims)
            {
                AddCellToTable(table, claim.ClaimID.ToString());
                AddCellToTable(table, claim.ClaimDate.ToString("yyyy-MM-dd"));
                AddCellToTable(table, claim.HoursWorked.ToString("F2"));
                AddCellToTable(table, claim.HourlyRate.ToString("C"));
                AddCellToTable(table, claim.TotalAmount.ToString("C"));
            }

            return table;
        }

        private void AddCellToTable(PdfPTable table, string content, bool isHeader = false)
        {
            var font = isHeader
                ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)
                : FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var cell = new PdfPCell(new Phrase(content, font))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                BackgroundColor = isHeader ? BaseColor.LIGHT_GRAY : BaseColor.WHITE
            };

            table.AddCell(cell);
        }
    }
}
