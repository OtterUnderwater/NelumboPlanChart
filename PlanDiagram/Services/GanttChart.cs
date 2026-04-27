using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PlanDiagram.Services
{
    public class GanttChart
    {
        #region [Переменные класса]
        private List<DateTime> _allWorkDays;
        private List<DateTime> _filteredWorkDays;
        private List<ProcessData> _process;
        private double _pixelsPerDay = 45.0;
        #endregion

        public GanttChart(List<DateTime> allWorkDays)
        {
            _allWorkDays = allWorkDays;
        }

        public List<GanttRowModel> Build(DateTime minDate, DateTime maxDate, List<ProcessData> process)
        {
            _process = process;

            // Фильтруем рабочие дни по диапазону дат
            _filteredWorkDays = _allWorkDays
                .Where(d => d >= minDate && d <= maxDate)
                .OrderBy(d => d)
                .ToList();

            return CreateGanttRows();
        }

        private List<GanttRowModel> CreateGanttRows()
        {
            var rows = new List<GanttRowModel>();

            if (_process == null || _process.Count == 0 || _filteredWorkDays == null || _filteredWorkDays.Count == 0)
                return rows;

            foreach (var task in _process)
            {
                // Находим индексы рабочих дней для дат задачи
                int startIndex = GetWorkingDayIndex(task.PlanStartDate);
                int endIndex = GetWorkingDayIndex(task.PlanEndDate);

                // Если даты не найдены в рабочих днях, пропускаем задачу
                if (startIndex == -1 || endIndex == -1)
                    continue;

                // Вычисляем позицию и ширину на основе индексов рабочих дней
                double x = startIndex * _pixelsPerDay;
                double width = (endIndex - startIndex + 1) * _pixelsPerDay;
                if (width < 4) width = 4;

                var displayText = width > 40 ? $"{task.WorkTime:F1} ч" : "";

                var ganttItem = new GanttItemModel
                {
                    Width = width,
                    Left = x,
                    DisplayText = displayText,
                    Color = new SolidColorBrush(Colors.DodgerBlue),
                    HoverColor = new SolidColorBrush(Colors.DeepSkyBlue),
                    TaskData = task
                };

                var row = new GanttRowModel
                {
                    ProcessName = task.ProcessName,
                    TaskData = task,
                    GanttItem = ganttItem
                };

                rows.Add(row);
            }
            return rows;
        }

        /// <summary>
        /// Получение индекса рабочего дня в отфильтрованном списке
        /// </summary>
        private int GetWorkingDayIndex(DateTime date)
        {
            for (int i = 0; i < _filteredWorkDays.Count; i++)
            {
                if (_filteredWorkDays[i].Date == date.Date)
                    return i;
            }
            return -1;
        }

        public double GetTotalWidth()
        {
            return _filteredWorkDays != null ? _filteredWorkDays.Count * _pixelsPerDay : 0;
        }

        public void DrawDateHeader(Canvas dateHeaderCanvas)
        {
            if (_filteredWorkDays == null || _filteredWorkDays.Count == 0) return;

            dateHeaderCanvas.Children.Clear();

            double totalWidth = GetTotalWidth();
            dateHeaderCanvas.Width = totalWidth;

            for (int i = 0; i < _filteredWorkDays.Count; i++)
            {
                DateTime currentDate = _filteredWorkDays[i];
                double x = i * _pixelsPerDay;

                // Дата
                var dateText = new TextBlock
                {
                    Text = currentDate.ToString("dd.MM"),
                    FontSize = 11,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(dateText, x + (_pixelsPerDay - 35) / 2);
                Canvas.SetTop(dateText, 5);
                dateHeaderCanvas.Children.Add(dateText);

                // День недели
                var dayOfWeekText = new TextBlock
                {
                    Text = currentDate.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(dayOfWeekText, x + (_pixelsPerDay - 30) / 2);
                Canvas.SetTop(dayOfWeekText, 28);
                dateHeaderCanvas.Children.Add(dayOfWeekText);

                // Вертикальная линия
                var line = new System.Windows.Shapes.Line
                {
                    X1 = x + _pixelsPerDay,
                    Y1 = 0,
                    X2 = x + _pixelsPerDay,
                    Y2 = 50,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                dateHeaderCanvas.Children.Add(line);
            }

            // Левая граница
            var leftBorder = new System.Windows.Shapes.Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 50,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            dateHeaderCanvas.Children.Add(leftBorder);
        }
    }
}