using PlanDiagram.Constants;
using PlanDiagram.Helpers;
using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            DateTime viewStart = _workDays.First();
            DateTime viewEnd = _workDays.Last();

            for (int i = 0; i < _process.Count; i++)
            {
                var proc = _process[i];
                double y = i * rowHeight;

                // Линия сетки
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

                // Отрисовка планового периода (прямоугольник)
                DrawPeriod(proc.PlanStartTime, proc.PlanEndTime, viewStart, viewEnd, y, rowHeight - 4, 
                    isRect: true, proc);

                // Отрисовка фактического периода (линия поверх)
                if (proc.StartTime.HasValue)
                {
                    DrawPeriod(proc.StartTime.Value, proc.EndTime, viewStart, viewEnd, y, rowHeight - 4, 
                        isRect: false, proc);
                }
            }
        }

        private void DrawPeriod(DateTime periodStart, DateTime? dateEnd, DateTime viewStart, DateTime viewEnd, double y, double height, bool isRect, ProcessData proc)
        {
            DateTime periodEnd = (dateEnd.HasValue) ? dateEnd.Value : DateTime.Today;

            // Проверяем пересечение периодов
            if (periodEnd < viewStart || periodStart > viewEnd)
                return;

            // Вычисляем видимую часть периода
            DateTime visibleStart = periodStart < viewStart ? viewStart : periodStart;
            DateTime visibleEnd = periodEnd > viewEnd ? viewEnd : periodEnd;

            // Получаем индексы рабочих дней
            int startIndex = GetWorkingDayIndex(visibleStart);
            int endIndex = GetWorkingDayIndex(visibleEnd);

            if (startIndex == -1 || endIndex == -1)
                return;

            // Вычисляем координаты
            double left = startIndex * GlobalConst.PixelsPerDay;
            double width = (endIndex - startIndex + 1) * GlobalConst.PixelsPerDay;

            if (width <= 0)
                return;

            if (isRect)
            {
                var rect = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = GanttHelper.GetColor(proc),
                    RadiusX = 3,
                    RadiusY = 3,
                    Tag = proc,
                    Cursor = Cursors.Hand,
                    ToolTip = $"План: {proc.PlanStartTime:dd.MM.yyyy} - {proc.PlanEndTime:dd.MM.yyyy}\n" +
                              $"Раб.место: {proc.WorkCenterName}\n" +
                              $"Кол-во: {proc.Qty}\n" +
                              $"Длит-ть: {proc.FullWorkTimeH:F1} ч"
                };

                rect.MouseLeftButtonDown += (s, e) =>
                {
                    if ((s as Rectangle)?.Tag is ProcessData clickedTask)
                        GanttHelper.ShowDetails(clickedTask);
                };

                Canvas.SetLeft(rect, left);
                Canvas.SetTop(rect, y + 2);
                _ganttCanvas.Children.Add(rect);

                // Текст внутри планового прямоугольника
                if (width > 40)
                {
                    var text = new TextBlock
                    {
                        Text = $"{proc.FullWorkTimeH:F1} ч",
                        FontSize = 11,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(text, left + 4);
                    Canvas.SetTop(text, y + 12);
                    _ganttCanvas.Children.Add(text);
                }
            }
            else
            {
                // Линия фактического периода
                var line = new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = width,
                    Y2 = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 3,
                    StrokeDashArray = new DoubleCollection { 2, 2 } // Пунктир для линии факта
                };

                double verticalCenter = y + (height / 2) + 2; // Центрируем по высоте прямоугольника
                Canvas.SetLeft(line, left);
                Canvas.SetTop(line, verticalCenter);
                _ganttCanvas.Children.Add(line);
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