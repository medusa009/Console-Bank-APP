using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BankApp
{
    public interface IAccount
    {
        string AccountNumber { get; }
        string AccountHolder { get; }
        decimal Balance { get; }
        void Deposit(decimal amount);
        bool Withdraw(decimal amount);
        string GetStatement();
    }

    public class SavingsAccount : IAccount
    {
        private const decimal MinimumBalance = 1000m;
        public string AccountNumber { get; private set; }
        public string AccountHolder { get; private set; }
        public decimal Balance { get; private set; }

        public SavingsAccount(string accountNumber, string accountHolder)
        {
            AccountNumber = accountNumber;
            AccountHolder = accountHolder;
            Balance = 0m;
        }

        public void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public bool Withdraw(decimal amount)
        {
            if (Balance - amount < MinimumBalance)
                return false;
            Balance -= amount;
            return true;
        }

        public string GetStatement()
        {
            return $"Savings Account - Number: {AccountNumber}, Holder: {AccountHolder}, Balance: {Balance:C}";
        }
    }

    public class CurrentAccount : IAccount
    {
        public string AccountNumber { get; private set; }
        public string AccountHolder { get; private set; }
        public decimal Balance { get; private set; }

        public CurrentAccount(string accountNumber, string accountHolder)
        {
            AccountNumber = accountNumber;
            AccountHolder = accountHolder;
            Balance = 0m;
        }

        public void Deposit(decimal amount)
        {
            Balance += amount;
        }

        public bool Withdraw(decimal amount)
        {
            if (Balance - amount < 0)
                return false;
            Balance -= amount;
            return true;
        }

        public string GetStatement()
        {
            return $"Current Account - Number: {AccountNumber}, Holder: {AccountHolder}, Balance: {Balance:C}";
        }
    }

    public class AccountFactory 
    {
        public static IAccount CreateAccount(string type, string accountNumber, string accountHolder)
        {
            switch (type.ToLower())
            {
                case "savings":
                    return new SavingsAccount(accountNumber, accountHolder);
                case "current":
                    return new CurrentAccount(accountNumber, accountHolder);
                default:
                    throw new ArgumentException("Invalid account type");
            }
        }
    }

    public class Bank
    {
        private readonly Dictionary<string, IAccount> _accounts = new Dictionary<string, IAccount>();
        private readonly Dictionary<string, string> _customers = new Dictionary<string, string>();

        public bool RegisterCustomer(string email, string password)
        {
            if (!IsValidEmail(email))
                return false;
            if (!IsValidPassword(password))
                return false;

            // Store customer info (for demo purposes)
            _customers[email] = password;
            return true;
        }

        public IAccount CreateAccount(string email, string accountType, string accountNumber)
        {
            if (!_customers.ContainsKey(email))
                throw new Exception("Customer not registered");

            var accountHolder = email;
            var account = AccountFactory.CreateAccount(accountType, accountNumber, accountHolder);
            _accounts[accountNumber] = account;
            return account;
        }

        public bool Deposit(string accountNumber, decimal amount)
        {
            if (_accounts.TryGetValue(accountNumber, out var account))
            {
                account.Deposit(amount);
                return true;
            }
            return false;
        }

        public bool Withdraw(string accountNumber, decimal amount)
        {
            if (_accounts.TryGetValue(accountNumber, out var account))
            {
                return account.Withdraw(amount);
            }
            return false;
        }

        public string GetStatement(string accountNumber)
        {
            if (_accounts.TryGetValue(accountNumber, out var account))
            {
                return account.GetStatement();
            }
            return "Account not found";
        }

        public bool Transfer(string fromAccountNumber, string toAccountNumber, decimal amount)
        {
            if (Withdraw(fromAccountNumber, amount))
            {
                Deposit(toAccountNumber, amount);
                return true;
            }
            return false;
        }

        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }

        private bool IsValidPassword(string password)
        {
            var hasUpperCase = new Regex(@"[A-Z]+");
            var hasLowerCase = new Regex(@"[a-z]+");
            var hasDigit = new Regex(@"[0-9]+");
            var hasSpecialChar = new Regex(@"[@#$%^&!]+");
            var minLength = password.Length >= 6;
            return minLength && hasUpperCase.IsMatch(password) && hasLowerCase.IsMatch(password) && hasDigit.IsMatch(password) && hasSpecialChar.IsMatch(password);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var bank = new Bank();
            
            // Example usage
            if (bank.RegisterCustomer("JohnDoe@example.com", "Password@123"))
            {
                var savingsAccount = bank.CreateAccount("JohnDoe@example.com", "savings", "1234567890");
                bank.Deposit("1234567890", 5000m);
                bank.Withdraw("1234567890", 2000m);
                Console.WriteLine(bank.GetStatement("1234567890"));

                var currentAccount = bank.CreateAccount("JohnDoe@example.com", "current", "0987654321");
                bank.Deposit("0987654321", 3000m);
                bank.Transfer("0987654321", "1234567890", 1000m);
                Console.WriteLine(bank.GetStatement("0987654321"));
                Console.WriteLine(bank.GetStatement("1234567890"));
            }
            else
            {
                Console.WriteLine("Invalid customer registration details.");
            }
        }
    }
}
