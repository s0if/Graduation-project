using System;
using System.Threading.Tasks;
using RestSharp;

namespace Graduation.Service
{
    public class WhatsAppService
    {

        static public async Task<string> SendMessageAsync(string recipient, string message, string? image=null)
        {
            var url = "";
            if (image is not null)
            
                 url = "https://api.ultramsg.com/instance118183/messages/image";
            else    
           
             url = "https://api.ultramsg.com/instance118183/messages/chat";

            var client = new RestClient(url);

            var request = new RestRequest(url, Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("token", "ilerny73eemdua2c");
            request.AddParameter("to", recipient);
            if (image is  null)
                request.AddParameter("body", message);
            else
            {

                request.AddParameter("image",image);
                request.AddParameter("caption", message);
            }

            RestResponse response = await client.ExecuteAsync(request);
            return response.Content;

        }

    }
}
