using PlanDiagram.Models;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PlanDiagram.Constants;
using System.Threading.Tasks;

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
                             $"План: {proc.PlanStartTime:dd.MM.yyyy} - {proc.PlanEndTime:dd.MM.yyyy}\n" +
                             $"Длительность: {proc.FullWorkTimeH:F1} час\n";

            MessageBox.Show(message, "Информация о процессе",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Отрисовка заголовка с датами
        /// </summary>
        public static void DrawDateHeader(Canvas dateHeaderCanvas, List<DateTime> filteredWorkDays)
        {
            if (filteredWorkDays == null || filteredWorkDays.Count == 0) return;

            dateHeaderCanvas.Children.Clear();
            double totalWidth = filteredWorkDays.Count * GlobalConst.PixelsPerDay;
            dateHeaderCanvas.Width = totalWidth;
            dateHeaderCanvas.Height = 50;

            for (int i = 0; i < filteredWorkDays.Count; i++)
            {
                DateTime date = filteredWorkDays[i];
                double x = i * GlobalConst.PixelsPerDay;

                var dateText = new TextBlock
                {
                    Text = date.ToString("dd.MM"),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(dateText, x + (GlobalConst.PixelsPerDay - 35) / 2);
                Canvas.SetTop(dateText, 5);
                dateHeaderCanvas.Children.Add(dateText);

                var dayText = new TextBlock
                {
                    Text = date.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(dayText, x + (GlobalConst.PixelsPerDay - 30) / 2);
                Canvas.SetTop(dayText, 28);
                dateHeaderCanvas.Children.Add(dayText);

                var line = new Line
                {
                    X1 = x + GlobalConst.PixelsPerDay,
                    Y1 = 0,
                    X2 = x + GlobalConst.PixelsPerDay,
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

        public static Brush GetColor(int status)
        {
            SolidColorBrush blue = new SolidColorBrush(Color.FromArgb(180, 100, 149, 237));
            SolidColorBrush yellow = new SolidColorBrush(Color.FromArgb(180, 255, 220, 100));
            SolidColorBrush green = new SolidColorBrush(Color.FromArgb(180, 144, 238, 144));
            SolidColorBrush darkGreen = new SolidColorBrush(Color.FromArgb(76, 144, 175, 80));
            SolidColorBrush red = new SolidColorBrush(Color.FromArgb(180, 255, 100, 100));
            SolidColorBrush gray = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200));
            SolidColorBrush orange = new SolidColorBrush(Color.FromArgb(180, 255, 165, 0));
            switch (status)
            {
                //case "Назначена": return blue;
                //case "Исполняется": return (task.PlanExecDate < DateTime.Today && !task.FactExecDate.HasValue) ? red : yellow;
                //case "Пауза": return (task.PlanExecDate < DateTime.Today && !task.FactExecDate.HasValue) ? red : orange;
                //case "Выполнена":  return (task.FactExecDate.HasValue && task.FactExecDate.Value <= task.PlanExecDate) ? green : red;
                //case "Завершена": return (task.FactExecDate.HasValue && task.FactExecDate.Value <= task.PlanExecDate) ? darkGreen : red;
               
                default:
                    return gray;
            }
        }

    }
}
