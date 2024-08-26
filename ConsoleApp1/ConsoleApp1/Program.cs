using System;
using System;
using System.Drawing.Printing;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using Org.BouncyCastle.Tls;

namespace ConsoleApp1
{
    internal class Program
    {
        private static string _connString;
        private static MySqlConnection _connection;
        private static string _userInputUsername;
        static void Main(string[] args)
        {
            ConnectSQL();
            Console.Write("Velkommen!\nVælg venligst en af følgende muligheder:\n\n1. Log ind\n2. Opret profil\n\nSkriv her: ");
            try
            {
                int selectedOption = Convert.ToInt32(Console.ReadLine());

                switch (selectedOption)
                {
                    case 1:
                        LogIn();
                        break;
                    case 2:
                        SignUp();
                        break;
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Ugyldigt input. Tryk Enter for at lukke programmet.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            ShowDashboard();
        }

        public static void LogIn()
        {
            Console.Clear();
            Console.Write("Indtast venligst dit brugernavn.\n\nSkriv her: ");
            _userInputUsername = Console.ReadLine();
            CheckUser(_userInputUsername);
        }

        public static void SignUp()
        {
            Console.Write("Du er nu ved at oprette en konto.\n\nVælg brugernavn: ");
            string userNameSignUp = Console.ReadLine();
            Console.Write("Vælg adgangskode: ");
            string passWordSignUp = Console.ReadLine();
            string query = "INSERT INTO users (username, password) VALUES (@username, @password)";
            var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@username", userNameSignUp);
            cmd.Parameters.AddWithValue("@password", passWordSignUp);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Konto blev oprettet! Log venligst ind ved at trykke Enter.");
            Console.ReadKey();
            LogIn();
        }

        public static void ShowDashboard()
        {
            Console.Clear();
            Console.Write($"Velkommen, {_userInputUsername}.\n\nDin nuværende saldo er: {CheckBalance(_userInputUsername)}\n\nVælg venligst en af følgende funktioner:\n\n1. Indsæt penge\n2. Hæv penge\n\nSkriv her: ");
            try
            {
                int chosenOption = Convert.ToInt32(Console.ReadLine());

                switch(chosenOption)
                {
                    case 1:
                        DepositMoney();
                        break;
                    case 2:
                        WithdrawMoney();
                        break;
                    default:
                        Console.Write("Ugyldigt input! Tryk Enter for at prøve igen.");
                        ShowDashboard();
                        break;
                }
            } catch
            {
                Console.Write("Ugyldigt input! Tryk Enter for at prøve igen.");
                Console.ReadKey();
                ShowDashboard();
            }
        }

        public static void DepositMoney()
        {
            Console.Clear();
            Console.Write("Hvor mange penge ønsker du at indsætte\n\nSkriv her: ");
            try
            {
                decimal depositAmount = Convert.ToDecimal(Console.ReadLine());
                ChangeBalance(_userInputUsername, (decimal)depositAmount);
            } catch 
            {
                Console.Write("Ugyldigt Input! Tryk Enter for at prøve igen.");
                Console.ReadKey();
                DepositMoney();
            }

            Console.Clear();
            Console.Write($"Din saldo er nu: {CheckBalance(_userInputUsername)}\n\nTryk Enter for at vende tilbage til dit Dashboard.");
            Console.ReadKey();
            ShowDashboard();
        }
        
        public static void WithdrawMoney()
        {
            Console.Clear();
            Console.Write("Hvor mange penge ønsker du at hæve?\n\nSkriv her: ");
            try
            {
                decimal depositAmount = Convert.ToDecimal(Console.ReadLine());
                ChangeBalance(_userInputUsername, (decimal)-depositAmount);
            }
            catch
            {
                Console.Write("Ugyldigt Input! Tryk Enter for at prøve igen.");
                Console.ReadKey();
                DepositMoney();
            }

            Console.Clear();
            Console.Write($"Din saldo er nu: {CheckBalance(_userInputUsername)}\n\nTryk Enter for at vende tilbage til dit Dashboard.");
            Console.ReadKey();
            ShowDashboard();
        }

        public static void ConnectSQL()
        {
            string server = "127.0.0.1";
            string databaseName = "banking";
            string userName = "root";
            string password = "";
            _connString = string.Format("Server={0}; database={1}; UID={2}; password={3}", server, databaseName, userName, password);

            _connection = new MySqlConnection(_connString);
            _connection.Open();
        }

        public static void CheckUser(string _userInputUsername)
        {
            try
            {
                string query = "SELECT password FROM users WHERE username = @username";
                var cmd = new MySqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@username", _userInputUsername);

                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    Console.WriteLine($"Brugernavnet '{_userInputUsername}' fíndes ikke i databasen. Tryk Enter for lukke programmet.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                reader.Read();
                string password = reader.GetString(0);
                reader.Close();

                Console.Clear();
                Console.Write("Indtast venligst din adgangskode.\n\nSkriv her: ");
                string userInputPassword = Console.ReadLine();

                if (userInputPassword == password)
                {

                    Console.Clear();
                } else
                {
                    throw new Exception("Adgangskoden er forkert!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FEJL! Kunne ikke forbinde til database. Fejlkode: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public static string CheckBalance(string username)
        {
            decimal balance = GetBalanceFromDatabase(username);
            return $"{balance:C}";
        }

        public static void ChangeBalance(string username, decimal amount)
        {
            decimal currentBalance = GetBalanceFromDatabase(username);
            decimal newBalance = currentBalance + amount;
            if (newBalance >= 0)
            {
                UpdateBalanceInDatabase(username, newBalance);
            } else
            {
                Console.Write("\nDu har ikke nok penge! Tryk Enter for at vende tilbage til dit dashboard.");
                Console.ReadKey();
                WithdrawMoney();
            }
        }

        private static decimal GetBalanceFromDatabase(string username)
        {
            decimal balance = 0;
            string query = "SELECT username, balance FROM users WHERE username = @username";
            using (var cmd = new MySqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        balance = reader.GetDecimal(1);
                    }
                    else
                    {
                        Console.WriteLine($"Brugeren '{username}' blev ikke fundet i databasen. Tryk Enter for at lukke programmet");
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                }
            }
            return balance;
        }

        private static void UpdateBalanceInDatabase(string username, decimal newBalance)
        {
            string query = "UPDATE users SET balance = @balance WHERE username = @username";
            using (var cmd = new MySqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.ExecuteNonQuery();
            }
        }
    }
}