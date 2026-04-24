using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace PlanDiagram.WinForms
{
    public partial class PlanForm : Form
    {
        private ElementHost _elementHost;
        private MainControl _wpfControl;

        public PlanForm(Hashtable parameters)
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

                ConfigureForm();
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

        private void ConfigureForm()
        {
            this.Text = "Продуктовый план";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.White;
        }
    }
}
