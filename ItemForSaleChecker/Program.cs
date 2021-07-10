using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

/// <summary>
/// Author: Daniel Aguayo
/// Date: 08/28/2017
/// </summary>
namespace ItemForSaleChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type: \"help\" for assistance.");
            Console.WriteLine("");

            bool isEmailSent = false;
            string html = String.Empty;

            // Get URL
            string url = GetUrl("Please enter URL: ");

            // Get Seconds
            int seconds = GetSeconds("Please enter after how many seconds you would like to check for updates: ");

            // Get Email
            string toEmail = GetUserEmail("Please enter Email: ");

            // Get past "The request was aborted: Could not create SSL/TLS secure channel" error when using https
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            while (isEmailSent == false)
            {
                // Send HttpWebRequest to retrieve HTML
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add(HttpRequestHeader.Cookie, "MyCookie"); // Add cookie to trick Amazon/BestBuy into thinking it's a real user
                request.UserAgent = "Mozilla/5.0"; // Need this specifically for BestBuy
                request.Accept = "/"; // Need this specifically for BestBuy
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US"); // Need this specifically for BestBuy                

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream data = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(data))
                        {
                            html = sr.ReadToEnd();
                        }
                    }                    
                }                

                Console.WriteLine(String.Format("Successfully executed Request: {0} ", DateTime.Now.ToString()));

                // Check if item is available to buy
                if (html.Contains("id=\"cart-button\"") || html.Contains("id=\"add-to-cart-button\""))
                {
                    // Create client to send email
                    using (var client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("YourEmail@gmail.com", "YourPassword"),
                        EnableSsl = true
                    })
                    {
                        // Send email
                        client.Send("ItemForSaleChecker@noreply.com", toEmail, "Item For Sale Checker", "Your item is for sale.");
                    }
                    isEmailSent = true;

                    Console.WriteLine("Items is for sale! Email was sent.");
                }
                else
                {
                    // Wait X seconds
                    Thread.Sleep(seconds);
                }

                
            }

            Console.WriteLine("Press any key to exist.");
            Console.ReadLine();
        }

        /// <summary>
        /// GetUrl
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetUrl(string message)
        {
            Console.Write(message);
            string val = Console.ReadLine();

            while (Help(val))
            {
                Console.WriteLine("");
                Console.Write(message);
                val = Console.ReadLine();
            }

            if (!val.StartsWith("http://") && !val.StartsWith("https://"))
            {
                Console.WriteLine("Please enter a valid URL. Example: http://myurl.com/item/1");
                GetUrl(message);

            }

            return val;
        }

        /// <summary>
        /// GetUserEmail
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetUserEmail(string message)
        {
            Console.Write(message);
            string val = Console.ReadLine();

            while (Help(val))
            {
                Console.WriteLine("");
                Console.Write(message);
                val = Console.ReadLine();
            }

            if (!val.Contains("@"))
            {
                Console.WriteLine("Please enter a valid email. Example: myemail@gmail.com");
                GetUserEmail(message);
            }

            return val;
        }

        /// <summary>
        /// GetSeconds
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int GetSeconds(string message)
        {
            Console.Write(message);
            string val = Console.ReadLine();
            int seconds = 0;

            while (Help(val))
            {
                Console.WriteLine("");
                Console.Write(message);
                val = Console.ReadLine();
            }


            if (!int.TryParse(val, out seconds))
            {
                Console.WriteLine("Please enter a valid number. Example: 1");
                GetSeconds(message);
            }
            else // The seconds are correct
                seconds = seconds * 1000;


            return seconds;
        }

        /// <summary>
        /// Help
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool Help(string command)
        {
            bool isHelp = false;
            if (command.Equals("help"))
            {
                Console.WriteLine("");
                Console.WriteLine("-----------------------------------------------");
                Console.WriteLine("To send email via text message: ");
                Console.WriteLine("AT&T: number@txt.att.net");
                Console.WriteLine("T-Mobile: number@tmomail.net");
                Console.WriteLine("Verizon: number@vtext.com");
                Console.WriteLine("Sprint: number@messaging.sprintpcs.com");
                Console.WriteLine("Virgin Mobile: number@vmobl.com");
                Console.WriteLine("-----------------------------------------------");
                Console.Write("");
                isHelp = true;
            }
            return isHelp;
        }
    }
}
