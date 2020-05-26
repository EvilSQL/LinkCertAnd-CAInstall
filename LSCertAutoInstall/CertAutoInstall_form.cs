using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using LSCertAutoInstall.Properties;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using CryptoPro.Sharpei;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;

namespace LinkCRequest
{
    public partial class CertAutoInstall_form : Form
    {
        public static List<string> CAList = new List<string>();

        public static int installcount = 0;
        public static int installccount = 0;
        public static int cainstallcount = 0;

        public static Thread ThCertInstall;

        public string CN = null;
        public bool link = true;

        public CertAutoInstall_form()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;
            this.FormClosing += Form1_FormClosing;
        }

        private static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("LinqBridge"))
            {
                using (var resource = new MemoryStream(Resources.LinqBridge_dll))
                using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflated))
                {
                    var one_megabyte = 384 * 384;
                    var buffer = reader.ReadBytes(one_megabyte);
                    return Assembly.Load(buffer);
                }
            }

            if (args.Name.Contains("CryptoPro.Sharpei.Base"))
            {
                using (var resource = new MemoryStream(Resources.CryptoPro_Sharpei_Base_dll))
                using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflated))
                {
                    var one_megabyte = 512 * 512;
                    var buffer = reader.ReadBytes(one_megabyte);
                    return Assembly.Load(buffer);
                }
            }

            if (args.Name.Contains("CryptoPro.Sharpei.CorLib"))
            {
                using (var resource = new MemoryStream(Resources.CryptoPro_Sharpei_CorLib_dll))
                using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflated))
                {
                    var one_megabyte = 384 * 384;
                    var buffer = reader.ReadBytes(one_megabyte);
                    return Assembly.Load(buffer);
                }
            }

            if (args.Name.Contains("BouncyCastle.Crypto"))
            {
                using (var resource = new MemoryStream(Resources.BouncyCastle_Crypto_dll))
                using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
                using (var reader = new BinaryReader(deflated))
                {
                    var one_megabyte = 2048 * 2048;
                    var buffer = reader.ReadBytes(one_megabyte);
                    return Assembly.Load(buffer);
                }
            }

            return null;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CryptAcquireContext(ref IntPtr hProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetProvParam(IntPtr hProv, uint dwParam, [In, Out] byte[] pbData, ref uint dwDataLen, uint dwFlags);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetProvParam(IntPtr hProv, uint dwParam, [MarshalAs(UnmanagedType.LPStr)] StringBuilder pbData, ref uint dwDataLen, uint dwFlags);
        [DllImport("advapi32.dll")]
        public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Environment.Exit(0);
        }

        public void CertAutoInstall_form_Load(object sender, EventArgs e)
        {
            var CA1 = @"-----BEGIN CERTIFICATE-----MIIFGTCCBMigAwIBAgIQNGgeQMtB7zOpoLfIdpKaKTAIBgYqhQMCAgMwggFKMR4wHAYJKoZIhvcNAQkBFg9kaXRAbWluc3Z5YXoucnUxCzAJBgNVBAYTAlJVMRwwGgYDVQQIDBM3NyDQsy4g0JzQvtGB0LrQstCwMRUwEwYDVQQHDAzQnNC+0YHQutCy0LAxPzA9BgNVBAkMNjEyNTM3NSDQsy4g0JzQvtGB0LrQstCwLCDRg9C7LiDQotCy0LXRgNGB0LrQsNGPLCDQtC4gNzEsMCoGA1UECgwj0JzQuNC90LrQvtC80YHQstGP0LfRjCDQoNC+0YHRgdC40LgxGDAWBgUqhQNkARINMTA0NzcwMjAyNjcwMTEaMBgGCCqFAwOBAwEBEgwwMDc3MTA0NzQzNzUxQTA/BgNVBAMMONCT0L7Qu9C+0LLQvdC+0Lkg0YPQtNC+0YHRgtC+0LLQtdGA0Y/RjtGJ0LjQuSDRhtC10L3RgtGAMB4XDTEyMDcyMDEyMzExNFoXDTI3MDcxNzEyMzExNFowggFKMR4wHAYJKoZIhvcNAQkBFg9kaXRAbWluc3Z5YXoucnUxCzAJBgNVBAYTAlJVMRwwGgYDVQQIDBM3NyDQsy4g0JzQvtGB0LrQstCwMRUwEwYDVQQHDAzQnNC+0YHQutCy0LAxPzA9BgNVBAkMNjEyNTM3NSDQsy4g0JzQvtGB0LrQstCwLCDRg9C7LiDQotCy0LXRgNGB0LrQsNGPLCDQtC4gNzEsMCoGA1UECgwj0JzQuNC90LrQvtC80YHQstGP0LfRjCDQoNC+0YHRgdC40LgxGDAWBgUqhQNkARINMTA0NzcwMjAyNjcwMTEaMBgGCCqFAwOBAwEBEgwwMDc3MTA0NzQzNzUxQTA/BgNVBAMMONCT0L7Qu9C+0LLQvdC+0Lkg0YPQtNC+0YHRgtC+0LLQtdGA0Y/RjtGJ0LjQuSDRhtC10L3RgtGAMGMwHAYGKoUDAgITMBIGByqFAwICIwEGByqFAwICHgEDQwAEQI+lv3kQI8jWka1kMVdbvpvFioP0Pyn3Knmp+2XD6KgPWnXEIlSRX8g/IYracDr51YsNc2KE3C7mkH6hA3M3ofujggGCMIIBfjCBxgYFKoUDZHAEgbwwgbkMI9Cf0JDQmtCcIMKr0JrRgNC40L/RgtC+0J/RgNC+IEhTTcK7DCDQn9CQ0JogwqvQk9C+0LvQvtCy0L3QvtC5INCj0KbCuww20JfQsNC60LvRjtGH0LXQvdC40LUg4oSWIDE0OS8zLzIvMi05OTkg0L7RgiAwNS4wNy4yMDEyDDjQl9Cw0LrQu9GO0YfQtdC90LjQtSDihJYgMTQ5LzcvMS80LzItNjAzINC+0YIgMDYuMDcuMjAxMjAuBgUqhQNkbwQlDCPQn9CQ0JrQnCDCq9Ca0YDQuNC/0YLQvtCf0YDQviBIU03CuzBDBgNVHSAEPDA6MAgGBiqFA2RxATAIBgYqhQNkcQIwCAYGKoUDZHEDMAgGBiqFA2RxBDAIBgYqhQNkcQUwBgYEVR0gADAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUi5g7iRhR6O+cAni46sjUILJVyV0wCAYGKoUDAgIDA0EA23Reec/Y27rpMi+iFbgWCazGY3skBTq5ZGsQKOUxCe4mO7UBDACiWqdA0nvqiQMXeHgqo//fO9pxuIHtymwyMg==-----END CERTIFICATE-----";
            var CA2 = @"-----BEGIN CERTIFICATE-----MIIIBDCCB7OgAwIBAgIRBKgeQAWpGFyC5hHjxY8VRTcwCAYGKoUDAgIDMIIBITEaMBgGCCqFAwOBAwEBEgwwMDc3MTA0NzQzNzUxGDAWBgUqhQNkARINMTA0NzcwMjAyNjcwMTEeMBwGCSqGSIb3DQEJARYPZGl0QG1pbnN2eWF6LnJ1MTwwOgYDVQQJDDMxMjUzNzUg0LMuINCc0L7RgdC60LLQsCDRg9C7LiDQotCy0LXRgNGB0LrQsNGPINC0LjcxLDAqBgNVBAoMI9Cc0LjQvdC60L7QvNGB0LLRj9C30Ywg0KDQvtGB0YHQuNC4MRUwEwYDVQQHDAzQnNC+0YHQutCy0LAxHDAaBgNVBAgMEzc3INCzLiDQnNC+0YHQutCy0LAxCzAJBgNVBAYTAlJVMRswGQYDVQQDDBLQo9CmIDEg0JjQoSDQk9Cj0KYwHhcNMTYxMjE5MTE1NDEyWhcNMjYxMjA3MTA1MTExWjCCAUQxGDAWBgUqhQNkARINMTAyNzQwMTg2OTk5MDEaMBgGCCqFAwOBAwEBEgwwMDc0MzgwMTQ2NzMxCzAJBgNVBAYTAlJVMTEwLwYDVQQIDCg3NCDQp9C10LvRj9Cx0LjQvdGB0LrQsNGPINC+0LHQu9Cw0YHRgtGMMRswGQYDVQQHDBLQp9C10LvRj9Cx0LjQvdGB0LoxKjAoBgNVBAkMITMg0JjQvdGC0LXRgNC90LDRhtC40L7QvdCw0LvQsCA2MzEwMC4GA1UECwwn0KPQtNC+0YHRgtC+0LLQtdGA0Y/RjtGJ0LjQuSDRhtC10L3RgtGAMSUwIwYDVQQKDBzQntCe0J4g0JvQuNC90Lot0YHQtdGA0LLQuNGBMSowKAYDVQQDDCHQo9CmINCe0J7QniDQm9C40L3Qui3RgdC10YDQstC40YEwYzAcBgYqhQMCAhMwEgYHKoUDAgIjAQYHKoUDAgIeAQNDAARA8lD7vifqm3sYoi/qJzjGI+mPMus09B6KJC1UBgAnAvkqEJz9kZvd3p8abF6XpJ6+85lgbqeC71Oh3UPMWzKY1KOCBJswggSXMA4GA1UdDwEB/wQEAwIBhjAdBgNVHQ4EFgQUABoJR4JL3Z+CAqBNhbJ49Zi9rrUwEgYDVR0TAQH/BAgwBgEB/wIBADAlBgNVHSAEHjAcMAgGBiqFA2RxATAIBgYqhQNkcQIwBgYEVR0gADA2BgUqhQNkbwQtDCsi0JrRgNC40L/RgtC+0J/RgNC+IENTUCIgKNCy0LXRgNGB0LjRjyA0LjApMBAGCSsGAQQBgjcVAQQDAgEAMIIBhgYDVR0jBIIBfTCCAXmAFJ/Cc1iodIFqYG0z9EekaKZ2OWeboYIBUqSCAU4wggFKMR4wHAYJKoZIhvcNAQkBFg9kaXRAbWluc3Z5YXoucnUxCzAJBgNVBAYTAlJVMRwwGgYDVQQIDBM3NyDQsy4g0JzQvtGB0LrQstCwMRUwEwYDVQQHDAzQnNC+0YHQutCy0LAxPzA9BgNVBAkMNjEyNTM3NSDQsy4g0JzQvtGB0LrQstCwLCDRg9C7LiDQotCy0LXRgNGB0LrQsNGPLCDQtC4gNzEsMCoGA1UECgwj0JzQuNC90LrQvtC80YHQstGP0LfRjCDQoNC+0YHRgdC40LgxGDAWBgUqhQNkARINMTA0NzcwMjAyNjcwMTEaMBgGCCqFAwOBAwEBEgwwMDc3MTA0NzQzNzUxQTA/BgNVBAMMONCT0L7Qu9C+0LLQvdC+0Lkg0YPQtNC+0YHRgtC+0LLQtdGA0Y/RjtGJ0LjQuSDRhtC10L3RgtGAggsAzOBFZAAAAAAAkjAfBgkrBgEEAYI3FQcEEjAQBggqhQMCAi4AAQIBAQIBADArBgNVHRAEJDAigA8yMDE2MTIxOTExNTQxMVqBDzIwMjYxMjE5MTE1NDExWjCCATAGBSqFA2RwBIIBJTCCASEMKyLQmtGA0LjQv9GC0L7Qn9GA0L4gQ1NQIiAo0LLQtdGA0YHQuNGPIDMuOSkMLCLQmtGA0LjQv9GC0L7Qn9GA0L4g0KPQpiIgKNCy0LXRgNGB0LjQuCAyLjApDF/QodC10YDRgtC40YTQuNC60LDRgiDRgdC+0L7RgtCy0LXRgtGB0YLQstC40Y8g0KTQodCRINCg0L7RgdGB0LjQuCDQodCkLzEyNC0yNTQwINC+0YIgMTUuMDEuMjAxNQxj0KHQtdGA0YLQuNGE0LjQutCw0YIg0YHQvtC+0YLQstC10YLRgdGC0LLQuNGPINCk0KHQkSDQoNC+0YHRgdC40Lgg4oSWINCh0KQvMTI4LTI4ODEg0L7RgiAxMi4wNC4yMDE2MGEGA1UdHwRaMFgwKqAooCaGJGh0dHA6Ly9yb3N0ZWxlY29tLnJ1L2NkcC92Z3VjMV81LmNybDAqoCigJoYkaHR0cDovL3JlZXN0ci1wa2kucnUvY2RwL3ZndWMxXzUuY3JsMHIGCCsGAQUFBwEBBGYwZDAwBggrBgEFBQcwAoYkaHR0cDovL3Jvc3RlbGVjb20ucnUvY2RwL3ZndWMxXzUuY3J0MDAGCCsGAQUFBzAChiRodHRwOi8vcmVlc3RyLXBraS5ydS9jZHAvdmd1YzFfNS5jcnQwCAYGKoUDAgIDA0EAwVGjm+AhdxEPMFK5+E+j/Cbz1W1h/X8kH0OX8rwtftFjs1WxMcy9L3l+ksDhBG8Ry4wX8OCpBfxgPNijzwIogQ==-----END CERTIFICATE-----";
            var CA3 = @"-----BEGIN CERTIFICATE-----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-----END CERTIFICATE-----";
            var CA4 = @"-----BEGIN CERTIFICATE-----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-----END CERTIFICATE-----";
            var CA5 = @"-----BEGIN CERTIFICATE-----MIIFFDCCBMGgAwIBAgIQTm1HiybyfWV/do4CXOPTkzAKBggqhQMHAQEDAjCCASQxHjAcBgkqhkiG9w0BCQEWD2RpdEBtaW5zdnlhei5ydTELMAkGA1UEBhMCUlUxGDAWBgNVBAgMDzc3INCc0L7RgdC60LLQsDEZMBcGA1UEBwwQ0LMuINCc0L7RgdC60LLQsDEuMCwGA1UECQwl0YPQu9C40YbQsCDQotCy0LXRgNGB0LrQsNGPLCDQtNC+0LwgNzEsMCoGA1UECgwj0JzQuNC90LrQvtC80YHQstGP0LfRjCDQoNC+0YHRgdC40LgxGDAWBgUqhQNkARINMTA0NzcwMjAyNjcwMTEaMBgGCCqFAwOBAwEBEgwwMDc3MTA0NzQzNzUxLDAqBgNVBAMMI9Cc0LjQvdC60L7QvNGB0LLRj9C30Ywg0KDQvtGB0YHQuNC4MB4XDTE4MDcwNjEyMTgwNloXDTM2MDcwMTEyMTgwNlowggEkMR4wHAYJKoZIhvcNAQkBFg9kaXRAbWluc3Z5YXoucnUxCzAJBgNVBAYTAlJVMRgwFgYDVQQIDA83NyDQnNC+0YHQutCy0LAxGTAXBgNVBAcMENCzLiDQnNC+0YHQutCy0LAxLjAsBgNVBAkMJdGD0LvQuNGG0LAg0KLQstC10YDRgdC60LDRjywg0LTQvtC8IDcxLDAqBgNVBAoMI9Cc0LjQvdC60L7QvNGB0LLRj9C30Ywg0KDQvtGB0YHQuNC4MRgwFgYFKoUDZAESDTEwNDc3MDIwMjY3MDExGjAYBggqhQMDgQMBARIMMDA3NzEwNDc0Mzc1MSwwKgYDVQQDDCPQnNC40L3QutC+0LzRgdCy0Y/Qt9GMINCg0L7RgdGB0LjQuDBmMB8GCCqFAwcBAQEBMBMGByqFAwICIwEGCCqFAwcBAQICA0MABEB1OSpFp7milX33EP0ikge6HbZacYp9fVj8sUa5RWFXrB27SKX5SvtIGepqKev69RSYeHHKR+jT9YX2NuSK9wONo4IBwjCCAb4wgfUGBSqFA2RwBIHrMIHoDDTQn9CQ0JrQnCDCq9Ca0YDQuNC/0YLQvtCf0YDQviBIU03CuyDQstC10YDRgdC40LggMi4wDEPQn9CQ0JogwqvQk9C+0LvQvtCy0L3QvtC5INGD0LTQvtGB0YLQvtCy0LXRgNGP0Y7RidC40Lkg0YbQtdC90YLRgMK7DDXQl9Cw0LrQu9GO0YfQtdC90LjQtSDihJYgMTQ5LzMvMi8yLzIzINC+0YIgMDIuMDMuMjAxOAw00JfQsNC60LvRjtGH0LXQvdC40LUg4oSWIDE0OS83LzYvMTA1INC+0YIgMjcuMDYuMjAxODA/BgUqhQNkbwQ2DDTQn9CQ0JrQnCDCq9Ca0YDQuNC/0YLQvtCf0YDQviBIU03CuyDQstC10YDRgdC40LggMi4wMEMGA1UdIAQ8MDowCAYGKoUDZHEBMAgGBiqFA2RxAjAIBgYqhQNkcQMwCAYGKoUDZHEEMAgGBiqFA2RxBTAGBgRVHSAAMA4GA1UdDwEB/wQEAwIBBjAPBgNVHRMBAf8EBTADAQH/MB0GA1UdDgQWBBTCVPG0a9RMt+BtNrQjkPH+wzybBjAKBggqhQMHAQEDAgNBAJr6/eI7rHL7+FsQnoH2i6DVxqalbIxLKj05edpZGPLLb6B2PTAMya7pSt9hb8QnFABgsR4IE5gT4VVkDWbX/n4=-----END CERTIFICATE-----";
            CAList.Add(CA5); CAList.Add(CA4); CAList.Add(CA3); CAList.Add(CA2); CAList.Add(CA1);

            ProgBar_AutoInstall.Maximum = 100;


            if (CSPFound())
            {
                ThCertInstall = new Thread(CertInstall);
                ThCertInstall.Start();
            }
            else
            {
                MessageBox.Show(null, "КриптоПРО CSP не установлен или лицензия не активна, обратитесь к инструкции или в службу технической поддержки.", "КриптоПРО CSP (error -2147467259)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        public bool CAInstall = false;
        public void InstallCertificateCA()
        {
            try
            {
                foreach (string cacert in CAList)
                {
                    var certBytes = Encoding.UTF8.GetBytes(cacert);
                    var signingcert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes, "", System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);

                    X509Certificate2 certificate = new X509Certificate2(signingcert);
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);

                    foreach (X509Certificate2 ca in store.Certificates)
                    {
                        if (ca.Thumbprint == certificate.Thumbprint)
                            CAInstall = true;
                    }

                    if (!CAInstall)
                    {
                        Invoke(new Action(() => label1.Text = "Устанавливаются корневые сертификаты ..."));

                        store.Add(certificate);
                        cainstallcount++;
                    }

                    CAInstall = false;

                    store.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, "Ошибка при установке корневого сертификата: " + ex.ToString(), "Произошла ошибка.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool X509CertificateInstall = false;
        public void InstallCertificate(X509Certificate2 cerFileName)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            foreach (X509Certificate2 ccert in store.Certificates)
            {
                if (ccert.Thumbprint == cerFileName.Thumbprint)
                {
                    X509CertificateInstall = true;
                    installccount++;
                }
            }

            if (!X509CertificateInstall)
            {
                Invoke(new Action(() => label2.Text = "Выполняется поиск сертификатов в контейнерах ... "));
                store.Add(cerFileName);
                installcount++;
            }

            X509CertificateInstall = false;
            store.Close();
        }

        public void CertInstall()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

        Repeat:

            Get_ContainerNames();

            InstallCertificateCA();

            this.Hide();

            if (installcount > 0)
            {
                if (cainstallcount == 0)
                    MessageBox.Show(null, "Действующие сертификаты были успешно установлены.\r\rУстановлено личных сертификатов: (" + installcount + ")", "Установка сертификатов успешно завершена.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show(null, "Действующие сертификаты были успешно установлены.\r\rУстановлено личных сертификатов: (" + installcount + ")\rУстановлено корневых сертификатов: (" + cainstallcount + ")", "Установка сертификатов успешно завершена.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
            }
            else if (installcount == 0 && installccount > 0)
            {
                if (cainstallcount == 0)
                    MessageBox.Show(null, "Не найдено личных сертификатов для установки.\rВозможно личные сертификаты были установлены ранее.", "Сертификаты уже установлены.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show(null, "Не найдено личных сертификатов для установки.\rВозможно личные сертификаты были установлены ранее.\r\rУстановлено корневых сертификатов: (" + cainstallcount + ")", "Сертификаты уже установлены.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                System.Windows.Forms.DialogResult dialogResult;

                if (cainstallcount == 0)
                    dialogResult = MessageBox.Show(null, "Не найдено действующих личных сертификатов для установки." + Environment.NewLine + Environment.NewLine + "Если сертифкаты находятся на внешнем устройстве: (токен\\флеш-карта) убедитесь, что устройство подключено и повторите попытку.", "Не найдено действующих сертифиатов.", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);
                else
                    dialogResult = MessageBox.Show(null, "Не найдено действующих личных сертификатов для установки.\r\rУстановлено корневых сертификатов: (" + cainstallcount + ")" + Environment.NewLine + Environment.NewLine + "Если сертифкаты находятся на внешнем устройстве: (токен\\флеш-карта) убедитесь, что устройство подключено и повторите попытку.", "Не найдено действующих сертифиатов.", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly);

                if (dialogResult == DialogResult.Retry)
                    goto Repeat;
                else
                    if (dialogResult == DialogResult.Cancel)
                        Environment.Exit(0);
            }

            Environment.Exit(0);
        }

        public static string CSPPath;

        public static string ProgramFiles()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        public static bool CSPFound()
        {
            CSPPath = ProgramFiles() + "\\Crypto Pro\\CSP\\";
            if (!String.IsNullOrEmpty(CSPPath))
                if (System.IO.File.Exists(CSPPath + "csptest.exe"))
                    return true;
                else return false;
            else return false;
        }

        public static string CSPVersion()
        {
            FileVersionInfo myFileVersionInfo = null;
            if (!String.IsNullOrEmpty(CSPPath))
                if (System.IO.File.Exists(CSPPath + "csptest.exe"))
                    myFileVersionInfo = FileVersionInfo.GetVersionInfo(CSPPath + "csptest.exe");
            return myFileVersionInfo.ProductVersion;
        }

        public static CspParameters cspParameters = new CspParameters(75);
        public static Gost3410_2012_256CryptoServiceProvider CryptoServiceProvider2012;
        public static Gost3410CryptoServiceProvider CryptoServiceProvider2001;
        public void Get_ContainerNames()
        {
            uint pcbData = 0;
            uint dwFlags = CRYPT_FIRST;
            IntPtr hProv = IntPtr.Zero;
            bool gotcsp = CryptAcquireContext(ref hProv, null, "Crypto-Pro GOST R 34.10-2001 Cryptographic Service Provider", PROV_RSA_FULL, CRYPT_VERIFYCONTEXT | CSPKEYTYPE);
            StringBuilder sb = null;
            CryptGetProvParam(hProv, PP_ENUMCONTAINERS, sb, ref pcbData, dwFlags);
            sb = new StringBuilder((int)(2 * pcbData));
            dwFlags = CRYPT_FIRST;

            DateTime dt = DateTime.Now;
            string curDate = dt.ToShortDateString();

            X509Certificate2 CurrentCert;
            String[] CertStorage;

            while (CryptGetProvParam(hProv, PP_ENUMCONTAINERS, sb, ref pcbData, dwFlags))
            {
                dwFlags = 0;
                cspParameters.KeyContainerName = sb.ToString();
                cspParameters.Flags = CspProviderFlags.NoPrompt;

                try
                {
                    CryptoServiceProvider2001 = new Gost3410CryptoServiceProvider(cspParameters);
                    CurrentCert = CryptoServiceProvider2001.ContainerCertificate;
                    CertStorage = CryptoServiceProvider2001.CspKeyContainerInfo.UniqueKeyContainerName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                }
                catch (Exception Ex)
                {
                    if (Environment.OSVersion.Version.Major < 6)
                        return;

                    if (Convert.ToInt32(CSPVersion()[0].ToString()) < 4)
                        return;
                    
                    CryptoServiceProvider2012 = new Gost3410_2012_256CryptoServiceProvider(cspParameters);
                    CurrentCert = CryptoServiceProvider2012.ContainerCertificate;
                    CertStorage = CryptoServiceProvider2012.CspKeyContainerInfo.UniqueKeyContainerName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                }

                try
                {
                    if (CurrentCert != null)
                    {
                        Invoke(new Action(() => label1.Text = "Поиск в контейнере: " + CertStorage[1]));

                        if (CurrentCert.NotAfter < DateTime.Now)
                        {
                            Invoke(new Action(() => label1.Text = "Истек срок действия: " + CurrentCert.SerialNumber + " (" + CurrentCert.NotAfter + ")"));
                        }
                        else
                        {
                            string GetNameInfo = CurrentCert.GetNameInfo(X509NameType.SimpleName, true);

                            if (GetNameInfo.Contains("Link-Service") || GetNameInfo.Contains("Линк-сервис") || CertStorage[0].Contains("LINK"))
                            {
                                X509Store CertificationStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                                CertificationStore.Open(OpenFlags.ReadWrite);

                                X509CertificateParser parser = new X509CertificateParser();

                                byte[] rawdata = CurrentCert.RawData;

                                Org.BouncyCastle.X509.X509Certificate cert = parser.ReadCertificate(rawdata);
                                DerSequence subject = cert.SubjectDN.ToAsn1Object() as DerSequence;

                                foreach (Asn1Encodable setItem in subject)
                                {
                                    DerSet subSet = setItem as DerSet;
                                    if (subSet == null) continue;
                                    DerSequence subSeq = subSet[0] as DerSequence;

                                    foreach (Asn1Encodable subSeqItem in subSeq)
                                    {
                                        DerObjectIdentifier oid = subSeqItem as DerObjectIdentifier;
                                        if (oid == null) continue;
                                        string value = subSeq[1].ToString();
                                        if (oid.Id.Equals("2.5.4.3")) CN = value;
                                    }
                                }

                                if (!CertStorage[0].Contains("ePass")) // ePass break; Equals
                                    if (CertStorage[0] != "REGISTRY")
                                        InstallCertificate(CurrentCert);
                                    else
                                        InstallCertificate(CurrentCert);

                                CertificationStore.Close();


                                // CryptoServiceProvider.Clear();
                            }
                            else
                                Invoke(new Action(() => label1.Text = "Сертификат: " + CurrentCert.SerialNumber + " (недоступен)"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //LogWrite(ex);
                }
            }

            if (hProv != IntPtr.Zero)
                CryptReleaseContext(hProv, 0);
        }

        const uint PROV_RSA_FULL = 0x00000001;
        const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        static uint CSPKEYTYPE = 0;
        const uint PP_ENUMCONTAINERS = 0x00000002;
        const uint CRYPT_FIRST = 0x00000001;

        public static void showWin32Error(int errorcode)
        {
            Win32Exception myEx = new Win32Exception(errorcode);

            if (myEx.ErrorCode == -2147467259)
            {
                MessageBox.Show(null, "КриптоПРО CSP не установлен или лицензия не активна, обратитесь к инструкции или в службу технической поддержки.", "КриптоПРО CSP (error -2147467259)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            System.IO.File.AppendAllText(DateTime.Today.ToShortDateString() + "_ApplicationError.log", "Error: " + myEx.ErrorCode + Environment.NewLine + "Error message: " + myEx.Message, Encoding.Default);
        }
    }
}
