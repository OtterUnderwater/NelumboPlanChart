using PlanDiagram.Models;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PlanDiagram.Helpers
{
    public static class GanttHelper
    {
        /// <summary>
        /// Вывод детелай процесса
        /// </summary>
        /// <param name="proc"></param>
        public static void ShowDetails(ProcessData proc)
        {
            string message = $"Рабочее место: {proc.WorkCenterName}\n" +
                             $"Процесс: {proc.ProcessName}\n" +
                             $"Операция: {proc.OpName}\n" +
                             $"Количество: {proc.Qty}\n" +
                             $"План: {proc.PlanStartDate:dd.MM.yyyy} - {proc.PlanEndDate:dd.MM.yyyy}\n" +
                             $"Длительность: {proc.WorkTime:F1} час\n";

            MessageBox.Show(message, "Информация о процессе",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Получаем цвет из hex кода
        /// </summary>
        /// <param name="hexCode"></param>
        /// <returns></returns>
        public static Brush GetBrushHex(string hexCode)
        {
            if (string.IsNullOrEmpty(hexCode)) return Brushes.Gray;
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexCode));
            }
            catch
            {
                return Brushes.Gray;
            }
        }
        
        /// <summary>
        /// Отрисовка заголовка с датами
        /// </summary>
        public static void DrawDateHeader(Canvas dateHeaderCanvas, List<DateTime> filteredWorkDays, double pixelsPerDay)
        {
            if (filteredWorkDays == null || filteredWorkDays.Count == 0) return;

            dateHeaderCanvas.Children.Clear();
            double totalWidth = filteredWorkDays.Count * pixelsPerDay;
            dateHeaderCanvas.Width = totalWidth;
            dateHeaderCanvas.Height = 50;

            for (int i = 0; i < filteredWorkDays.Count; i++)
            {
                DateTime date = filteredWorkDays[i];
                double x = i * pixelsPerDay;

                var dateText = new TextBlock
                {
                    Text = date.ToString("dd.MM"),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(dateText, x + (pixelsPerDay - 35) / 2);
                Canvas.SetTop(dateText, 5);
                dateHeaderCanvas.Children.Add(dateText);

                var dayText = new TextBlock
                {
                    Text = date.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(dayText, x + (pixelsPerDay - 30) / 2);
                Canvas.SetTop(dayText, 28);
                dateHeaderCanvas.Children.Add(dayText);

                var line = new Line
                {
                    X1 = x + pixelsPerDay,
                    Y1 = 0,
                    X2 = x + pixelsPerDay,
                    Y2 = 50,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                dateHeaderCanvas.Children.Add(line);
            }

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

        /// <summary>
        /// Расчёт общей ширины диаграммы
        /// </summary>
        public static double GetWidth(List<DateTime> filteredWorkDays, double pixelsPerDay)
        {
            return filteredWorkDays != null ? filteredWorkDays.Count * pixelsPerDay : 0;
        }
    }
}
