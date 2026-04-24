using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PlanDiagram.Services
{
    public class GanttChart
    {
        #region [Переменные класса]
        private DateTime _minDate;
        private DateTime _maxDate;
        private List<ProcessData> _tasks;

        private Canvas _ganttCanvas;
        private Canvas _dateHeaderCanvas;
        private double _pixelsPerDay = 45.0;
        #endregion

        public GanttChart(Canvas ganttCanvas, Canvas dateHeaderCanvas)
        {
            _ganttCanvas = ganttCanvas;
            _dateHeaderCanvas = dateHeaderCanvas;
        }

        public void Build(DateTime minDate, DateTime maxDate, List<ProcessData> task)
        {
            _minDate = minDate;
            _maxDate = maxDate;
            _tasks = task;
            DrawChart();
            DrawDateHeader();
        }

        private void DrawChart()
        {
            if (_tasks == null || _tasks.Count == 0) return;

            _ganttCanvas.Children.Clear();

            double totalWidth = GetTotalWidth();
            double rowHeight = 40;
            double currentY = 0;

            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = _tasks.Count * rowHeight;

            // Отрисовка задач
            for (int i = 0; i < _tasks.Count; i++)
            {
                var task = _tasks[i];
                double y = currentY + i * rowHeight;

                // Горизонтальные линии сетки
                var gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y + rowHeight,
                    X2 = totalWidth,
                    Y2 = y + rowHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                _ganttCanvas.Children.Add(gridLine);

                // Прямоугольник задачи
                double x = (task.PlanStartDate - _minDate).TotalDays * _pixelsPerDay;
                double width = (task.PlanEndDate - task.PlanStartDate).TotalDays * _pixelsPerDay + _pixelsPerDay;
                if (width < 4) width = 4;

                var rect = new Rectangle
                {
                    Width = width,
                    Height = rowHeight - 4,
                    Fill = new SolidColorBrush(Colors.DodgerBlue),
                    RadiusX = 3,
                    RadiusY = 3,
                    Tag = task
                };

                // Создаем ToolTip с информацией о задаче
                string tooltipText = $"{task.ProcessName}\n" +
                                    $"Начало: {task.PlanStartDate:dd.MM.yyyy}\n" +
                                    $"Окончание: {task.PlanEndDate:dd.MM.yyyy}\n" +
                                    $"Длительность: {task.WorkTime:F1} ч";

                if (!string.IsNullOrEmpty(task.TooltipText) && task.TooltipText != task.ProcessName)
                {
                    tooltipText += $"\n{task.TooltipText}";
                }
                rect.ToolTip = tooltipText;


                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + 2);
                _ganttCanvas.Children.Add(rect);

                // Текст внутри прямоугольника (если достаточно места)
                if (width > 40)
                {
                    var text = new TextBlock
                    {
                        Text = $"{task.WorkTime:F1} ч",
                        FontSize = 11,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Canvas.SetLeft(text, x + 4);
                    Canvas.SetTop(text, y + 12);
                    _ganttCanvas.Children.Add(text);
                }
            }
        }

        private void DrawDateHeader()
        {
            if (_tasks == null || _tasks.Count == 0) return;

            _dateHeaderCanvas.Children.Clear();

            int totalDays = (int)(_maxDate - _minDate).TotalDays + 1;
            var grayColor = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            _dateHeaderCanvas.Width = GetTotalWidth();

            for (int i = 0; i < totalDays; i++)
            {
                DateTime currentDate = _minDate.AddDays(i);
                double x = i * _pixelsPerDay;

                // Подсветка выходных дней
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    var weekendBg = new Rectangle
                    {
                        Width = _pixelsPerDay,
                        Height = 50,
                        Fill = grayColor
                    };
                    Canvas.SetLeft(weekendBg, x);
                    Canvas.SetTop(weekendBg, 0);
                    _dateHeaderCanvas.Children.Add(weekendBg);
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
                _dateHeaderCanvas.Children.Add(dateText);

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
                _dateHeaderCanvas.Children.Add(dayOfWeekText);

                // Вертикальная линия
                var line = new Line
                {
                    X1 = x + _pixelsPerDay,
                    Y1 = 0,
                    X2 = x + _pixelsPerDay,
                    Y2 = 50,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                _dateHeaderCanvas.Children.Add(line);
            }

            // Левая граница
            var leftBorder = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 50,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            _dateHeaderCanvas.Children.Add(leftBorder);
        }

        private double GetTotalWidth()
        {
            int totalDays = (int)(_maxDate - _minDate).TotalDays + 1;
            return totalDays * _pixelsPerDay;
        }
    }
}