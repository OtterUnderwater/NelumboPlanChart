using PlanDiagram.Models;
using PlanDiagram.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PlanDiagram
{
    public partial class MainControl : UserControl
    {
        #region [Переменные класса]
        private GanttChart _ganttChartService;
        private PlanRepository _planRepository;
        private List<ProcessData> _allProcess;
        private List<DateTime> _allWorkDays;
        private bool _isSyncing = false;
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
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e)
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

            // Фильтруем задачи
            var filteredTasks = _allProcess
                .Where(t => t.PlanEndDate >= startDate && t.PlanStartDate <= endDate)
                .OrderBy(t => t.PlanStartDate)
                .ToList();

            TasksList.ItemsSource = filteredTasks;

            // Строим диаграмму
            _ganttChartService.Build(startDate, endDate, filteredTasks, GanttCanvas);
            _ganttChartService.DrawDateHeader(DateHeaderCanvas);

            // Сбрасываем прокрутку после построения
            ResetScrollPositions();
        }

        private void GanttItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null && border.Tag is ProcessData task)
            {
                ShowTaskDetails(task);
                e.Handled = true;
            }
        }

        private void TaskItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null && border.DataContext is GanttRowModel row)
            {
                ShowTaskDetails(row.TaskData);
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

        #region [Прокрутка диаграммы - синхронная]

        private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing)
            {
                _isSyncing = true;

                // Синхронизация горизонтали с заголовком дат
                if (e.HorizontalChange != 0 && DateHeaderScrollViewer != null)
                {
                    DateHeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                }

                // Синхронизация вертикали с левым списком
                if (e.VerticalChange != 0 && LeftScrollViewer != null)
                {
                    LeftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                }

                _isSyncing = false;
            }
        }

        private void DateHeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing && e.HorizontalChange != 0 && GanttScrollViewer != null)
            {
                _isSyncing = true;
                GanttScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                _isSyncing = false;
            }
        }

        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_isSyncing && e.VerticalChange != 0 && GanttScrollViewer != null)
            {
                _isSyncing = true;
                GanttScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                _isSyncing = false;
            }
        }

        private void ResetScrollPositions()
        {
            _isSyncing = true;

            GanttScrollViewer?.ScrollToHome();
            DateHeaderScrollViewer?.ScrollToHome();
            LeftScrollViewer?.ScrollToTop();

            _isSyncing = false;
        }

        #endregion
    }
}