using BookStoreUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreUI.Contracts
{
    public interface IBookRepository : IBaseRepository<Book>
    {
    }
}
