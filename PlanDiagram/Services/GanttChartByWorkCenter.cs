using PlanDiagram.Helpers;
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

            // Для каждого рабочего места: для каждого дня – количество записей загрузки
            var workCenterDayLoadCount = new Dictionary<string, Dictionary<DateTime, int>>();
            foreach (var wc in workCenters)
            {
                var dict = new Dictionary<DateTime, int>();
                var processesForWc = _processes.Where(p => p.WorkCenterName == wc);
                foreach (var day in _filteredWorkDays)
                {
                    int count = processesForWc.Sum(p => p.WorkTimeDay?.Count(ld => ld.OnDate.Date == day.Date) ?? 0);
                    dict[day] = Math.Max(count, 1); // минимум 1, чтобы не делить на ноль
                }
                workCenterDayLoadCount[wc] = dict;
            }

            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = workCenters.Count * _rowHeight;

            double currentY = 0;

            for (int wcIndex = 0; wcIndex < workCenters.Count; wcIndex++)
            {
                string wcName = workCenters[wcIndex];
                var processesForWc = _processes.Where(p => p.WorkCenterName == wcName).ToList();

                // Фон строки
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

                    var dayLoads = new List<KeyValuePair<ProcessData, LoadingWC>>();

                    foreach (var proc in processesForWc)
                    {
                        if (proc.WorkTimeDay == null) continue;
                        foreach (var load in proc.WorkTimeDay)
                        {
                            if (load.OnDate.Date == currentDay.Date)
                            {
                                dayLoads.Add(new KeyValuePair<ProcessData, LoadingWC>(proc, load));
                            }
                        }
                    }

                    if (dayLoads.Count == 0) continue;

                    double blockHeight = _rowHeight / dayLoads.Count;

                    var orderedLoads = dayLoads.OrderBy(kvp => kvp.Key.ProcessName).ToList();

                    for (int i = 0; i < orderedLoads.Count; i++)
                    {
                        var kvp = orderedLoads[i];
                        ProcessData proc = kvp.Key;
                        LoadingWC load = kvp.Value;

                        double yOffset = currentY + i * blockHeight;

                        int hours = (int)(load.WorkTimeMin / 60);
                        int minutes = (int)(load.WorkTimeMin % 60);
                        string loadText = load.WorkTimeMin > 0 ? $"{hours} ч {minutes} мин" : "0 мин";

                        var rect = new Rectangle
                        {
                            Width = _pixelsPerDay - 2,
                            Height = blockHeight - 2,
                            Fill = GanttHelper.GetBrushHex(proc.HexCode),
                            RadiusX = 2,
                            RadiusY = 2,
                            Tag = proc,
                            Cursor = Cursors.Hand,
                            ToolTip = $"Процесс: {proc.ProcessName}\n" +
                                      $"Операция: {proc.OpName}\n" +
                                      $"Дата: {load.OnDate:dd.MM.yyyy}\n" +
                                      $"Загрузка: {loadText}\n" +
                                      $"Кол-во: {proc.Qty}"
                        };

                        rect.MouseLeftButtonDown += (s, e) =>
                        {
                            if ((s as Rectangle)?.Tag is ProcessData clickedTask)
                                GanttHelper.ShowDetails(clickedTask);
                        };

                        Canvas.SetLeft(rect, x + 1);
                        Canvas.SetTop(rect, yOffset + 1);
                        _ganttCanvas.Children.Add(rect);  
                    }
                }
                // Разделительная линия
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

        /// <summary>
        /// Отрисовка дат
        /// </summary>
        public void DrawDateHeader(Canvas dateHeaderCanvas)
        {
            GanttHelper.DrawDateHeader(dateHeaderCanvas, _filteredWorkDays, _pixelsPerDay);
        }
        public double GetTotalWidth() => GanttHelper.GetWidth(_filteredWorkDays, _pixelsPerDay);
    }
}