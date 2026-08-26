using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Simando.Infrastructure.Documents;

internal static class DocxBuilderHelper
{
    public const string DefaultFont = "Calibri";
    public const string StandardFontSize = "22"; // 11pt in half-points
    public const string TitleFontSize = "28";    // 14pt
    public const string HeaderFontSize = "24";   // 12pt
    public const string SmallFontSize = "18";    // 9pt

    public static Body CreateDocumentBody(WordprocessingDocument doc)
    {
        var mainPart = doc.AddMainDocumentPart();
        var body = new Body();
        mainPart.Document = new Document(body);

        var sectionProps = new SectionProperties(
            new PageMargin
            {
                Top = 1134,    // 2 cm
                Bottom = 1134,
                Left = 1417,   // 2.5 cm
                Right = 1417
            }
        );
        body.AppendChild(sectionProps);

        return body;
    }

    public static void AppendPgnControlHeaderBox(Body body, string docNo = "O-001/06.02", string revision = "01", string? effectiveDate = null, string pageNo = "160 dari 211")
    {
        var table = new Table();
        var tblProps = new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 8, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 8, Color = "000000" },
                new LeftBorder { Val = BorderValues.Single, Size = 8, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 8, Color = "000000" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 6, Color = "000000" }
            )
        );
        table.AppendChild(tblProps);

        // Row 1: Document Control Header Banner
        var row1 = new TableRow();
        var cellTitle = new TableCell(
            new TableCellProperties(new TableCellWidth { Width = "5000", Type = TableWidthUnitValues.Pct }),
            new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center },
                    new SpacingBetweenLines { Before = "80", After = "40" }
                ),
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = SmallFontSize }, new RunFonts { Ascii = DefaultFont }),
                    new Text("PELANGGAN KOMERSIAL DAN INDUSTRI SELAIN PELANGGAN KORPORAT")
                ),
                new Break(),
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = StandardFontSize }, new RunFonts { Ascii = DefaultFont }),
                    new Text("PT PERUSAHAAN GAS NEGARA Tbk.")
                )
            )
        );
        row1.AppendChild(cellTitle);
        table.AppendChild(row1);

        // Row 2: Document Control Information (4 Columns)
        var row2 = new TableRow();
        var dateText = effectiveDate ?? "1 April 2023";

        row2.AppendChild(CreateBoxCell($"No. Dok.: {docNo}"));
        row2.AppendChild(CreateBoxCell($"Revisi Ke: {revision}"));
        row2.AppendChild(CreateBoxCell($"Tgl. Berlaku: {dateText}"));
        row2.AppendChild(CreateBoxCell($"Hal.: {pageNo}"));

        table.AppendChild(row2);
        body.AppendChild(table);
        body.AppendChild(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "180" })));
    }

    private static TableCell CreateBoxCell(string text)
    {
        return new TableCell(
            new TableCellProperties(
                new Shading { Fill = "F8F9FA" },
                new TableCellMargin
                {
                    TopMargin = new TopMargin { Width = "80" },
                    BottomMargin = new BottomMargin { Width = "80" },
                    LeftMargin = new LeftMargin { Width = "120" },
                    RightMargin = new RightMargin { Width = "120" }
                }
            ),
            new Paragraph(
                new ParagraphProperties(new SpacingBetweenLines { After = "0" }),
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = SmallFontSize }, new RunFonts { Ascii = DefaultFont }),
                    new Text(text)
                )
            )
        );
    }

    public static void AppendTitle(Body body, string title, string? docNumber = null, string? areaName = null, string? lampiranLabel = null)
    {
        if (!string.IsNullOrWhiteSpace(lampiranLabel))
        {
            var pLamp = body.AppendChild(new Paragraph());
            pLamp.ParagraphProperties = new ParagraphProperties(
                new SpacingBetweenLines { After = "120" }
            );
            var runLamp = pLamp.AppendChild(new Run());
            runLamp.RunProperties = new RunProperties(
                new Bold(),
                new FontSize { Val = StandardFontSize },
                new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
            );
            runLamp.AppendChild(new Text(lampiranLabel));
        }

        var p = body.AppendChild(new Paragraph());
        p.ParagraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "120", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        );

        var runHeader = p.AppendChild(new Run());
        runHeader.RunProperties = new RunProperties(
            new Bold(),
            new FontSize { Val = TitleFontSize },
            new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
        );
        runHeader.AppendChild(new Text("PT PERUSAHAAN GAS NEGARA Tbk"));

        p.AppendChild(new Break());

        var runTitle = p.AppendChild(new Run());
        runTitle.RunProperties = new RunProperties(
            new Bold(),
            new FontSize { Val = HeaderFontSize },
            new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
        );
        runTitle.AppendChild(new Text(title));

        if (!string.IsNullOrWhiteSpace(areaName))
        {
            p.AppendChild(new Break());
            var runArea = p.AppendChild(new Run());
            runArea.RunProperties = new RunProperties(
                new Bold(),
                new FontSize { Val = StandardFontSize },
                new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
            );
            runArea.AppendChild(new Text($"AREA : {areaName.ToUpperInvariant()}"));
        }

        if (!string.IsNullOrWhiteSpace(docNumber))
        {
            p.AppendChild(new Break());
            var runDoc = p.AppendChild(new Run());
            runDoc.RunProperties = new RunProperties(
                new Italic(),
                new FontSize { Val = StandardFontSize },
                new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
            );
            runDoc.AppendChild(new Text(docNumber));
        }

        body.AppendChild(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "200" })));
    }

    public static void AppendSectionHeader(Body body, string sectionTitle)
    {
        var p = body.AppendChild(new Paragraph());
        p.ParagraphProperties = new ParagraphProperties(
            new SpacingBetweenLines { Before = "240", After = "120" }
        );

        var run = p.AppendChild(new Run());
        run.RunProperties = new RunProperties(
            new Bold(),
            new FontSize { Val = StandardFontSize },
            new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont },
            new Color { Val = "1F4E79" }
        );
        run.AppendChild(new Text(sectionTitle));
    }

    public static void AppendKeyValueParagraph(Body body, string label, string? value, bool indent = false)
    {
        var p = body.AppendChild(new Paragraph());
        p.ParagraphProperties = new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        );
        if (indent)
        {
            p.ParagraphProperties.Indentation = new Indentation { Left = "280" };
        }

        var runLabel = p.AppendChild(new Run());
        runLabel.RunProperties = new RunProperties(
            new Bold(),
            new FontSize { Val = StandardFontSize },
            new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
        );
        runLabel.AppendChild(new Text($"{label} : ") { Space = SpaceProcessingModeValues.Preserve });

        var runVal = p.AppendChild(new Run());
        runVal.RunProperties = new RunProperties(
            new FontSize { Val = StandardFontSize },
            new RunFonts { Ascii = DefaultFont, HighAnsi = DefaultFont }
        );
        runVal.AppendChild(new Text(value ?? ".................................................."));
    }

    public static void AppendKeyValueTable(Body body, IEnumerable<(string Label, string? Value)> items)
    {
        var table = new Table();
        var tblProps = new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.None },
                new BottomBorder { Val = BorderValues.None },
                new LeftBorder { Val = BorderValues.None },
                new RightBorder { Val = BorderValues.None },
                new InsideHorizontalBorder { Val = BorderValues.None },
                new InsideVerticalBorder { Val = BorderValues.None }
            )
        );
        table.AppendChild(tblProps);

        foreach (var (label, value) in items)
        {
            var row = new TableRow();

            // Label Cell (~38% width)
            var cellLabel = new TableCell(
                new TableCellProperties(new TableCellWidth { Width = "1900", Type = TableWidthUnitValues.Pct }),
                new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { After = "40", Line = "240" }),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = StandardFontSize }, new RunFonts { Ascii = DefaultFont }),
                        new Text(label)
                    )
                )
            );

            // Colon Cell (~4% width)
            var cellColon = new TableCell(
                new TableCellProperties(new TableCellWidth { Width = "200", Type = TableWidthUnitValues.Pct }),
                new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { After = "40", Line = "240" }),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = StandardFontSize }, new RunFonts { Ascii = DefaultFont }),
                        new Text(":")
                    )
                )
            );

            // Value Cell (~58% width)
            var displayVal = !string.IsNullOrWhiteSpace(value) && value != "-" ? value : "..................................................";
            var cellValue = new TableCell(
                new TableCellProperties(new TableCellWidth { Width = "2900", Type = TableWidthUnitValues.Pct }),
                new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { After = "40", Line = "240" }),
                    new Run(
                        new RunProperties(new FontSize { Val = StandardFontSize }, new RunFonts { Ascii = DefaultFont }),
                        new Text(displayVal)
                    )
                )
            );

            row.AppendChild(cellLabel);
            row.AppendChild(cellColon);
            row.AppendChild(cellValue);
            table.AppendChild(row);
        }

        body.AppendChild(table);
        body.AppendChild(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "120" })));
    }

    public static Table CreateTable(string[] headers, List<string[]> rows)
    {
        var table = new Table();

        var tblProps = new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Color = "000000" }
            )
        );
        table.AppendChild(tblProps);

        // Header Row
        var headerRow = new TableRow();
        headerRow.AppendChild(new TableRowProperties(new TableHeader()));
        foreach (var header in headers)
        {
            var cell = new TableCell(
                new TableCellProperties(
                    new Shading { Fill = "F8F9FA" },
                    new TableCellMargin
                    {
                        TopMargin = new TopMargin { Width = "120" },
                        BottomMargin = new BottomMargin { Width = "120" },
                        LeftMargin = new LeftMargin { Width = "150" },
                        RightMargin = new RightMargin { Width = "150" }
                    }
                ),
                new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Center },
                        new SpacingBetweenLines { After = "0" }
                    ),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = SmallFontSize }, new RunFonts { Ascii = DefaultFont }),
                        new Text(header)
                    )
                )
            );
            headerRow.AppendChild(cell);
        }
        table.AppendChild(headerRow);

        // Data Rows
        foreach (var rowData in rows)
        {
            var tr = new TableRow();
            foreach (var cellText in rowData)
            {
                var cell = new TableCell(
                    new TableCellProperties(
                        new TableCellMargin
                        {
                            TopMargin = new TopMargin { Width = "100" },
                            BottomMargin = new BottomMargin { Width = "100" },
                            LeftMargin = new LeftMargin { Width = "150" },
                            RightMargin = new RightMargin { Width = "150" }
                        }
                    ),
                    new Paragraph(
                        new ParagraphProperties(new SpacingBetweenLines { After = "0" }),
                        new Run(
                            new RunProperties(new FontSize { Val = SmallFontSize }, new RunFonts { Ascii = DefaultFont }),
                            new Text(cellText)
                        )
                    )
                );
                tr.AppendChild(cell);
            }
            table.AppendChild(tr);
        }

        return table;
    }

    public static void AppendSignatureBlock(Body body, string leftTitle, string leftSubtitle, string rightTitle, string rightSubtitle)
    {
        body.AppendChild(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { Before = "360", After = "120" })));

        var table = new Table();
        table.AppendChild(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.None },
                new BottomBorder { Val = BorderValues.None },
                new LeftBorder { Val = BorderValues.None },
                new RightBorder { Val = BorderValues.None },
                new InsideHorizontalBorder { Val = BorderValues.None },
                new InsideVerticalBorder { Val = BorderValues.None }
            )
        ));

        // Header signature title row
        var titleRow = new TableRow();
        titleRow.AppendChild(CreateSignatureCell(leftTitle, true));
        titleRow.AppendChild(CreateSignatureCell(rightTitle, true));
        table.AppendChild(titleRow);

        // Spacing row for physical/digital signature
        var spaceRow = new TableRow();
        spaceRow.AppendChild(CreateSignatureCell("\n\n\n\n", false));
        spaceRow.AppendChild(CreateSignatureCell("\n\n\n\n", false));
        table.AppendChild(spaceRow);

        // Subtitle row (Name & Position)
        var nameRow = new TableRow();
        nameRow.AppendChild(CreateSignatureCell($"( {leftSubtitle} )", false));
        nameRow.AppendChild(CreateSignatureCell($"( {rightSubtitle} )", false));
        table.AppendChild(nameRow);

        body.AppendChild(table);
    }

    private static TableCell CreateSignatureCell(string text, bool isBold)
    {
        var cell = new TableCell();
        var p = new Paragraph(new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "60" }
        ));

        var runProps = new RunProperties(new FontSize { Val = StandardFontSize }, new RunFonts { Ascii = DefaultFont });
        if (isBold) runProps.AppendChild(new Bold());

        var run = new Run(runProps);
        run.AppendChild(new Text(text));
        p.AppendChild(run);
        cell.AppendChild(p);
        return cell;
    }
}