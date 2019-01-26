using MailKit;
using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ImapCheker
{
    class ImapCheker
    {
        public string[] mailData { get; private set; }
        public ImapClient client;
        public int maxErrors;

        public ImapCheker(string data, int errorLimit = 50)
        {
            maxErrors = errorLimit;
            mailData = data.Split(':');
            client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        public bool ValidateMail()
        {
            Console.WriteLine("Подключение к почтовому сервису");
            try
            {
                if (mailData[0].Contains("mail.ru"))
                {
                    client.Connect("imap.mail.ru", 993, true);
                }
                else if (mailData[0].Contains("hotmail.com"))
                {
                    client.Connect("imap-mail.outlook.com", 993, true);
                }
                else if (mailData[0].Contains("yahoo.com"))
                {
                    client.Connect("imap.mail.yahoo.com", 993, true);
                }
                else if (mailData[0].Contains("yandex.ru"))
                {
                    client.Connect("imap.yandex.ru", 993, true);
                }
                else if (mailData[0].Contains("gmail.com"))
                {
                    client.Connect("imap.gmail.com", 993, true);
                }
            }
            catch
            {
                return false;
            }

            try
            {
                Console.WriteLine("Попытка авторизации!");
                client.Authenticate(mailData[0], mailData[1]);
                Console.WriteLine("Почта успешно авторизовалась!");
                var personal = client.GetFolder(client.PersonalNamespaces[0]);
                //foreach (var folder in personal.GetSubfolders(false))
                //    Console.WriteLine("[folder] {0}", folder.Name);
                //Console.ReadLine();
            }
            catch
            {
                Console.WriteLine("Невозможно авторизоваться!");
                return false;
            }

            try
            {
                Console.WriteLine("Попытка получить список сообщений!");
                var mail = client.GetFolder("inbox");
                mail.Open(FolderAccess.ReadOnly);
                Console.WriteLine("Список сообщений получен успешно!");
            }
            catch
            {
                Console.WriteLine("Невозможно получить список сообщений!");
                return false;
            }
            return true;
        }

        public string FindCode(string codeRegex, string mailFrom)
        {
            IMailFolder mail;

            var regex = new Regex(codeRegex);
            var link = "";
            var flag = true;
            var errors = 0;
            while (true)
            {
                if (flag)
                {
                    flag = false;
                    mail = client.GetFolder("inbox");
                    mail.Open(FolderAccess.ReadOnly);
                    Console.WriteLine("Поиск письма во входящих");
                }
                else
                {
                    try
                    {
                        flag = true;
                        if (mailData[0].Contains("hotmail.com"))
                        {
                            mail = client.GetFolder("Junk");
                        }
                        else if (mailData[0].Contains("yahoo.com"))
                        {
                            mail = client.GetFolder("Bulk Mail");
                        }
                        else
                        {
                            mail = client.GetFolder(SpecialFolder.Junk);
                        }
                        mail.Open(FolderAccess.ReadOnly);
                    }
                    catch
                    {
                        mail = null;
                    }
                    Console.WriteLine("Поиск письма в спам");
                }
                for (int i = 0; i < mail.Count; i++)
                {
                    var msg = mail.GetMessage(i);
                    if (msg.From.ToString().Contains(mailFrom))
                    {
                        Console.WriteLine("Письмо найдено!");
                        Match match;
                        try
                        {
                            match = regex.Match(WebUtility.HtmlDecode(msg.HtmlBody));
                            link = match.Groups[0].ToString();
                            Console.WriteLine("Ссылка успешно разобрана!");
                            Console.WriteLine("Ссылка: " + link);
                            try
                            {
                                client.Disconnect(true);
                            }
                            catch { }
                            return link;
                        }
                        catch
                        {
                            Console.WriteLine("Не удалось найти ссылку!");
                        }
                    }
                }
                errors++;
                if (errors > maxErrors)
                {
                    Console.WriteLine("Ничего не пришло!");
                    return "";
                }
                Thread.Sleep(2000);
            }
        }
    }
}
