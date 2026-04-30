using PlanDiagram.Constants;
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
        private List<DateTime> _workDays;
        private List<ProcessData> _processes;
        private Canvas _ganttCanvas;
        public void Build(List<ProcessData> proc, List<DateTime> workDays, Canvas ganttCanvas)
        {
            _processes = proc;
            _ganttCanvas = ganttCanvas;
            _workDays = workDays ?? new List<DateTime>();
            DrawGanttChart();
        }

        private void DrawGanttChart()
        {
            if (_ganttCanvas == null || _processes == null || _processes.Count == 0 ||
                _workDays == null || _workDays.Count == 0)
                return;

            _ganttCanvas.Children.Clear();

            double totalWidth = _workDays.Count * GlobalConst.PixelsPerDay;

            var workCenters = _processes
                .Select(p => p.WorkCenterName)
                .Distinct()
                .OrderBy(w => w)
                .ToList();

            // Предварительный расчёт загрузки по дням
            var workCenterDayLoadCount = new Dictionary<string, Dictionary<DateTime, int>>();
            foreach (var wc in workCenters)
            {
                var dict = new Dictionary<DateTime, int>();
                var processesForWc = _processes.Where(p => p.WorkCenterName == wc);
                foreach (var day in _workDays)
                {
                    int count = processesForWc.Sum(p => p.WorkTimeDay?.Count(ld => ld.OnDate.Date == day.Date) ?? 0);
                    dict[day] = Math.Max(count, 1);
                }
                workCenterDayLoadCount[wc] = dict;
            }

            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = workCenters.Count * GlobalConst.WCRowHeight;

            double currentY = 0;

            for (int wcIndex = 0; wcIndex < workCenters.Count; wcIndex++)
            {
                string wcName = workCenters[wcIndex];
                var processesForWc = _processes.Where(p => p.WorkCenterName == wcName).ToList();

                // Фон строки
                var rowBackground = new Rectangle
                {
                    Width = totalWidth,
                    Height = GlobalConst.WCRowHeight,
                    Fill = Brushes.White,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(rowBackground, 0);
                Canvas.SetTop(rowBackground, currentY);
                _ganttCanvas.Children.Add(rowBackground);

                for (int dayIndex = 0; dayIndex < _workDays.Count; dayIndex++)
                {
                    DateTime currentDay = _workDays[dayIndex];
                    double x = dayIndex * GlobalConst.PixelsPerDay;

                    var dayLoads = new List<KeyValuePair<ProcessData, LoadingWC>>();
                    foreach (var proc in processesForWc)
                    {
                        if (proc.WorkTimeDay == null) continue;
                        foreach (var load in proc.WorkTimeDay)
                        {
                            if (load.OnDate.Date == currentDay.Date)
                                dayLoads.Add(new KeyValuePair<ProcessData, LoadingWC>(proc, load));
                        }
                    }

                    if (dayLoads.Count == 0) continue;

                    double blockHeight = GlobalConst.WCRowHeight / dayLoads.Count;
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
                            Width = GlobalConst.PixelsPerDay - 2,
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

                var separator = new Line
                {
                    X1 = 0,
                    Y1 = currentY + GlobalConst.WCRowHeight,
                    X2 = totalWidth,
                    Y2 = currentY + GlobalConst.WCRowHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                _ganttCanvas.Children.Add(separator);

                currentY += GlobalConst.WCRowHeight;
            }
        }
    }
}