namespace LMS.Migrations
{
    using FizzWare.NBuilder;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Web;

    internal sealed class Configuration : DbMigrationsConfiguration<LMS.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            try
            {
                if (!context.Roles.Any(r => r.Name == "Manager"))
                {
                    var store = new RoleStore<IdentityRole>(context);
                    var manager = new RoleManager<IdentityRole>(store);
                    var mgr = new IdentityRole { Name = "Manager" };
                    manager.Create(mgr);
                    var assistant = new IdentityRole { Name = "Assistant" };
                    manager.Create(assistant);
                }

                if (!context.Users.Any(u => u.UserName == "manager@example.com"))
                {
                    var store = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(store);

                    var manager = new ApplicationUser { FullName="Manager", UserName = "manager@example.com", Email= "manager@example.com" };
                    userManager.Create(manager, "Password@123");
                    userManager.AddToRole(manager.Id, "Manager");

                    var assistant = new ApplicationUser { FullName = "Assistant", UserName = "assistant@example.com", Email = "assistant@example.com" };
                    userManager.Create(assistant, "Password@123");
                    userManager.AddToRole(assistant.Id, "Assistant");
                }
                context.SaveChanges();

                var randomNumber = new RandomGenerator();

                var publishers = Builder<Publisher>.CreateListOfSize(10)
                    .All()
                    .With(p => p.Name = Faker.Company.Name())
                    .With(p => p.Location = Faker.Address.UkPostCode())
                    .Build();

                context.Publishers.AddOrUpdate(publishers.ToArray());
                context.SaveChanges();

                var authors = Builder<Author>.CreateListOfSize(20)
                    .All()
                    .With(a => a.FirstName = Faker.Name.First())
                    .With(a => a.MiddleName = "")
                    .With(a => a.LastName = Faker.Name.Last())
                    .Build();

                context.Authors.AddOrUpdate(authors.ToArray());
                context.SaveChanges();

                var press = Builder<Press>.CreateListOfSize(5)
                    .All()
                    .With(p => p.Name = Faker.Company.Name())
                    .Build();

                context.Press.AddOrUpdate(press.ToArray());
                context.SaveChanges();

                var categories = new Category[]
                {
                    new Category { Name = "Adventure", AgeRestricted = false},
                    new Category { Name = "Animals", AgeRestricted = false },
                    new Category { Name = "Art and Music", AgeRestricted = false },
                    new Category { Name = "Biographies", AgeRestricted = false },
                    new Category { Name = "Character", AgeRestricted = false },
                    new Category { Name = "Classics", AgeRestricted = false },
                    new Category { Name = "Folklore, Fairytales and Mythology", AgeRestricted = false },
                    new Category { Name = "General Fiction", AgeRestricted = false },
                    new Category { Name = "Graphic Novels", AgeRestricted = false },
                    new Category { Name = "Health and Human Body", AgeRestricted = false },
                    new Category { Name = "History", AgeRestricted = false },
                    new Category { Name = "Language Arts", AgeRestricted = false },
                    new Category { Name = "Math", AgeRestricted = false },
                    new Category { Name = "Other", AgeRestricted = false },
                    new Category { Name = "Reference", AgeRestricted = false },
                    new Category { Name = "Romance", AgeRestricted = true},
                    new Category { Name = "Sci-Fi and Fantasy", AgeRestricted = false },
                    new Category { Name = "Science", AgeRestricted = false },
                    new Category { Name = "Social Studies", AgeRestricted = false },
                    new Category { Name = "Sports and Recreation", AgeRestricted = false },
                    new Category { Name = "Supernatural", AgeRestricted = true },
                    new Category { Name = "Technology and Transportation", AgeRestricted = false }
                };

                context.Categories.AddOrUpdate(categories);

                context.SaveChanges();

                var books = Builder<Book>.CreateListOfSize(25)
                    .All()
                    .With(b => b.ISBN = Faker.RandomNumber.Next().ToString())
                    .With(b => b.Name = string.Join(" ", Faker.Lorem.Words(5)))
                    .With(b => b.Publisher = Pick<Publisher>.RandomItemFrom(publishers))
                    .With(b => b.Press = Pick<Press>.RandomItemFrom(press))
                    .With(b => b.PublishedDate = DateTime.Now.AddYears(-randomNumber.Next(1, 10)))
                    .With(b => b.Charge = randomNumber.Next(1, 10))
                    .With(b => b.PenaltyCharge = randomNumber.Next(2, 20))
                    .With(b => b.Authors = Pick<Author>.UniqueRandomList(With.Between(1).And(5).Elements).From(authors))
                    .With(b => b.Categories = Pick<Category>.UniqueRandomList(With.Between(1).And(3).Elements).From(categories))
                    .Build();

