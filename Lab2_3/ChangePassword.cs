using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Lab2_3
{
    public partial class ChangePassword : Form
    {
        public ChangePassword(string drive)
        {
            InitializeComponent();
            this.path = drive;
        }
        string path;
        const string alf = "qwertyuiopasdfghjklzxcvbnm0123456789";

        private string res;
        private int k, x, z;
        int count=3; //счётчик неправильных вводов пароля

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
        int securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes fileAttributes,
        IntPtr template);
        private string Encryption(string pass, string key)
        {

            res = string.Empty;
            while (key.Length < pass.Length)
            {
                key += key;
                if (key.Length > pass.Length) key = key.Remove(pass.Length);
            }
            for (int i = 0; i < pass.Length; i++)
            {
                for (int id = 0; id < alf.Length; id++)
                {
                    if (key[i] == alf[id]) k = id;
                    if (pass[i] == alf[id]) x = id;
                    z = (x + k) % alf.Length;
                }
                res += alf[z];
            }
            return res;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text=="")
            {
                MessageBox.Show("Заполните все поля");
            }
            string oldpassword = Encryption(textBox1.Text, "abc");

            //защита от подмены флешки путем перечитывания нулевого сектора
            //читаем информацию из нулевого сектора для проверки есть ли пароль
            SafeFileHandle driveHandle = CreateFile(path, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            using (FileStream disk = new FileStream(driveHandle, FileAccess.Read))
            {
                WriteToZero.buffer0sector = new byte[512];
                disk.Read(WriteToZero.buffer0sector, 0, 512);
            }
            byte[] pass = new byte[32];
            for (int a=0; a<oldpassword.Length; a++)
            {
                    pass[a] = (byte)oldpassword[a];
                
            }
            //сравниваем побитно хеш старого пароля и нового   
            for (int ii = 0; ii < 32; ii++)
            
                if (WriteToZero.buffer0sector[384 + ii] != pass[ii])
                {
                    textBox1.Text = "";
                    count--;
                    MessageBox.Show(String.Format("Неправильный пароль!\nОсталось попыток: {0}", count));
                    if (count == 0)
                    {
                        MessageBox.Show("У Вас закончились попытки ввода пароль!");
                        Application.Exit();
                    }
                    return;
                }
                
                {
                    //записываем пароль в нулевой сектор
                    SafeFileHandle driveHandleRead = CreateFile(path, FileAccess.Write, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
                    {
                        using (FileStream disk = new FileStream(driveHandleRead, FileAccess.Write))
                        {
                            String hashpassword = Encryption(textBox1.Text, "abc");
                            for (int i = 0; i < hashpassword.Length; i++)
                                WriteToZero.buffer0sector[384 + i] = (byte)hashpassword[i];

                            disk.Write(WriteToZero.buffer0sector, 0, 512);
                            MessageBox.Show("Пароль записан в нулевой сектор!");
                            
                            WriteToZero myclass = new WriteToZero();
                            myclass.Show();
                            this.Close();
                        }
                    }
                }
            
        }
    }
}
