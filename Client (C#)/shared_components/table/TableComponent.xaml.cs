using app.structure.models;
using app.structure.models.general;
using app.structure.services;
using app.structure.services.translation;
using app.structure.utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace app.shared_components
{
    public partial class TableComponent : UserControl
    {
        private TranslationService translation;
        private ComponentService componentService;

        private int columnsCount = 0;

        public TableComponent()
        {
            InitializeComponent();

            if (ComponentService.isRuntimeMode)
            {
                translation = Services.getService<TranslationService>();
                componentService = Services.getService<ComponentService>();
                TranslationService.changed += onLanguageChange;
                Unloaded += onUnloaded;
            }
        }

        private void onLanguageChange(Languages lang)
        {
            componentService.updateScrollViewer(scrollArea, lang);

            bool isArabic = lang == Languages.AR;

            int columnIndex = isArabic ? columnsCount - 1 : 0;

            foreach (Grid cell in table.Children)
            {
                TableCell c = cell.Tag as TableCell;

                if (c != null)
                {
                    TextBlock text = cell.Children[cell.Children.Count - 1] as TextBlock;
                    text.Text = c.getTranslatedValue(translation);
                    text.HorizontalAlignment = isArabic ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                }

                Grid.SetColumn(cell, isArabic ? columnIndex-- : columnIndex++);

                if (columnIndex < 0 || columnIndex == columnsCount)
                {
                    columnIndex = isArabic ? columnsCount - 1 : 0;
                }
            }
        }

        private void onUnloaded(object sender, RoutedEventArgs e)
        {
            TranslationService.changed -= onLanguageChange;
        }

        private void renderRow(TableRow row, int index, bool showBottomLine, bool isHeader = false)
        {
            int columnIndex = 0;

            columnsCount = Math.Max(columnsCount, row.cells.Count);

            row.cells.ForEach(cell =>
            {
                TextBlock text = new TextBlock();
                text.FontSize = 16;
                text.FontFamily = AppFonts.HpSimplified;
                text.FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal;
                text.Foreground = new SolidColorBrush(AppColors.DarkerGreyColor);

                if (isHeader)
                {
                    for(int columns = 0; columns < cell.colSpan; columns++)
                    {
                        ColumnDefinition colDef = new ColumnDefinition();
                        table.ColumnDefinitions.Add(colDef);
                    }
                }

                text.Text = cell.getTranslatedValue(translation);
                text.Margin = new Thickness(columnIndex == 0 ? 0 : 10, !isHeader ? 10 + (index == 1 ? 10 : 0) : 0, 0, 10);
                text.TextWrapping = TextWrapping.WrapWithOverflow;
                text.TextTrimming = TextTrimming.CharacterEllipsis;
                text.HorizontalAlignment = TranslationService.language == Languages.AR ? HorizontalAlignment.Right : HorizontalAlignment.Left;

                Grid cellContent = new Grid();
                cellContent.Tag = cell;
                cellContent.RowDefinitions.Add(new RowDefinition());

                Grid.SetRow(text, 0);

                if (showBottomLine)
                {
                    RowDefinition lineRow = new RowDefinition();
                    lineRow.Height = GridLength.Auto;
                    cellContent.RowDefinitions.Add(lineRow);

                    Rectangle line = new Rectangle();
                    line.Height = 1;
                    line.Fill = new SolidColorBrush(AppColors.LightGreyColor);

                    Grid.SetRow(line, 1);

                    cellContent.Children.Add(line);
                }

                cellContent.Children.Add(text);

                Grid.SetRow(cellContent, index);
                Grid.SetColumn(cellContent, columnIndex++);
                Grid.SetColumnSpan(cellContent, cell.colSpan);

                table.Children.Add(cellContent);
            });

            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = GridLength.Auto;

            table.RowDefinitions.Add(rowDef);
        }

        public void init(TableRow header, List<TableRow> rows)
        {
            renderRow(header, 0, true, true);

            columnsCount = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                renderRow(rows[i], i + 1, i < rows.Count - 1 && rows[i].joinToNext);
            }
        }
    }

    public class TableCell
    {
        public object value;
        public int colSpan;
        private string gender;
        public bool isNumberTranslation;
        public bool isGenderTranslation { get { return !string.IsNullOrEmpty(gender); } }

        public TableCell(object value, int colSpan = 1) {
            this.value = value;
            this.colSpan = colSpan;
        }

        public TableCell translateByGender(string gender)
        {
            this.gender = gender;
            isNumberTranslation = false;
            return this;
        }

        public TableCell translateAsNumber()
        {
            gender = "";
            isNumberTranslation = true;
            return this;
        }

        public string getTranslatedValue(TranslationService translation)
        {
            if (value is ItemTranslation)
            {
                ItemTranslation item = value as ItemTranslation;
                return TranslationService.language == Languages.AR ? item.AR : item.EN;
            }

            if (value is DateTime)
            {
                return DateUtils.getDateString((DateTime)value, translation);
            }

            if (isNumberTranslation)
            {
                return translation.translateNumeric((long)value);
            }

            string key = value.ToString();

            return isGenderTranslation ? translation.translateByGender(key, gender) : translation.translate(key);
        }
    }

    public class TableRow
    {
        public List<TableCell> cells;
        public bool joinToNext;

        public TableRow(List<TableCell> cells, bool joinToNext = false) {
            this.cells = cells;
            this.joinToNext = joinToNext;
        }

        public TableRow(bool joinToNext = false)
        {
            cells = new List<TableCell>();
            this.joinToNext = joinToNext;
        }
    }
}
