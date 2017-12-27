using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab2_3
{
    public partial class MyKey : Form
    {
        private MyClass myclass;
        public MyKey()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = "";
            Enter main = this.Owner as Enter;
            myclass = new MyClass();
            if (main != null)
            {
                s = main.textBox1.Text;    //переменной s присваиваем значение textBox1(пароль)
            }
            string key = textBox1.Text;        //введенный ключ
            myclass.SectorOpen();   //зашифрованный пароль из файла

            if (myclass.ComparePass(s, key))
            {
                key = myclass.SecterSave(s);
                WriteToZero cl = new WriteToZero(key);
                cl.Show();
                this.Hide();
            }
            else if (main != null)
            {
                main.a--;
                MessageBox.Show(String.Format("Неправильный пароль или ключ!\nОсталось попыток: {0}", main.a));
                if (main.a == 0) Application.Exit();
                main.Show();
                main.textBox1.Text = "";
            }
            this.Hide();
        }

    }
}
