using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Itelligent.Web.Ui.Configuration;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using opra.itelligent.es.Hubs;
using opra.itelligent.es.Models;
using opra.itelligent.es.Services;
using opra.itelligent.es.ViewModels;

namespace opra.itelligent.es
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 443;
            });

            services.AddControllersWithViews()
                .AddNewtonsoftJson()
                .AddRazorRuntimeCompilation();

            services.AddITelligentUI(options =>
            {
                options.ImageNavbar = "/images/logo_old.PNG";
            }).AddMenuService<MenuService>();

            services.AddHttpContextAccessor();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = Configuration["IdentityServer:Authority"];
                options.RequireHttpsMetadata = false;

                options.ClientId = Configuration["IdentityServer:ClientId"];
                options.ClientSecret = Configuration["IdentityServer:Secret"];
                options.ResponseType = "code id_token";

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                //Aquí se indican los scope solicitados, por ejemplo
                options.Scope.Add("openid");
                options.Scope.Add("profile");

                options.ClaimActions.MapJsonKey("role", "role", "role");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

            services.AddDbContext<BdOpraContext>(options => options.UseSqlServer(Configuration.GetConnectionString("bdOpraConnection")));

            services.AddOData();

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();

            services.AddMemoryCache();

            services.AddSignalR()
                .AddNewtonsoftJsonProtocol();

            services.AddTransient<ProblemasService>();
            services.AddScoped<VehicleRoutingService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.Select().Expand().Filter().OrderBy().MaxTop(null).Count();
                endpoints.MapODataRoute("odata", "odata", GetEdmModel());
                endpoints.MapHub<SchedulerHub>("/schedulerHub");
            });
        }

        public IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.ComplexType<PointData>();

            builder.EntitySet<TblMaestraProblema>("MaestraProblemas").EntityType.HasKey(x => x.IntId);
            var problemasTSP = builder.EntitySet<TblMaestraProblemaTsp>("MaestraProblemasTSP").EntityType.HasKey(x => x.IntId);
            problemasTSP.Function("CoordenadasOptimas").ReturnsCollection<PointData>();

            builder.EntitySet<TblSolucionTsp>("SolucionesTSP").EntityType.HasKey(x => x.IntId);

            builder.EntitySet<TblSolucion>("Soluciones").EntityType.HasKey(x => x.IntId);
            builder.EntitySet<TblEjecucion>("Ejecucions").EntityType.HasKey(x => x.IntId);

            builder.EntitySet<TblModeloTsp>("ModelosTSP").EntityType.HasKey(x => x.IntId);

            return builder.GetEdmModel();
        }
    }
}
