using System;

namespace BankAccount.Models{
    public class Transaction
    {
        public int TransactionId{get; set;}
        public decimal Amount{get;set;}

        public DateTime CreatedAt {get;set;}=DateTime.Now;
        public DateTime UpdatedAt {get;set;}=DateTime.Now;

        public int UserId{get; set;}

        public User BankUser{get; set;}



    }
}