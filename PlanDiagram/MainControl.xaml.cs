using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using PlanDiagram.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PlanDiagram
{
    public partial class MainControl : UserControl, INotifyPropertyChanged
    {
        #region [Переменные класса]
        private IGanttChart _ganttChartService;
        private IEnumerable _leftColumnItems;
        private string _leftColumnTitle;
        private PlanRepository _planRepository;
        private List<ProcessData> _allProcess;
        private List<DateTime> _allWorkDays;
        #endregion

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();
            DataContext = this;

            string connectionString = (string)parameters["ConnectionString"];
            int regID = (int)parameters["RegID"];
            int? orderID = (int?)parameters["OrderID"];

            _planRepository = new PlanRepository(connectionString, orderID);
            _allProcess = _planRepository.GetPlanList();
            _allWorkDays = _planRepository.GetWorkingDaysFromCalendar();

            if (orderID == null)
                _ganttChartService = new GanttChartByWorkCenter(_allWorkDays);
            else
                _ganttChartService = new GanttChart(_allWorkDays);

            SetDefaultDates(orderID);
        }

        public IEnumerable LeftColumnItems
        {
            get => _leftColumnItems;
            set { _leftColumnItems = value; OnPropertyChanged(nameof(LeftColumnItems)); }
        }

        public string LeftColumnTitle
        {
            get => _leftColumnTitle;
            set { _leftColumnTitle = value; OnPropertyChanged(nameof(LeftColumnTitle)); }
        }

        private void UpdateLeftColumn(List<ProcessData> sourceTasks = null)
        {
            if (sourceTasks == null) sourceTasks = _allProcess;

            if (_ganttChartService is GanttChartByWorkCenter)
            {
                // Список уникальных рабочих мест (отсортированный)
                var workCenters = sourceTasks
                    .Select(p => p.WorkCenterName)
                    .Distinct()
                    .OrderBy(w => w)
                    .ToList();
                LeftColumnItems = workCenters;
                LeftColumnTitle = "Рабочее место";
            }
            else
            {
                // Список процессов
                LeftColumnItems = sourceTasks;
                LeftColumnTitle = "Процесс / Операция";
            }
        }

        private void SetDefaultDates(int? OrderID)
        {
            if (_allProcess == null || _allProcess.Count == 0 || OrderID == null)
            {
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);
                UpdateLeftColumn(); // обновим левую колонку (может быть пустой)
                return;
            }

            StartDatePicker.SelectedDate = _allProcess.Min(t => t.PlanStartDate);
            EndDatePicker.SelectedDate = _allProcess.Max(t => t.PlanEndDate);
            UpdatePlan();
        }

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

            // Фильтруем задачи по диапазону дат
            var filteredTasks = _allProcess
                .Where(t => IsDateRangeIntersects(t.PlanStartDate, t.PlanEndDate, startDate, endDate))
                .OrderBy(t => t.PlanStartDate)
                .ThenBy(t => t.PlanEndDate)
                .ToList();

            // Обновляем левую колонку в соответствии с отфильтрованными задачами
            UpdateLeftColumn(filteredTasks);

            // Строим диаграмму
            GanttCanvas.Children.Clear();
            DateHeaderCanvas.Children.Clear();
            _ganttChartService.Build(startDate, endDate, filteredTasks, GanttCanvas);
            _ganttChartService.DrawDateHeader(DateHeaderCanvas);

            ResetScrollPositions();
        }

        private void GanttItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rectangle && rectangle.Tag is ProcessData task)
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

        #region Прокрутка
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}