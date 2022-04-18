
using Algo.Context.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Security.Cryptography;
using System.Text;
using System.Web;

string loginUrl = "https://api.fyers.in/api/v2/generate-authcode?client_id=JFYTTATV30-100&redirect_uri=http://127.0.0.1&response_type=code&state=568";
const string appId = "";
const string appSecretId = "";
string clientId = "";
string password = "";


ChromeOptions options = new();
options.AddArgument("--disable-gpu");
options.AddArgument("--no-sandbox");
//options.AddArgument("--headless");

IWebDriver driver = new ChromeDriver(options);
driver.Navigate().GoToUrl(loginUrl);

driver.FindElement(By.XPath("//*[@id=\"fy_client_id\"]")).SendKeys(clientId);
driver.FindElement(By.XPath("//*[@id=\"clientIdSubmit\"]")).Click();
Thread.Sleep(1000);
driver.FindElement(By.XPath("//*[@id=\"fy_client_pwd\"]")).SendKeys(password);
driver.FindElement(By.XPath("//*[@id=\"loginSubmit\"]")).Click();
Thread.Sleep(1000);


driver.FindElement(By.XPath("//div[@id=\"pin-container\"]/input[@id=\"first\"]")).SendKeys("9");
driver.FindElement(By.XPath("//div[@id=\"pin-container\"]/input[@id=\"second\"]")).SendKeys("5");
driver.FindElement(By.XPath("//div[@id=\"pin-container\"]/input[@id=\"third\"]")).SendKeys("3");
driver.FindElement(By.XPath("//div[@id=\"pin-container\"]/input[@id=\"fourth\"]")).SendKeys("8");

driver.FindElement(By.XPath("//*[@id=\"verifyPinSubmit\"]")).Click();

Thread.Sleep(1000);

string currentUrl = driver.Url;
driver.Close();

string authCode = HttpUtility.ParseQueryString(new Uri(currentUrl).Query)?["auth_code"];

string appIdHash = String.Empty;
using (SHA256 sha256Hash = SHA256.Create())
{
    // ComputeHash - returns byte array
    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes($"{appId}:{appSecretId}"));

    // Convert byte array to a string
    StringBuilder builder = new StringBuilder();
    for (int i = 0; i < bytes.Length; i++)
    {
        builder.Append(bytes[i].ToString("x2"));
    }
    appIdHash = builder.ToString();
}

HttpClient httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

var postData = new
{
    grant_type = "authorization_code",
    appIdHash = appIdHash,
    code = authCode
};

JsonSerializerSettings settings = new JsonSerializerSettings()
{
    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
    DateTimeZoneHandling = DateTimeZoneHandling.Utc
};

HttpContent content = new StringContent(JsonConvert.SerializeObject(postData, settings), Encoding.UTF8, "application/json");

var response = await httpClient.PostAsync("https://api.fyers.in/api/v2/validate-authcode", content);

var accessToken = JObject.Parse(await response.Content.ReadAsStringAsync())["access_token"];

Console.WriteLine(accessToken);

using (var context = new AlgoContext())
{
    var user = context.Users.Where(x => x.AppId.Equals(appId)).First();
    user.Token = accessToken.ToString();
    await context.SaveChangesAsync();

}

Console.WriteLine("Saved success");

Console.WriteLine("");
