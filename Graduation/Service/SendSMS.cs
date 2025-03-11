using Azure.Communication.Sms;
using System;
using System.Threading.Tasks;

namespace Graduation.Service
{
    public class SendSMS
    {
        private string connectionString = "endpoint=https://graduationsms.unitedstates.communication.azure.com/;accesskey=2d53ekYwRJFfWuuZv8MFi3M1qYutA4svy3j5SahZfZFauaTrJF5aJQQJ99BCACULyCpFGm8qAAAAAZCSXsF6";

        public async Task SendMessage(string number, string code)
        {
            try
            {
                // إنشاء عميل SMS
                SmsClient smsClient = new SmsClient(connectionString);

                // رقم المرسل (يجب أن يكون مدعومًا من Azure)
                string fromNumber = "+972595203640";

                // رقم المستلم
                string toNumber = number;

                // نص الرسالة
                string messageText = $"hello {code}";

                // إرسال الرسالة
                SmsSendResult sendResult = await smsClient.SendAsync(
                    from: fromNumber,
                    to: toNumber,
                    message: messageText
                );

                // طباعة نتيجة الإرسال
                Console.WriteLine($"تم إرسال الرسالة بنجاح! ID: {sendResult.MessageId}");
            }
            catch (Exception ex)
            {
                // طباعة الخطأ إذا حدثت مشكلة
                Console.WriteLine($"حدث خطأ أثناء إرسال الرسالة: {ex.Message}");
            }
        }
    }
}