using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreUI.Static
{
    public static class Endpoints
    {
        public static string BaseUrl = "https://localhost:44369/";
        public static string AuthorsEndpoint = $"{BaseUrl}api/author/";
        public static string BooksEndpoint = $"{BaseUrl}api/book/";
        public static string RegisterEndpoint = $"{BaseUrl}api/users/register/";
        public static string LoginEndpoint = $"{BaseUrl}api/users/login/";
    }
}
