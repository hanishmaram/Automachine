using Algo.Context.Lib;
using FyersAPI;

//https://api.fyers.in/api/v2/generate-authcode?client_id=HDFO32LMYO-100&redirect_uri=http://127.0.0.1&response_type=code&state=568
string access_token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkuZnllcnMuaW4iLCJpYXQiOjE2NDkwNDEyMDEsImV4cCI6MTY0OTExODYwMSwibmJmIjoxNjQ5MDQxMjAxLCJhdWQiOlsieDowIiwieDoxIiwieDoyIiwiZDoxIiwiZDoyIiwieDoxIiwieDowIl0sInN1YiI6ImFjY2Vzc190b2tlbiIsImF0X2hhc2giOiJnQUFBQUFCaVNsOHhNZFVuM01RbXRibDlOSFN2elEzcGVxVklGeXZjbWRmdHh0d2dSMW13eVhJTFhPNVMwZUd0d1NUcllFNGw3cG9BY0Z3N25aNDF4QTFjVjVkZ0VlQjBQTnVVVXJyejZTU1JQaG1HVVFxdEZLbz0iLCJkaXNwbGF5X25hbWUiOiJQQVNVUFVMRVRJIFNBSSBQQUxMQVZJIiwiZnlfaWQiOiJYUDA5MzE0IiwiYXBwVHlwZSI6MTAwLCJwb2FfZmxhZyI6Ik4ifQ.XJuewIxyD4OLHay0bRuE0TfC30ioh6E9V17-uAZVVKA";
const string appId = "";
//const string appSecretId = "";
//const string url = "http://127.0.0.1/?s=ok&code=200&auth_code=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhcGkubG9naW4uZnllcnMuaW4iLCJpYXQiOjE2NDg2OTg3MjcsImV4cCI6MTY0ODcyODcyNywibmJmIjoxNjQ4Njk4MTI3LCJhdWQiOiJbXCJ4OjBcIiwgXCJ4OjFcIiwgXCJ4OjJcIiwgXCJkOjFcIiwgXCJkOjJcIiwgXCJ4OjFcIiwgXCJ4OjBcIl0iLCJzdWIiOiJhdXRoX2NvZGUiLCJkaXNwbGF5X25hbWUiOiJYUDA5MzE0Iiwibm9uY2UiOiIiLCJhcHBfaWQiOiJKRllUVEFUVjMwIiwidXVpZCI6IjdmYWU1NDk5MmQ4MDQ3NDg5ODhjZTdkYzgzMzQ4MjZiIiwiaXBBZGRyIjoiMC4wLjAuMCIsInNjb3BlIjoiIn0.EWyuH2KdmVBeCAPRBfwGd4nyhO1ifNDYoCw3JQcTNL8&state=568";
//const string symbolExpiry = "22331";
const string StratergyName = "1%_STRADDLE_BN";

 
Fyers fyers = new(appId);
using (var context = new AlgoContext())
{
    access_token = context.Users.Where(x => x.AppId.Equals(appId)).First().Token.ToString();

}
Add_AccessToken_Header();

while (true)
{
    try
    {
        //PositionResponse res112 = await fyers.GetPositionAsync();
        Console.ResetColor();
        Order order = null;

        using (var context = new AlgoContext())
        {
            order = context.Orders.Where(x => x.OrderDate.Equals(DateOnly.FromDateTime(DateTime.Now)) && x.Stratergy.Equals(StratergyName)).First();

        }

        var ltpBN = await GetBNQuoteData();

        if(TimeOnly.FromDateTime(DateTime.Now) > new TimeOnly(14, 59))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exceeeds the intraday time :: Exit positions current LTP : {ltpBN}");
            PositionResponse res = await fyers.GetPositionAsync();
            if (res?.netPositions?.Count > 0) {
                Console.Beep(5000, 5000);
                await fyers.ExitPositionAsync(new ExitPositionByIdPayload { id = res.netPositions[0].id });
                await fyers.ExitPositionAsync(new ExitPositionByIdPayload { id = res.netPositions[1].id });
            }
        }

        if (ltpBN > Convert.ToDouble(order.UpperLimit) || ltpBN < Convert.ToDouble(order.LowerLimit))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exit positions current LTP : {ltpBN}");
            PositionResponse res = await fyers.GetPositionAsync();
            if (res?.netPositions?.Count > 0)
            {
                Console.Beep(5000, 5000);
                await fyers.ExitPositionAsync(new ExitPositionByIdPayload { id = res.netPositions[0].id });
                await fyers.ExitPositionAsync(new ExitPositionByIdPayload { id = res.netPositions[1].id });
            }
        }
        Console.WriteLine($"Checked BN Quote LTP {ltpBN} with {order.LowerLimit} and {order.UpperLimit} Now: {DateTime.Now}");
        await Task.Delay(30000);
    }
    catch (Exception ex) 
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.ToString());
        await Task.Delay(3000);
    }
}
async Task<double> GetBNQuoteData()
{
    var quouteData = await fyers.GetQuotesAsync(new string[] { "NSE:NIFTYBANK-INDEX" });

    Console.WriteLine(quouteData.d[0].v.lp);
    return quouteData.d[0].v.lp;
}

void Add_AccessToken_Header()
{
    fyers.AddHttpHeaders(new Dictionary<string, string>() { { "Authorization", $"{appId}:{access_token}" } });
}

Console.ReadLine();
