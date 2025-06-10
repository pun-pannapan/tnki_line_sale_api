namespace tnki_line_sale_api.Models
{
    public class AuthenticateResponse
    {
        public string Token { get; set; }


        public AuthenticateResponse(string token)
        {
            Token = token;
        }
    }
    public class CheckOTP_ResponseResult
    {
        public bool isValid { get; set; }
        public string resultDesc { get; set; }
        public bool isExisted { get; set; }
    }
    public class APIAuthen_ResponseResult
    {
        public Guid custGuid { get; set; }
        public bool isExisted { get; set; }
        public String Token { get; set; }
        public String customerName { get; set; }
        public String custTel { get; set; }

        //adding linePic
        public String linePic { get; set; }
    }

    public class Login_ResponseResult
    {
        public String Token { get; set; }
        public String UserName { get; set; }
        public String Name { get; set; }
        public String Role { get; set; }
    }

}
