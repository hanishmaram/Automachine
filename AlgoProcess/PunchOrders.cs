
using Algo.Context.Lib;
using Microsoft.EntityFrameworkCore;

using FyersAPI;
//https://api.fyers.in/api/v2/generate-authcode?client_id=JFYTTATV30-100&redirect_uri=http://127.0.0.1&response_type=code&state=568
string access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE2NDkwNDEyMDEsImV4cCI6MTY0OTExODYwMSwibmJmIjoxNjQ5MDQxMjAxLCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCaVNsOHhNZFVuM01RbXRibDlOSFN2elEzcGVxVklGeXZjbWRmdHh0d2dSMW13eVhJTFhPNVMwZUd0d1NUcllFNGw3cG9BY0Z3N25aNDF4QTFjVjVkZ0VlQjBQTnVVVXJyejZTU1JQaG1HVVFxdEZLbz0iLCJkaXNwbGF5X25hbWUiOiJQQVNVUFVMRVRJIFNBSSBQQUxMQVZJIiwiZnlfaWQiOiJYUDA5MzE0IiwiYXBwVHlwZSI6MTAwLCJwb2FfZmxhZyI6Ik4ifQ.XJuewIxyD4OLHay0bRuE0TfC30ioh6E9V17-uAZVVKA";
const string appId = "";
const string appSecretId = "";
const string url = "http://127.0.0.1/?s=ok&code=200&auth_code=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkubG9naW4uZnllcnMuaW4iLCJpYXQiOjE2NDkwNDExNzcsImV4cCI6MTY0OTA3MTE3NywibmJmIjoxNjQ5MDQwNTc3LCJhdWQiOiJbXCJ4OjBcIiwgXCJ4OjFcIiwgXCJ4OjJcIiwgXCJkOjFcIiwgXCJkOjJcIiwgXCJ4OjFcIiwgXCJ4OjBcIl0iLCJzdWIiOiJhdXRoX2NvZGUiLCJkaXNwbGF5X25hbWUiOiJYUDA5MzE0Iiwibm9uY2UiOiIiLCJhcHBfaWQiOiJKRllUVEFUVjMwIiwidXVpZCI6IjU3MzE4OWExOGVhNTQ2Nzg5MzUzZDQ1MDk2YzU0MWVlIiwiaXBBZGRyIjoiMC4wLjAuMCIsInNjb3BlIjoiIn0.ZMYHddBR3RMYwmrNRMuo-sP4TeRmmGwaQ8gI779ohGY&state=568";
const string symbolExpiry = "22421";
const string StratergyName = "1%_STRADDLE_BN";

Fyers fyers = new(appId);

using (var context = new AlgoContext())
{
    access_token = context.Users.Where(x=>x.AppId.Equals(appId)).First().Token.ToString();

}

// RUN BELOW CODE WHEN YOU WANT TO GENERATE ACCESS CODE
//await GenerateAccessToken();

// RUN BELOW LINE WHEN YOU WANT TO PLACE ORDERS
await PlaceATMShortStraddleOrder();

// RUN BELOW TO TEST BY CHECKING BN LTP
//Add_AccessToken_Header();
//await GetBNQuoteData();

//await GetPositions();
async Task GetPositions()
{
    Add_AccessToken_Header();
    PositionResponse res = await fyers.GetPositionAsync();
}

async Task PlaceATMShortStraddleOrder()
{
    try
    {
        Add_AccessToken_Header();
        double ltpBN = await GetBNQuoteData();
        int strickPrice = GetATMStrikePrice(ltpBN);

        await PlaceOrders(GetATMSymbols(strickPrice));

        using (var context = new AlgoContext())
        {
            Order obj = new Order();
            obj.EntryLTP = Convert.ToDecimal(ltpBN);
            obj.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            obj.Stratergy = StratergyName;
            obj.StrikePrice = strickPrice;
            obj.LowerLimit = obj.EntryLTP - (obj.EntryLTP * Convert.ToDecimal(1 / 100d));
            obj.UpperLimit = obj.EntryLTP + (obj.EntryLTP * Convert.ToDecimal(1 / 100d));
            var data = await context.Orders.AddAsync(obj);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void Add_AccessToken_Header()
{
    fyers.AddHttpHeaders(new Dictionary<string, string>() { { "Authorization", $"{appId}:{access_token}" } });
}

async Task PlaceOrder(string tradingSymbol)
{
    var response = await fyers.PlaceOrderAsync(new PlaceOrderPayload()
    {
        disclosedQty = 0,
        offlineOrder = "False",
        productType = ProductTypes.INTRADAY,
        qty = 25,
        takeProfit = 0.0m,
        side = OrderSide.Sell,
        stopLoss = 0.0m,
        stopPrice = 0.0m,
        symbol = tradingSymbol,
        type = (int)OrderType.Market,
        validity = OrderValidity.DAY
    });

    if (response != null && !string.IsNullOrEmpty(response.id))
        throw new Exception($"Failed to place order {tradingSymbol}");
}

async Task PlaceOrders(string[] symbols)
{
    //TODO: Find Some better way to loop async await
    //symbols.ToList<string>().ForEach(async x =>
    //{
    //    await PlaceOrder(x);
    //});

    var response = await fyers.PlaceOrderAsync(new PlaceOrderPayload()
    {
        disclosedQty = 0,
        offlineOrder = "False",
        productType = ProductTypes.INTRADAY,
        qty = 25,
        takeProfit = 0.0m,
        side = OrderSide.Sell,
        stopLoss = 0.0m,
        stopPrice = 0.0m,
        symbol = symbols[0],
        type = (int)OrderType.Market,
        validity = OrderValidity.DAY
    });
    Console.WriteLine(response.id);
    if (response == null || string.IsNullOrEmpty(response.id))
        throw new Exception($"Failed to place order {symbols[0]}");

    var response1 = await fyers.PlaceOrderAsync(new PlaceOrderPayload()
    {
        disclosedQty = 0,
        offlineOrder = "False",
        productType = ProductTypes.INTRADAY,
        qty = 25,
        takeProfit = 0.0m,
        side = OrderSide.Sell,
        stopLoss = 0.0m,
        stopPrice = 0.0m,
        symbol = symbols[1],
        type = (int)OrderType.Market,
        validity = OrderValidity.DAY
    });
    Console.WriteLine(response1.id);
    if (response1 == null || string.IsNullOrEmpty(response1.id))
        throw new Exception($"Failed to place order {symbols[1]}");
}


async Task<double> GetBNQuoteData()
{
    var quouteData = await fyers.GetQuotesAsync(new string[] { "NSE:NIFTYBANK-INDEX" });

    Console.WriteLine(quouteData.d[0].v.lp);
    return quouteData.d[0].v.lp;
}

int GetATMStrikePrice(double ltpPrice)
{
    return Convert.ToInt32(Math.Round(ltpPrice / 100, 0)) * 100;
}

string[] GetATMSymbols(int strikePrice)
{
    string symbol = $"NSE:BANKNIFTY{symbolExpiry}{strikePrice}";
    return new string[] { $"{symbol}CE", $"{symbol}PE" };
}

async Task GenerateAccessToken()
{
    TokenPayload outToken;

    Fyers.IsValidLogin(url, appId, appSecretId, out outToken);
    TokenResponse res = await fyers.GenerateTokenAsync(outToken);
    if(res is null)
    { 
        Console.ForegroundColor= ConsoleColor.Red;
        Console.WriteLine("Failed to generate access token");
        Console.Clear();
    }
    Console.WriteLine(res.access_token);
}


Console.ReadLine();
