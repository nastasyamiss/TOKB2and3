using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab2_3
{
    public partial class Enter : Form
    {
        public int a = 3;
        public Enter()
        {
            InitializeComponent();
        }
        MyClass myclass = new MyClass();
        private void button1_Click(object sender, EventArgs e)
        {
            MyClass.Path = "\\\\.\\" + comboBoxdisk.Text.Remove(2);
            myclass.SectorOpen();
            int summa = 0;
            for (int i = 384; i < 416; i++)
                summa += MyClass.buffer0sector[i];
            //если сумма отличается от нуля, значит пароль в нулевом секторе есть
            if (summa > 0)
            {
                //пароля нет
                //создаем новое окно
                this.Hide();
                MyKey cl = new MyKey(); //Вывод на экран окна успешного ввода
                cl.Owner = this;
                cl.Show();
            }
            else
            {
                string key = myclass.SecterSave(textBox1.Text);
                WriteToZero cl = new WriteToZero(key);
               
                this.Hide();
                cl.Show();
            }
        }

        private void Enter_Load(object sender, EventArgs e)
        {
            string mydrive;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && (d.DriveType == DriveType.Removable))
                {
                    mydrive = d.Name;
                    comboBoxdisk.Items.Add(mydrive);
                }
            }
            comboBoxdisk.SelectedIndex = 0;
            if (comboBoxdisk.Items.Count == 0)
            {
                comboBoxdisk.Items.Add("Внешние носители отсутствуют");
            }
        }
    }
}
