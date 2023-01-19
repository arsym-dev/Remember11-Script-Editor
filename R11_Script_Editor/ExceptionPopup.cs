using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace R11_Script_Editor.FileTypes
{
    public partial class ExceptionPopup : Form
    {
        public ExceptionPopup(Exception e)
        {
            InitializeComponent();

            textBox1.Text = e.ToString();
        }
    }
}
