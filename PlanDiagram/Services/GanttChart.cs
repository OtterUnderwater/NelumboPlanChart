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
        private DateTime _minDate;
        private DateTime _maxDate;
        private List<ProcessData> _tasks;
        private double _pixelsPerDay = 45.0;
        #endregion

        public GanttChart()
        {
        }

        public List<GanttRowModel> Build(DateTime minDate, DateTime maxDate, List<ProcessData> tasks)
        {
            _minDate = minDate;
            _maxDate = maxDate;
            _tasks = tasks;

            return CreateGanttRows();
        }

        private List<GanttRowModel> CreateGanttRows()
        {
            var rows = new List<GanttRowModel>();

            if (_tasks == null || _tasks.Count == 0)
                return rows;

            foreach (var task in _tasks)
            {
                // Вычисляем позицию и ширину для задачи
                double x = (task.PlanStartDate - _minDate).TotalDays * _pixelsPerDay;
                double width = (task.PlanEndDate - task.PlanStartDate).TotalDays * _pixelsPerDay + _pixelsPerDay;
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

        public double GetTotalWidth()
        {
            int totalDays = (int)(_maxDate - _minDate).TotalDays + 1;
            return totalDays * _pixelsPerDay;
        }

        public void DrawDateHeader(Canvas dateHeaderCanvas, DateTime minDate, DateTime maxDate, List<ProcessData> tasks)
        {
            if (tasks == null || tasks.Count == 0) return;

            dateHeaderCanvas.Children.Clear();

            int totalDays = (int)(maxDate - minDate).TotalDays + 1;
            var grayColor = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            double totalWidth = GetTotalWidth();

            dateHeaderCanvas.Width = totalWidth;

            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDate = minDate.AddDays(i);
                double x = i * _pixelsPerDay;

                // Подсветка выходных дней
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    var weekendBg = new System.Windows.Shapes.Rectangle
                    {
                        Width = _pixelsPerDay,
                        Height = 50,
                        Fill = grayColor
                    };
                    Canvas.SetLeft(weekendBg, x);
                    Canvas.SetTop(weekendBg, 0);
                    dateHeaderCanvas.Children.Add(weekendBg);
                }

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
                    Foreground = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday ?
                                Brushes.Red : Brushes.Gray
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