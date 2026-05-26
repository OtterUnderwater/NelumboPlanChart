using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using PlanDiagram.Helpers;
using PlanDiagram.Interfaces;
using PlanDiagram.Models;
using PlanDiagram.Services;

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
        private Visibility _showColorIndicators = Visibility.Collapsed;
        public Visibility ShowColorIndicators
        {
            get => _showColorIndicators;
            set { _showColorIndicators = value; OnPropertyChanged(nameof(ShowColorIndicators)); }
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
            {
                _ganttChartService = new GanttChartByWorkCenter();
                ShowColorIndicators = Visibility.Collapsed;
            }
            else
            {
                _ganttChartService = new GanttChart();
                ShowColorIndicators = Visibility.Visible;
            }
            SetDefaultDates(orderID);
        }  
        private void UpdateLeftColumn(List<ProcessData> sourceTasks = null)
        {
            if (sourceTasks == null) sourceTasks = _allProcess;

            if (_ganttChartService is GanttChartByWorkCenter)
            {
                // Список рабочих мест 
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
                UpdateLeftColumn(); // обновим левую колонку
                return;
            }

            StartDatePicker.SelectedDate = _allProcess.Min(t => t.PlanStartTime);
            EndDatePicker.SelectedDate = _allProcess.Max(t => t.PlanEndTime);
            UpdatePlan();
        }
        private void BuildButton_Click(object sender, RoutedEventArgs e) => UpdatePlan();
       
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
            var filteredProc = _allProcess
             .Where(p => DateHelper.IsDateRange(p.PlanStartTime, p.PlanEndTime, startDate, endDate))
             .OrderBy(p => p.PlanStartTime).ThenBy(p => p.PlanEndTime)
             .ToList();

            // Фильтруем рабочие дни по тому же диапазону
            var filteredWorkDays = _allWorkDays
                .Where(d => d >= startDate && d <= endDate)
                .OrderBy(d => d)
                .ToList();

            // Обновляем левую колонку в соответствии с отфильтрованными задачами
            UpdateLeftColumn(filteredProc);

            // Строим диаграмму
            GanttCanvas.Children.Clear();
            DateHeaderCanvas.Children.Clear();

            // Передаём отфильтрованные рабочие дни в gantt-класс
            _ganttChartService.Build(filteredProc, filteredWorkDays, GanttCanvas);

            GanttHelper.DrawDateHeader(DateHeaderCanvas, filteredWorkDays);
            ResetScrollPositions();
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
