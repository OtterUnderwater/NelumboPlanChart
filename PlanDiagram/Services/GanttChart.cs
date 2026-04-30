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
    public class GanttChart : IGanttChart
    {
        private List<DateTime> _workDays;
        private List<ProcessData> _process;
        private Canvas _ganttCanvas;

        /// <summary>
        /// Построение диаграммы Ганта
        /// </summary>
        public void Build(List<ProcessData> proc, List<DateTime> workDays, Canvas ganttCanvas)
        {
            _process = proc;
            _ganttCanvas = ganttCanvas;
            _workDays = workDays ?? new List<DateTime>();

            DrawGanttChart();
        }

        /// <summary>
        /// Обрезает дату задачи по границам отображаемых рабочих дней
        /// </summary>
        private DateTime ClipDate(DateTime date, bool isStart)
        {
            if (_workDays.Count == 0) return date;

            DateTime firstDay = _workDays.First();
            DateTime lastDay = _workDays.Last();

            if (isStart)
                return date < firstDay ? firstDay : date;
            else
                return date > lastDay ? lastDay : date;
        }

        private void DrawGanttChart()
        {
            if (_ganttCanvas == null || _process == null || _process.Count == 0 ||
                _workDays == null || _workDays.Count == 0)
                return;

            _ganttCanvas.Children.Clear();

            double totalWidth = _workDays.Count * GlobalConst.PixelsPerDay;
            double rowHeight = 40;
            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = _process.Count * rowHeight;

            for (int i = 0; i < _process.Count; i++)
            {
                var proc = _process[i];
                double y = i * rowHeight;

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

                DateTime visibleStart = ClipDate(proc.PlanStartDate, true);
                DateTime visibleEnd = ClipDate(proc.PlanEndDate, false);
                if (visibleStart > visibleEnd) continue;

                int startIndex = GetWorkingDayIndex(visibleStart);
                int endIndex = GetWorkingDayIndex(visibleEnd);
                if (startIndex == -1 || endIndex == -1) continue;

                double x = startIndex * GlobalConst.PixelsPerDay;
                double width = (endIndex - startIndex + 1) * GlobalConst.PixelsPerDay;
                if (width < 4) width = 4;

                var rect = new Rectangle
                {
                    Width = width,
                    Height = rowHeight - 4,
                    Fill = GanttHelper.GetBrushHex(proc.HexCode),
                    RadiusX = 3,
                    RadiusY = 3,
                    Tag = proc,
                    Cursor = Cursors.Hand,
                    ToolTip = $"План: {proc.PlanStartDate:dd.MM.yyyy} - {proc.PlanEndDate:dd.MM.yyyy}\n" +
                              $"Раб.место: {proc.WorkCenterName}\n" +
                              $"Кол-во: {proc.Qty}\n" +
                              $"Длит-ть: {proc.WorkTime:F1} ч"
                };

                rect.MouseLeftButtonDown += (s, e) =>
                {
                    if ((s as Rectangle)?.Tag is ProcessData clickedTask)
                        GanttHelper.ShowDetails(clickedTask);
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + 2);
                _ganttCanvas.Children.Add(rect);

                if (width > 40)
                {
                    var text = new TextBlock
                    {
                        Text = $"{proc.WorkTime:F1} ч",
                        FontSize = 11,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(text, x + 4);
                    Canvas.SetTop(text, y + 12);
                    _ganttCanvas.Children.Add(text);
                }
            }
        }

        private int GetWorkingDayIndex(DateTime date)
        {
            for (int i = 0; i < _workDays.Count; i++)
                if (_workDays[i].Date == date.Date)
                    return i;
            return -1;
        }
    }
}