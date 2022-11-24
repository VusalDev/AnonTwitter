using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace YETwitter.Identity.Web.Tests
{
    public class AuthControllerTests
    {
        protected WebApplicationFactory<Program> appFactory;

        public AuthControllerTests(ITestOutputHelper testOutput)
        {
            appFactory = new ApplicationFactory(testOutput)
                .WithWebHostBuilder(builder =>
                {
                    // ... Configure test services
                });

        }

        #region Register

        [Fact]
        public async Task TestRegister_Success()
        {
            var client = appFactory.CreateClient();
            var (_, json) = await this.RegisterUserAsync(client);

            var data = JsonConvert.DeserializeObject<ResponseModel>(json);

            Assert.Equal("Success", data.Status);
        }

        [Fact]
        public async Task TestRegister_Failed_Duplicate()
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = "test_usr_pass",
            };
            var response1 = await client.PostAsJsonAsync("api/v1/auth/register", body);
            response1.EnsureSuccessStatusCode();

            var response2 = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);

            var json = await response2.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.InternalServerError, data.Status);
            Assert.Equal("User creation failed", data.Title);
            Assert.Equal("User already exists", data.Detail);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")]
        public async Task TestRegister_Failed_NoPassword(string password)
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = password,
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Password", data.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1")]
        public async Task TestRegister_Failed_NoUsername(string username)
        {
            var client = appFactory.CreateClient();
            var body = new RegisterModel
            {
                Username = username,
                Password = "123456",
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Username", data.Errors);
        }

        #endregion

        #region Login

        [Theory]
        [InlineData(false, 120)]
        [InlineData(true, 365 * 24 * 60)]
        public async Task TestLogin_Success(bool rememeberMe, int ttlMinutes)
        {
            var client = appFactory.CreateClient();
            var (registerModel, _) = await this.RegisterUserAsync(client);

            var body = new LoginModel
            {
                Username = registerModel.Username,
                Password = registerModel.Password,
                RememberMe = rememeberMe,
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/login", body);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<TokenDataModel>(json);

            Assert.Equal(DateTime.UtcNow.AddMinutes(ttlMinutes), data.ValidTo, TimeSpan.FromSeconds(1)); //from appsettings
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")]
        public async Task TestLogin_Failed_NoPassword(string password)
        {
            var client = appFactory.CreateClient();
            var body = new LoginModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = password,
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/login", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Password", data.Errors);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1")]
        public async Task TestLogin_Failed_NoUsername(string username)
        {
            var client = appFactory.CreateClient();
            var body = new LoginModel
            {
                Username = username,
                Password = "123456",
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/login", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Username", data.Errors);
        }

        [Fact]
        public async Task TestLogin_FailedWrongPassword()
        {
            var client = appFactory.CreateClient();
            var (registerModel, _) = await this.RegisterUserAsync(client);

            var body = new LoginModel
            {
                Username = registerModel.Username,
                Password = "123456",
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/login", body);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.Unauthorized, data.Status);
            Assert.Equal("Unauthorized", data.Title);
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task TestChangePassword_Success()
        {
            var client = appFactory.CreateClient();
            var registerModel = await this.RegisterAndLoginUserAsync(client);

            var newPass = "123456";
            var body = new ChangePasswordModel
            {
                Password = registerModel.Password,
                NewPassword = newPass
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/change-password", body);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ResponseModel>(json);
            Assert.Equal("Success", data.Status);

            var client2 = appFactory.CreateClient();
            var login = new LoginModel
            {
                Username = registerModel.Username,
                Password = newPass
            };

            var loginResponse = await client2.PostAsJsonAsync("api/v1/auth/login", login);
            loginResponse.EnsureSuccessStatusCode();
            var loginData = JsonConvert.DeserializeObject<TokenDataModel>(await loginResponse.Content.ReadAsStringAsync());
            Assert.NotNull(loginData?.Token);
        }

        [Fact]
        public async Task TestChangePassword_FailedWrongPassword()
        {
            var client = appFactory.CreateClient();
            await this.RegisterAndLoginUserAsync(client);

            var body = new ChangePasswordModel
            {
                Password = "123456",
                NewPassword = "123456"
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/change-password", body);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.InternalServerError, data.Status);
            Assert.Equal("Password changing failed", data.Title);
            Assert.Equal("Incorrect password.", data.Detail);
        }

        [Theory]
        [InlineData(null,  null)]
        [InlineData("", "")]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, "12345")]
        [InlineData("12345", null)]
        [InlineData("12345", "12345")]
        public async Task TestChangePassword_Failed_NoPassword(string password, string newPassword)
        {
            var client = appFactory.CreateClient();
            await this.RegisterAndLoginUserAsync(client);

            var body = new ChangePasswordModel
            {
                Password = password,
                NewPassword = newPassword
            };

            var response = await client.PostAsJsonAsync("api/v1/auth/change-password", body);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ValidationProblemDetails>(json);

            Assert.Equal((int)HttpStatusCode.BadRequest, data.Status);
            Assert.Equal("One or more validation errors occurred.", data.Title);
            Assert.Contains("Password", data.Errors);
        }

        #endregion

        private async Task<(RegisterModel, string)> RegisterUserAsync(HttpClient client)
        {
            var body = new RegisterModel
            {
                Username = $"test_usr_{Guid.NewGuid().ToString("N")}",
                Password = "test_usr_pass",
            };
            var response = await client.PostAsJsonAsync("api/v1/auth/register", body);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return (body, json);
        }

        private async Task<RegisterModel> RegisterAndLoginUserAsync(HttpClient client)
        {
            var (registerModel, _) = await this.RegisterUserAsync(client);

            var login = new LoginModel
            {
                Username = registerModel.Username,
                Password = registerModel.Password,
            };

            var loginResponse = await client.PostAsJsonAsync("api/v1/auth/login", login);
            loginResponse.EnsureSuccessStatusCode();
            var loginData = JsonConvert.DeserializeObject<TokenDataModel>(await loginResponse.Content.ReadAsStringAsync());

            client.DefaultRequestHeaders.Authorization
                = new System.Net.Http.Headers.AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, loginData.Token);

            return registerModel;
        }
    }
}