﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineExam.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OnlineExamEntities4 : DbContext
    {
        public OnlineExamEntities4()
            : base("name=OnlineExamEntities4")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionCategory> QuestionCategories { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestXPaper> TestXPapers { get; set; }
        public DbSet<TestXQuestion> TestXQuestions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}
