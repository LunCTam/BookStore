using Blazored.LocalStorage;
using BookStoreUI.Contracts;
using BookStoreUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStoreUI.Services
{
    public class AuthorRepository : BaseRepository<Author>, IAuthorRepository
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _localStorage;

        public AuthorRepository(IHttpClientFactory client, 
            ILocalStorageService localStorage) : base(client, localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }

    }
}
