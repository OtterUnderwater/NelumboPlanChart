using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PlanDiagram.Services
{
    public class GanttChartByWorkCenter : IGanttChart
    {
        private List<DateTime> _allWorkDays;
        private List<DateTime> _filteredWorkDays;
        private List<ProcessData> _processes;
        private double _pixelsPerDay = 45.0;
        private Canvas _ganttCanvas;
        private DateTime _minDate;
        private DateTime _maxDate;
        private double _rowHeight = 60; // высота строки рабочего места (должна совпадать с высотой в левой колонке)

        public GanttChartByWorkCenter(List<DateTime> allWorkDays)
        {
            _allWorkDays = allWorkDays;
        }

        public void Build(DateTime minDate, DateTime maxDate, List<ProcessData> processes, Canvas ganttCanvas)
        {
            _processes = processes;
            _ganttCanvas = ganttCanvas;
            _minDate = minDate;
            _maxDate = maxDate;

            _filteredWorkDays = _allWorkDays
                .Where(d => d >= minDate && d <= maxDate)
                .OrderBy(d => d)
                .ToList();

            DrawGanttChart();
        }

        private void DrawGanttChart()
        {
            if (_ganttCanvas == null || _processes == null || _processes.Count == 0 ||
                _filteredWorkDays == null || _filteredWorkDays.Count == 0)
                return;

            _ganttCanvas.Children.Clear();

            double totalWidth = _filteredWorkDays.Count * _pixelsPerDay;

            // Группировка процессов по рабочим местам
            var workCenters = _processes
                .Select(p => p.WorkCenterName)
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            // Для каждого рабочего места вычисляем максимальное количество активных процессов в день
            var maxProcessesPerWorkCenter = new Dictionary<string, int>();
            foreach (var wc in workCenters)
            {
                var processesForWc = _processes.Where(p => p.WorkCenterName == wc).ToList();
                int maxCount = 0;
                foreach (var day in _filteredWorkDays)
                {
                    int count = processesForWc.Count(p =>
                        p.PlanStartDate.Date <= day.Date && p.PlanEndDate.Date >= day.Date);
                    if (count > maxCount) maxCount = count;
                }
                maxProcessesPerWorkCenter[wc] = Math.Max(maxCount, 1); // минимум 1
            }

            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = workCenters.Count * _rowHeight;

            double currentY = 0;

            for (int wcIndex = 0; wcIndex < workCenters.Count; wcIndex++)
            {
                string wcName = workCenters[wcIndex];
                var processesForWc = _processes.Where(p => p.WorkCenterName == wcName).ToList();
                int maxProcesses = maxProcessesPerWorkCenter[wcName];
                double blockHeight = _rowHeight / maxProcesses;

                // Фон строки (светло-серый)
                var rowBackground = new Rectangle
                {
                    Width = totalWidth,
                    Height = _rowHeight,
                    Fill = Brushes.White,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(rowBackground, 0);
                Canvas.SetTop(rowBackground, currentY);
                _ganttCanvas.Children.Add(rowBackground);

                // Для каждого дня
                for (int dayIndex = 0; dayIndex < _filteredWorkDays.Count; dayIndex++)
                {
                    DateTime currentDay = _filteredWorkDays[dayIndex];
                    double x = dayIndex * _pixelsPerDay;

                    // Активные процессы в этот день (упорядочим, чтобы было предсказуемо)
                    var activeProcesses = processesForWc
                        .Where(p => p.PlanStartDate.Date <= currentDay.Date &&
                                    p.PlanEndDate.Date >= currentDay.Date)
                        .OrderBy(p => p.PlanStartDate) // можно изменить порядок на более логичный
                        .ToList();

                    if (activeProcesses.Count == 0) continue;

                    // Рисуем каждый процесс в ячейке дня
                    for (int i = 0; i < activeProcesses.Count; i++)
                    {
                        var task = activeProcesses[i];
                        double yOffset = currentY + i * blockHeight; // последовательно сверху вниз

                        var rect = new Rectangle
                        {
                            Width = _pixelsPerDay - 2,
                            Height = blockHeight - 2,
                            Fill = GetBrushFromHex(task.HexCode),
                            RadiusX = 2,
                            RadiusY = 2,
                            Tag = task,
                            Cursor = Cursors.Hand,
                            ToolTip = $"Раб.место: {task.WorkCenterName}\n" +
                                     $"Дата: {currentDay:dd.MM.yyyy}\n" +
                                     $"Операция: {task.OpName}\n" +
                                     $"Процесс: {task.ProcessName}\n" +
                                     $"Кол-во: {task.Qty}\n" +
                                     $"Длит-ть: {task.WorkTime:F1} ч"
                        };

                        rect.MouseLeftButtonDown += (s, e) =>
                        {
                            if ((s as Rectangle)?.Tag is ProcessData clickedTask)
                                ShowTaskDetails(clickedTask);
                        };

                        Canvas.SetLeft(rect, x + 1);
                        Canvas.SetTop(rect, yOffset + 1);
                        _ganttCanvas.Children.Add(rect);
                    }
                }

                // Разделительная линия снизу строки
                var separator = new Line
                {
                    X1 = 0,
                    Y1 = currentY + _rowHeight,
                    X2 = totalWidth,
                    Y2 = currentY + _rowHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                _ganttCanvas.Children.Add(separator);

                currentY += _rowHeight;
            }
        }

        private Brush GetBrushFromHex(string hexCode)
        {
            if (string.IsNullOrEmpty(hexCode))
                return Brushes.Gray;
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexCode));
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        private void ShowTaskDetails(ProcessData task)
        {
            string message = $"Рабочее место: {task.WorkCenterName}\n" +
                             $"Процесс: {task.ProcessName}\n" +
                             $"Операция: {task.OpName}\n" +
                             $"Количество: {task.Qty}\n" +
                             $"План: {task.PlanStartDate:dd.MM.yyyy} - {task.PlanEndDate:dd.MM.yyyy}\n" +
                             $"Длительность: {task.WorkTime:F1} час\n";
            MessageBox.Show(message, "Информация о процессе",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void DrawDateHeader(Canvas dateHeaderCanvas)
        {
            if (_filteredWorkDays == null || _filteredWorkDays.Count == 0) return;

            dateHeaderCanvas.Children.Clear();
            double totalWidth = _filteredWorkDays.Count * _pixelsPerDay;
            dateHeaderCanvas.Width = totalWidth;
            dateHeaderCanvas.Height = 50;

            for (int i = 0; i < _filteredWorkDays.Count; i++)
            {
                DateTime date = _filteredWorkDays[i];
                double x = i * _pixelsPerDay;

                var dateText = new TextBlock
                {
                    Text = date.ToString("dd.MM"),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(dateText, x + (_pixelsPerDay - 35) / 2);
                Canvas.SetTop(dateText, 5);
                dateHeaderCanvas.Children.Add(dateText);

                var dayText = new TextBlock
                {
                    Text = date.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(dayText, x + (_pixelsPerDay - 30) / 2);
                Canvas.SetTop(dayText, 28);
                dateHeaderCanvas.Children.Add(dayText);

                var line = new Line
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
        }

        public double GetTotalWidth() => _filteredWorkDays.Count * _pixelsPerDay;
    }
}