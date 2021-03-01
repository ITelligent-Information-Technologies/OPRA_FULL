using Itelligent.Web.Ui.Models;
using Itelligent.Web.Ui.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace opra.itelligent.es.Services
{
    public class MenuService : IMenuService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuService(IHttpContextAccessor httpContextAcessor)
        {
            _httpContextAccessor = httpContextAcessor;
        }

        public MenuItem GetHome()
        {
            return new MenuItem("Inicio", "Home", "Index");
        }

        public IEnumerable<MenuItem> GetMenu()
        {
            List<MenuItem> model = new List<MenuItem>
            {
                new MenuItem("Inicio", "Home", "Index"),
                new MenuItem("Noticias", "News", "Index"),
                new MenuItem("Repositorio", urlExterna: "https://github.com/ITelligent-Information-Technologies/opra"),
                new MenuItem("Área privada", "PrivateArea", "Index")
                {
                    HtmlExtra = async () =>
                    {
                        await Task.Yield();
                        return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated ? "" : "<i class='fas fa-lock ml-2'></i>";
                    }
                }
            };

            return model;
        }
    }
}
