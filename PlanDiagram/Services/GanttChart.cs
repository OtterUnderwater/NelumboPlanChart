using PlanDiagram.Helpers;
using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PlanDiagram.Services
{
    public class GanttChart : IGanttChart
    {
        #region [Переменные класса]
        private List<DateTime> _allWorkDays;
        private List<DateTime> _filteredWorkDays;
        private List<ProcessData> _process;
        private double _pixelsPerDay = 45.0;
        private Canvas _ganttCanvas;
        private DateTime _minDate;
        private DateTime _maxDate;
        #endregion

        public GanttChart(List<DateTime> allWorkDays)
        {
            _allWorkDays = allWorkDays;
        }

        /// <summary>
        /// Построение диаграммы Ганта
        /// </summary>
        public void Build(DateTime minDate, DateTime maxDate, List<ProcessData> process, Canvas ganttCanvas)
        {
            _process = process;
            _ganttCanvas = ganttCanvas;
            _minDate = minDate;
            _maxDate = maxDate;

            // Фильтруем рабочие дни по диапазону дат
            _filteredWorkDays = _allWorkDays
                .Where(d => d >= minDate && d <= maxDate)
                .OrderBy(d => d)
                .ToList();

            // Отрисовываем диаграмму
            DrawGanttChart();
        }

        /// <summary>
        /// Обрезает дату задачи по границам фильтра
        /// </summary>
        private DateTime ClipDate(DateTime date, bool isStart)
        {
            if (isStart)
            {
                return date < _minDate ? _minDate : date;
            }
            else
            {
                return date > _maxDate ? _maxDate : date;
            }
        }

        /// <summary>
        /// Отрисовка диаграммы Ганта на Canvas
        /// </summary>
        private void DrawGanttChart()
        {
            if (_ganttCanvas == null || _process == null || _process.Count == 0 ||
                _filteredWorkDays == null || _filteredWorkDays.Count == 0)
                return;

            _ganttCanvas.Children.Clear();

            double totalWidth = GetTotalWidth();
            double rowHeight = 40;
            double currentY = 0;

            _ganttCanvas.Width = totalWidth;
            _ganttCanvas.Height = _process.Count * rowHeight;

            for (int i = 0; i < _process.Count; i++)
            {
                var proc = _process[i];
                double y = currentY + i * rowHeight;

                // Горизонтальная линия сетки
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

                // Обрезаем даты задачи по границам видимой области
                DateTime visibleStart = ClipDate(proc.PlanStartDate, true);
                DateTime visibleEnd = ClipDate(proc.PlanEndDate, false);

                // Проверяем, есть ли пересечение с видимой областью
                if (visibleStart > visibleEnd)
                    continue;

                // Находим индексы рабочих дней для обрезанных дат
                int startIndex = GetWorkingDayIndex(visibleStart);
                int endIndex = GetWorkingDayIndex(visibleEnd);

                // Если даты не найдены в рабочих днях, пропускаем задачу
                if (startIndex == -1 || endIndex == -1)
                    continue;

                // Вычисляем позицию и ширину на основе индексов рабочих дней
                double x = startIndex * _pixelsPerDay;
                double width = (endIndex - startIndex + 1) * _pixelsPerDay;
                if (width < 4) width = 4;

                // Прямоугольник задачи
                var rect = new Rectangle
                {
                    Width = width,
                    Height = rowHeight - 4,
                    Fill  = GanttHelper.GetBrushHex(proc.HexCode),
                    RadiusX = 3,
                    RadiusY = 3,
                    Tag = proc,
                    Cursor = Cursors.Hand,
                    ToolTip = $"План: {proc.PlanStartDate:dd.MM.yyyy} - {proc.PlanEndDate:dd.MM.yyyy}\n" + 
                              $"Раб.место: {proc.WorkCenterName}\n" +
                              $"Кол-во: {proc.Qty}\n" +
                              $"Длит-ть: {proc.WorkTime:F1} ч"
                };

                // Обработчик клика
                rect.MouseLeftButtonDown += (s, e) =>
                {
                    var rectangle = s as Rectangle;
                    if (rectangle?.Tag is ProcessData clickedTask)
                    {
                        GanttHelper.ShowDetails(clickedTask);
                    }
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + 2);
                _ganttCanvas.Children.Add(rect);

                // Текст внутри прямоугольника (если достаточно места)
                if (width > 40)
                {
                    var text = new TextBlock
                    {
                        Text = $"{proc.WorkTime:F1} ч",
                        FontSize = 11,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        IsHitTestVisible = false   // для tooltip
                    };
                    Canvas.SetLeft(text, x + 4);
                    Canvas.SetTop(text, y + 12);
                    _ganttCanvas.Children.Add(text);
                }
            }
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