                context.Books.AddOrUpdate(books.ToArray());
                context.SaveChanges();


                foreach (var book in books.ToList())
                {
                    var bookCopies = Builder<BookCopy>.CreateListOfSize(randomNumber.Next(3, 10))
                        .All()
                        .With(b => b.Book = book)
                        .With(b => b.Location = "Rack #" + randomNumber.Next(1, 100))
                        .With(b => b.AddedOn = DateTime.Now.AddMonths(-randomNumber.Next(1, 90)))
                        .Build();

                    context.BookCopies.AddOrUpdate(bookCopies.ToArray());
                }
                context.SaveChanges();

                var memberships = new Membership[] {
                    new Membership { Name = "Normal", MaxLoans = 5 },
                    new Membership { Name = "Premium", MaxLoans = 10 }
                };

                context.Memberships.AddOrUpdate(memberships);

                context.SaveChanges();

                var members = Builder<Member>.CreateListOfSize(100)
                    .All()
                    .With(a => a.FirstName = Faker.Name.First())
                    .With(a => a.MiddleName = "")
                    .With(a => a.LastName = Faker.Name.Last())
                    .With(a => a.DateOfBirth = DateTime.Now.AddYears(-randomNumber.Next(15, 40)))
                    .With(a => a.Membership = Pick<Membership>.RandomItemFrom(memberships))
                    .With(a => a.Address = Faker.Address.StreetAddress())
                    .With(a => a.PhoneNumber = Faker.Phone.Number())
                    .Build();

                context.Members.AddOrUpdate(members.ToArray());
                context.SaveChanges();

                var loanTypes = new LoanType[] {
                    new LoanType { Name="Weekly", Duration=7 },
                    new LoanType { Name="Fortnightly", Duration=15 },
                    new LoanType { Name="Monthly", Duration=30 }

                };

                context.LoanTypes.AddOrUpdate(loanTypes);
                context.SaveChanges();

                List<Loan> loans = new List<Loan>();

                for(int i = 1; i <= 200; i++)
                {
                    var book = Pick<Book>.RandomItemFrom(books);
                    var bookCopy = Pick<BookCopy>.RandomItemFrom(book.Copies.ToList());
                    if (bookCopy.Available)
                    {
                        var loanType = Pick<LoanType>.RandomItemFrom(loanTypes);
                        var dateIssued = DateTime.Now.AddDays(-randomNumber.Next(1, 90));
                        var dueDate = dateIssued.AddDays(loanType.Duration);
                        var dateReturned = randomNumber.Next(1, 100) > 50 ? dateIssued.AddDays(loanType.Duration + randomNumber.Next(-5, 10)) : DateTime.MinValue;
                        var penalty = dateReturned.Equals(DateTime.MinValue) ? 0 : (dateReturned < dueDate ? 0 : (dateReturned - dueDate).TotalDays * book.PenaltyCharge);

                        var validMember = false;
                        var member = Pick<Member>.RandomItemFrom(members);

                        do
                        {
                            if (DateTime.Now.AddYears(-18).CompareTo(member.DateOfBirth) == -1 && bookCopy.Book.Categories.Any(c => c.AgeRestricted == true))
                            {
                                member = Pick<Member>.RandomItemFrom(members);
                                continue;
                            }

                            validMember = true;
                        } while (!validMember);

                        var loan = new Loan
                        {
                            BookCopy = bookCopy,
                            Member = member,
                            IssuedOn = dateIssued,
                            DueDate = dueDate,
                            LoanCharge = loanType.Duration * book.Charge,
                            PenaltyCharge = penalty,
                            LoanType = loanType,
                            LoanedBy = context.Users.First()
                        };

                        if (!dateReturned.Equals(DateTime.MinValue))
                        {
                            loan.ReturnedOn = dateReturned;
                        }

                        bookCopy.Available = dateReturned != DateTime.MinValue;
                        loans.Add(loan);
                    }
                }

                context.Loans.AddOrUpdate(loans.ToArray());
                context.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                );
            }
        }
    }
}
