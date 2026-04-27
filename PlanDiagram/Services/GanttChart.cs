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
    public class GanttChart
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
                var task = _process[i];
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
                DateTime visibleStart = ClipDate(task.PlanStartDate, true);
                DateTime visibleEnd = ClipDate(task.PlanEndDate, false);

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
                    Fill = new SolidColorBrush(Colors.DodgerBlue),
                    RadiusX = 3,
                    RadiusY = 3,
                    Tag = task,
                    Cursor = Cursors.Hand,
                    ToolTip = $"{task.ProcessName}\n" +
                             $"План: {task.PlanStartDate:dd.MM.yyyy} - {task.PlanEndDate:dd.MM.yyyy}\n" +
                             $"Видимый период: {visibleStart:dd.MM.yyyy} - {visibleEnd:dd.MM.yyyy}\n" +
                             $"Длительность: {task.WorkTime:F1} ч"
                };

                // Обработчик клика
                rect.MouseLeftButtonDown += (s, e) =>
                {
                    var rectangle = s as Rectangle;
                    if (rectangle?.Tag is ProcessData clickedTask)
                    {
                        ShowTaskDetails(clickedTask);
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

        /// <summary>
        /// Отображение деталей задачи
        /// </summary>
        private void ShowTaskDetails(ProcessData task)
        {
            string message = $"Рабочее место: {task.WorkCenterName}\n" +
                            $"Процесс: {task.ProcessName}\n" +
                            $"Операция: {task.OpName}\n" +
                            $"Количество: {task.Qty}\n" +
                            $"Плановая дата начала: {task.PlanStartDate:dd.MM.yyyy}\n" +
                            $"Плановая дата окончания: {task.PlanEndDate:dd.MM.yyyy}\n" +
                            $"Длительность: {task.WorkTime:F1} часов\n";

            MessageBox.Show(message, "Информация о процессе",
                          MessageBoxButton.OK, MessageBoxImage.Information);
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
        /// Получение общей ширины диаграммы
        /// </summary>
        public double GetTotalWidth()
        {
            return _filteredWorkDays != null ? _filteredWorkDays.Count * _pixelsPerDay : 0;
        }

        /// <summary>
        /// Отрисовка заголовка с датами
        /// </summary>
        public void DrawDateHeader(Canvas dateHeaderCanvas)
        {
            if (_filteredWorkDays == null || _filteredWorkDays.Count == 0) return;

            dateHeaderCanvas.Children.Clear();

            double totalWidth = GetTotalWidth();
            dateHeaderCanvas.Width = totalWidth;
            dateHeaderCanvas.Height = 50;

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
            dateHeaderCanvas.Children.Add(leftBorder);
        }
    }
}