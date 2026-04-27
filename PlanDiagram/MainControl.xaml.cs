using PlanDiagram.Models;
using PlanDiagram.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PlanDiagram
{
    public partial class MainControl : UserControl
    {
        #region [Переменные класса]
        private GanttChart _ganttChartService;
        private PlanRepository _planRepository;
        private List<ProcessData> _allProcess;
        private List<DateTime> _allWorkDays;
        #endregion
        public MainControl(Hashtable parameters)
        {
            InitializeComponent();

            string connectionString = (string)parameters["ConnectionString"];
            int regID = (int)parameters["RegID"];
            int? orderID = (int?)parameters["OrderID"];

            _planRepository = new PlanRepository(connectionString, orderID);
            _allProcess = _planRepository.GetPlanList();
            _allWorkDays = _planRepository.GetWorkingDaysFromCalendar();

            _ganttChartService = new GanttChart(_allWorkDays);

            SetDefaultDates(orderID);
            TasksList.ItemsSource = _allProcess;
        }

        private void SetDefaultDates(int? OrderID)
        {
            if (_allProcess == null || _allProcess.Count == 0 || OrderID == null)
            {
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);
                return;
            }

            StartDatePicker.SelectedDate = _allProcess.Min(t => t.PlanStartDate);
            EndDatePicker.SelectedDate = _allProcess.Max(t => t.PlanEndDate);
            UpdatePlan();
        }

        /// <summary>
        /// Проверяет, пересекаются ли два периода дат
        /// </summary>
        /// <param name="taskStart">Начало задачи</param>
        /// <param name="taskEnd">Конец задачи</param>
        /// <param name="filterStart">Начало фильтра</param>
        /// <param name="filterEnd">Конец фильтра</param>
        /// <returns>True, если периоды пересекаются</returns>
        private bool IsDateRangeIntersects(DateTime taskStart, DateTime taskEnd, DateTime filterStart, DateTime filterEnd)
        {
            return Max(taskStart, filterStart) <= Min(taskEnd, filterEnd);
        }
        private DateTime Max(DateTime date1, DateTime date2) => date1 > date2 ? date1 : date2;
        private DateTime Min(DateTime date1, DateTime date2) => date1 < date2 ? date1 : date2;

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatePlan();
        }

        private void UpdatePlan()
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите начальную и конечную дату", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Фильтруем задачи с использованием IsDateRangeIntersects
            var filteredTasks = _allProcess
                .Where(t => IsDateRangeIntersects(t.PlanStartDate, t.PlanEndDate, startDate, endDate))
                .OrderBy(t => t.PlanStartDate)
                .ThenBy(t => t.PlanEndDate)
                .ToList();

            TasksList.ItemsSource = filteredTasks;

            // Передаем отфильтрованные данные в GanttChart и строим диаграмму
            _ganttChartService.Build(startDate, endDate, filteredTasks, GanttCanvas);
            _ganttChartService.DrawDateHeader(DateHeaderCanvas);

            // Сбрасываем прокрутку после построения
            ResetScrollPositions();
        }

        private void GanttItem_Click(object sender, MouseButtonEventArgs e)
        {
            var rectangle = sender as Rectangle;
            if (rectangle != null && rectangle.Tag is ProcessData task)
            {
                ShowTaskDetails(task);
                e.Handled = true;
            }
        }

        private void TaskItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null && border.DataContext is ProcessData task)
            {
                ShowTaskDetails(task);
                e.Handled = true;
            }
        }

        private void ShowTaskDetails(ProcessData process)
        {
            string message = $"Рабочее место: {process.WorkCenterName}\n" +
                            $"Процесс: {process.ProcessName}\n" +
                            $"Операция: {process.OpName}\n" +
                            $"Количество: {process.Qty}\n" +
                            $"Плановая дата начала: {process.PlanStartDate:dd.MM.yyyy}\n" +
                            $"Плановая дата окончания: {process.PlanEndDate:dd.MM.yyyy}\n" +
                            $"Длительность: {process.WorkTime:F1} часов\n";

            MessageBox.Show(message, "Информация о процессе",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region [Прокрутка диаграммы]

        private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            DateHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void DateHeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void ResetScrollPositions()
        {
            GanttScrollViewer?.ScrollToHome();
            DateHeaderScrollViewer?.ScrollToHome();
            LeftScrollViewer?.ScrollToTop();
        }

        #endregion
    }
}