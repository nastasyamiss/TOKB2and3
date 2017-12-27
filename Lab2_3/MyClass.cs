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
   class MyClass
    {
        public static string Path;
        public static byte[] buffer0sector;
        const string alf = "qwertyuiopasdfghjklzxcvbnm0123456789";
        const string fileway = "password.txt";

        private string res;
        private int k, x, z;
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
        string fileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
        [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
        int securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes fileAttributes,
        IntPtr template);

        private string CreateKey()
        {
            string key = "";
            var rnd = new Random();
            var s = new StringBuilder();
            for (int i = 0; i < 3; i++)
                s.Append((char)rnd.Next('a', 'z'));
            key = s.ToString();
            return key;
        }
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
        public bool ComparePass(string pass, string key)
        {
            if (Encryption(pass, key) == SectorOpen()) return true;
            else return false;
        }

        public string SectorOpen()
        {
            string pass = "";
            SafeFileHandle driveHandle = CreateFile(Path, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            using (FileStream disk = new FileStream(driveHandle, FileAccess.Read))
            {
                buffer0sector = new byte[512];
                disk.Read(buffer0sector, 0, 512);
            }

            for (int i = 384; i < 416; i++)
                if((char)buffer0sector[i]!='\0')
                pass += (char)buffer0sector[i];
            //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
            return pass;
        }

        public string SecterSave(string endpass)
        {
            string Key = CreateKey();
            SafeFileHandle driveHandleRead = CreateFile(Path, FileAccess.Write, FileShare.ReadWrite, 0, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
            {
                using (FileStream disk = new FileStream(driveHandleRead, FileAccess.Write))
                {
                    for (int i = 0; i < 32; i++)
                        buffer0sector[384 + i] = 0;
                    String hashpassword = Encryption(endpass, Key);
                    for (int i = 0; i < hashpassword.Length; i++)
                        buffer0sector[384 + i] = (byte)hashpassword[i];

                    disk.Write(buffer0sector, 0, 512);
                    MessageBox.Show("Пароль записан в нулевой сектор!");
                }
            }
            return Key;
        }
    }
}
