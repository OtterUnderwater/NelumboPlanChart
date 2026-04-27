using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace PlanDiagram.WinForms
{
    public partial class PlanPanel : UserControl
    {
        private ElementHost _elementHost;
        private MainControl _wpfControl;

        public PlanPanel(Hashtable parameters)
        {
            try
            {
                InitializeComponent();

                // Создаем контейнер для WPF
                _elementHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Name = "wpfHost"
                };

                _wpfControl = new MainControl(parameters);
                    
                _elementHost.Child = _wpfControl;

                Controls.Add(_elementHost);

                ConfigurePanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке диаграммы:\n\n{ex.Message}\n\n",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }
        private void ConfigurePanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
        }
    }
}
